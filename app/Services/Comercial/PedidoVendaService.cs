using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.Models.Producao.Enums;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço do Pedido de Venda.
/// Gerencia CRUD, transições de status, histórico de eventos, e chamada única (Create/Update)
/// que cria/atualiza o PV + itens de forma atômica (transação única).
///
/// Fluxo depende do Tipo:
///   Normal   → Liberado → Andamento → Concluido → AEntregar → Entregue
///   PreVenda → AguardandoNS → RecebidoNS → AguardandoRetorno → Liberado → (fluxo Normal)
/// Status especiais: Pausado, Cancelado, Reaberto, Devolvido.
///
/// Integrações automáticas:
/// - PV → Liberado: notifica módulo Producao
/// - PV → Entregue: bloqueia se existe OP Master não finalizada
/// - PV → Cancelado/Pausado: notifica Producao (com OPs ativas)
/// - Itens alterados em status avançado: registra evento ItensAlterados + notifica Eng/Prod/Almox
/// </summary>
public class PedidoVendaService(AppDbContext context, NotificacaoService notificacoes)
{
    private readonly AppDbContext _context = context;
    private readonly NotificacaoService _notificacoes = notificacoes;

    // ========================================================================
    // LISTAGEM
    // ========================================================================

    public async Task<List<PedidoVendaResponseDTO>> GetAll(int pagina = 0, int tamanho = 0)
    {
        var query = _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .OrderByDescending(p => p.Data);

        List<PedidoVenda> pedidos;

        if (pagina > 0 && tamanho > 0)
            pedidos = await query.Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        else
            pedidos = await query.ToListAsync();

        var resultado = new List<PedidoVendaResponseDTO>();

        foreach (var p in pedidos)
        {
            var itens = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == p.Id)
                .OrderBy(i => i.Id)
                .ToListAsync();

            resultado.Add(ToResponseDTO(p, itens));
        }

        return resultado;
    }

    public async Task<PedidoVendaResponseDTO?> GetById(int id)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente)
                .ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null) return null;

        var itens = await _context.PedidosVendaItens
            .Where(i => i.PedidoVendaId == id)
            .OrderBy(i => i.Id)
            .ToListAsync();

        return ToResponseDTO(pedido, itens);
    }

    // ========================================================================
    // CREATE — atômico, PV + itens numa transação
    // ========================================================================

    /// <summary>
    /// Cria PV com itens em uma única chamada atômica.
    /// OBRIGATÓRIO: lista de itens com pelo menos 1 item.
    /// Se qualquer validação falhar, transação é revertida (PV não fica criado).
    /// </summary>
    public async Task<(PedidoVendaResponseDTO? Item, string? Erro)> Create(PedidoVendaCreateDTO dto)
    {
        // Validação 1: itens obrigatórios
        if (dto.Itens == null || dto.Itens.Count == 0)
            return (null, "Pedido de venda deve ter ao menos um item.");

        // Validação 2: cada item precisa ter dados válidos
        for (int i = 0; i < dto.Itens.Count; i++)
        {
            var item = dto.Itens[i];
            if (item.Quantidade <= 0)
                return (null, $"Item {i + 1}: quantidade deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(item.Descricao))
                return (null, $"Item {i + 1}: descrição é obrigatória.");
        }

        // Validação 3: cliente existe
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

        if (cliente == null)
            return (null, "Cliente não encontrado");

        // Status inicial por tipo
        var statusInicial = dto.Tipo == TipoPedidoVenda.PreVenda
            ? StatusPedidoVenda.AguardandoNS
            : StatusPedidoVenda.Liberado;

        var codigo = await GerarCodigo();
        var agora = DateTime.UtcNow;

        // Transação: PV + itens + histórico, tudo junto ou nada
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var pedido = new PedidoVenda
            {
                Codigo = codigo,
                ClienteId = dto.ClienteId,
                Tipo = dto.Tipo,
                Status = statusInicial,
                Data = dto.Data ?? agora,
                DataEntrega = dto.DataEntrega,
                Observacoes = dto.Observacoes,
                CriadoEm = agora
            };

            _context.PedidosVenda.Add(pedido);
            await _context.SaveChangesAsync();

            // Cria itens
            foreach (var itemDto in dto.Itens)
            {
                _context.PedidosVendaItens.Add(new PedidoVendaItem
                {
                    PedidoVendaId = pedido.Id,
                    Quantidade = itemDto.Quantidade,
                    Descricao = itemDto.Descricao.Trim(),
                    Observacao = string.IsNullOrWhiteSpace(itemDto.Observacao) ? null : itemDto.Observacao.Trim(),
                    CriadoEm = agora
                });
            }

            // Evento inicial no histórico
            RegistrarEvento(pedido.Id, EventoPedidoVenda.Criado, null, statusInicial, null, null);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            pedido.Cliente = cliente;

            // Notifica Producao se já nasceu Liberado (Normal)
            if (statusInicial == StatusPedidoVenda.Liberado)
                await NotificarProducaoPvLiberado(pedido);

            // Recarrega com itens pra retornar
            var itensCriados = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == pedido.Id)
                .OrderBy(i => i.Id)
                .ToListAsync();

            return (ToResponseDTO(pedido, itensCriados), null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (null, $"Erro ao criar pedido: {ex.Message}");
        }
    }

    // ========================================================================
    // UPDATE — atômico, cabeçalho + diff de itens numa transação
    // ========================================================================

    /// <summary>
    /// Atualiza PV + itens em uma única chamada com diff sincronizado.
    /// Itens com Id preenchido: atualiza. Sem Id: cria. Ausentes da lista: deleta.
    ///
    /// Regras de status:
    /// - Status iniciais (AguardandoNS/RecebidoNS/AguardandoRetorno/Liberado): livre, sem justificativa
    /// - Status avançados (Andamento/Concluido/AEntregar/Pausado): justificativa OBRIGATÓRIA,
    ///   gera evento ItensAlterados no histórico e notifica Engenharia/Produção/Almoxarifado
    /// - Status terminais (Entregue/Devolvido/Cancelado/Reaberto): bloqueado
    /// </summary>
    public async Task<(PedidoVendaResponseDTO? Item, string? Erro)> Update(int id, PedidoVendaUpdateDTO dto)
    {
        // Validação 1: itens obrigatórios
        if (dto.Itens == null || dto.Itens.Count == 0)
            return (null, "Pedido de venda deve ter ao menos um item.");

        // Validação 2: cada item
        for (int i = 0; i < dto.Itens.Count; i++)
        {
            var it = dto.Itens[i];
            if (it.Quantidade <= 0)
                return (null, $"Item {i + 1}: quantidade deve ser maior que zero.");
            if (string.IsNullOrWhiteSpace(it.Descricao))
                return (null, $"Item {i + 1}: descrição é obrigatória.");
        }

        // Busca PV
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return (null, null);

        // Validação 3: status permite edição
        if (!StatusPermiteEdicao(pedido.Status) && !StatusPermiteEdicaoComJustificativa(pedido.Status))
            return (null, $"Pedido com status {pedido.Status} não aceita edição.");

        // Validação 4: justificativa obrigatória em status avançado
        var statusAvancado = StatusPermiteEdicaoComJustificativa(pedido.Status);
        var justificativa = dto.Justificativa?.Trim();

        if (statusAvancado && string.IsNullOrWhiteSpace(justificativa))
            return (null, $"Justificativa é obrigatória para editar pedido em status {pedido.Status}.");

        // Validação 5: cliente existe
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

        if (cliente == null)
            return (null, "Cliente não encontrado");

        // Transação
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Cabeçalho
            pedido.ClienteId = dto.ClienteId;
            pedido.Tipo = dto.Tipo;
            pedido.Data = dto.Data ?? pedido.Data;
            pedido.DataEntrega = dto.DataEntrega;
            pedido.Observacoes = dto.Observacoes;
            pedido.ModificadoEm = DateTime.UtcNow;

            // Diff de itens
            var itensExistentes = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == id)
                .ToListAsync();

            int qtdAdicionados = 0;
            int qtdRemovidos = 0;
            int qtdAlterados = 0;

            // IDs no payload (itens com Id válido)
            var idsNoPayload = dto.Itens
                .Where(x => x.Id.HasValue && x.Id.Value > 0)
                .Select(x => x.Id!.Value)
                .ToHashSet();

            // Remove os que não estão no payload
            foreach (var existente in itensExistentes)
            {
                if (!idsNoPayload.Contains(existente.Id))
                {
                    _context.PedidosVendaItens.Remove(existente);
                    qtdRemovidos++;
                }
            }

            // Processa payload: update ou insert
            foreach (var itemDto in dto.Itens)
            {
                if (itemDto.Id.HasValue && itemDto.Id.Value > 0)
                {
                    var existente = itensExistentes.FirstOrDefault(x => x.Id == itemDto.Id.Value);
                    if (existente == null)
                        return (null, $"Item {itemDto.Id} não pertence a este pedido.");

                    var mudou = existente.Quantidade != itemDto.Quantidade
                             || existente.Descricao != itemDto.Descricao.Trim()
                             || (existente.Observacao ?? "") != (itemDto.Observacao?.Trim() ?? "");

                    if (mudou)
                    {
                        existente.Quantidade = itemDto.Quantidade;
                        existente.Descricao = itemDto.Descricao.Trim();
                        existente.Observacao = string.IsNullOrWhiteSpace(itemDto.Observacao) ? null : itemDto.Observacao.Trim();
                        existente.ModificadoEm = DateTime.UtcNow;
                        qtdAlterados++;
                    }
                }
                else
                {
                    _context.PedidosVendaItens.Add(new PedidoVendaItem
                    {
                        PedidoVendaId = id,
                        Quantidade = itemDto.Quantidade,
                        Descricao = itemDto.Descricao.Trim(),
                        Observacao = string.IsNullOrWhiteSpace(itemDto.Observacao) ? null : itemDto.Observacao.Trim(),
                        CriadoEm = DateTime.UtcNow
                    });
                    qtdAdicionados++;
                }
            }

            bool houveMudancaItens = qtdAdicionados > 0 || qtdRemovidos > 0 || qtdAlterados > 0;

            // Evento + notificação em status avançado
            if (statusAvancado && houveMudancaItens)
            {
                var detalhe = $"+{qtdAdicionados} adicionados, -{qtdRemovidos} removidos, {qtdAlterados} alterados";
                RegistrarEvento(id, EventoPedidoVenda.ItensAlterados, pedido.Status, pedido.Status, justificativa, detalhe);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Notificações multi-setor em status avançado
            if (statusAvancado && houveMudancaItens)
                await NotificarEdicaoItensStatusAvancado(pedido, justificativa!, pedido.Status);

            // Carrega resposta
            var itensAtualizados = await _context.PedidosVendaItens
                .Where(i => i.PedidoVendaId == id)
                .OrderBy(i => i.Id)
                .ToListAsync();

            pedido.Cliente = cliente;
            return (ToResponseDTO(pedido, itensAtualizados), null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (null, $"Erro ao atualizar pedido: {ex.Message}");
        }
    }

    // ========================================================================
    // ALTERAR STATUS
    // ========================================================================

    /// <summary>
    /// Altera status do PV com validações + integração com Produção:
    /// - Bloqueia Entregue se existe OP Master em Pendente/Andamento/Pausada.
    /// - Notifica Producao em Liberado, Cancelado e Pausado.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, StatusPedidoVendaDTO dto)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return (false, null);

        var statusAnterior = pedido.Status;
        var novoStatus = dto.NovoStatus;

        if (statusAnterior == novoStatus)
            return (false, $"O pedido já está com status {novoStatus}");

        var (transicaoValida, erroTransicao) = TransicaoPermitida(statusAnterior, novoStatus);
        if (!transicaoValida)
            return (false, erroTransicao);

        // Bloqueio: Entregue só se todas OPs Master finalizadas
        if (novoStatus == StatusPedidoVenda.Entregue)
        {
            var pendentes = await _context.OrdensProducao
                .Where(o => o.PedidoVendaId == id
                         && o.OrdemPaiId == null
                         && o.Status != StatusOrdemProducao.Concluida
                         && o.Status != StatusOrdemProducao.Cancelada)
                .CountAsync();

            if (pendentes > 0)
                return (false, $"Não é possível marcar como Entregue: existem {pendentes} OP(s) Master não finalizada(s)");
        }

        var exigeJustificativa = ExigeJustificativa(statusAnterior, novoStatus);
        var justificativa = dto.Justificativa?.Trim();

        if (exigeJustificativa && string.IsNullOrWhiteSpace(justificativa))
            return (false, "Justificativa é obrigatória para esta transição de status");

        pedido.Status = novoStatus;
        pedido.ModificadoEm = DateTime.UtcNow;

        var evento = MapearEvento(statusAnterior, novoStatus);
        RegistrarEvento(id, evento, statusAnterior, novoStatus, justificativa, null);

        await _context.SaveChangesAsync();

        if (novoStatus == StatusPedidoVenda.Liberado)
            await NotificarProducaoPvLiberado(pedido);
        else if (novoStatus == StatusPedidoVenda.Cancelado)
            await NotificarProducaoPvCancelado(pedido, justificativa);
        else if (novoStatus == StatusPedidoVenda.Pausado)
            await NotificarProducaoPvPausado(pedido, justificativa);

        return (true, null);
    }

    // ========================================================================
    // DELETE
    // ========================================================================

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        if (pedido.Status != StatusPedidoVenda.AguardandoNS && pedido.Status != StatusPedidoVenda.Liberado)
            return (false, "Só é possível excluir PV nos status iniciais (AguardandoNS ou Liberado)");

        var temNS = await _context.NumerosSerie.AnyAsync(n => n.PedidoVendaId == id);
        if (temNS)
            return (false, "Não é possível excluir pedido com Número de Série vinculado");

        var temOp = await _context.OrdensProducao.AnyAsync(o => o.PedidoVendaId == id);
        if (temOp)
            return (false, "Não é possível excluir pedido com Ordens de Produção vinculadas");

        var itens = await _context.PedidosVendaItens.Where(i => i.PedidoVendaId == id).ToListAsync();
        _context.PedidosVendaItens.RemoveRange(itens);

        var historico = await _context.PedidoVendaHistorico.Where(h => h.PedidoVendaId == id).ToListAsync();
        _context.PedidoVendaHistorico.RemoveRange(historico);

        _context.PedidosVenda.Remove(pedido);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<List<PedidoVendaHistoricoResponseDTO>> GetHistorico(int pedidoVendaId)
    {
        return await _context.PedidoVendaHistorico
            .Where(h => h.PedidoVendaId == pedidoVendaId)
            .OrderByDescending(h => h.DataHora)
            .Select(h => new PedidoVendaHistoricoResponseDTO
            {
                Id = h.Id,
                PedidoVendaId = h.PedidoVendaId,
                Evento = h.Evento,
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                Justificativa = h.Justificativa,
                DataHora = h.DataHora
            })
            .ToListAsync();
    }

    // ========================================================================
    // NOTIFICAÇÕES
    // ========================================================================

    private async Task NotificarProducaoPvLiberado(PedidoVenda pv)
    {
        await _notificacoes.Create(new NotificacaoCreateDTO
        {
            ModuloDestino = ModuloSistema.Producao,
            Tipo = TipoNotificacao.Info,
            Titulo = $"PV {pv.Codigo} liberado para produção",
            Mensagem = $"Cliente: {pv.Cliente?.Pessoa?.Nome ?? "?"}. Programar OPs.",
            OrigemTabela = "Comercial_PedidosVenda",
            OrigemId = pv.Id
        });
    }

    private async Task NotificarProducaoPvCancelado(PedidoVenda pv, string? justificativa)
    {
        var temOpAtiva = await _context.OrdensProducao
            .AnyAsync(o => o.PedidoVendaId == pv.Id
                        && o.Status != StatusOrdemProducao.Concluida
                        && o.Status != StatusOrdemProducao.Cancelada);

        if (!temOpAtiva) return;

        await _notificacoes.Create(new NotificacaoCreateDTO
        {
            ModuloDestino = ModuloSistema.Producao,
            Tipo = TipoNotificacao.Aviso,
            Titulo = $"PV {pv.Codigo} cancelado — há OPs ativas",
            Mensagem = $"Motivo: {justificativa ?? "(sem justificativa)"}. Avaliar cancelamento das OPs.",
            OrigemTabela = "Comercial_PedidosVenda",
            OrigemId = pv.Id
        });
    }

    private async Task NotificarProducaoPvPausado(PedidoVenda pv, string? justificativa)
    {
        var temOpAtiva = await _context.OrdensProducao
            .AnyAsync(o => o.PedidoVendaId == pv.Id
                        && o.Status != StatusOrdemProducao.Concluida
                        && o.Status != StatusOrdemProducao.Cancelada);

        if (!temOpAtiva) return;

        await _notificacoes.Create(new NotificacaoCreateDTO
        {
            ModuloDestino = ModuloSistema.Producao,
            Tipo = TipoNotificacao.Aviso,
            Titulo = $"PV {pv.Codigo} pausado — há OPs ativas",
            Mensagem = $"Motivo: {justificativa ?? "(sem justificativa)"}. Avaliar pausa das OPs.",
            OrigemTabela = "Comercial_PedidosVenda",
            OrigemId = pv.Id
        });
    }

    /// <summary>
    /// Notifica Engenharia, Produção e Almoxarifado sobre edição de itens em status avançado.
    /// Almoxarifado só é notificado se status é Andamento ou Pausado (ainda tem impacto em materiais).
    /// </summary>
    private async Task NotificarEdicaoItensStatusAvancado(PedidoVenda pv, string justificativa, StatusPedidoVenda status)
    {
        var clienteNome = pv.Cliente?.Pessoa?.Nome ?? "?";

        await _notificacoes.Create(new NotificacaoCreateDTO
        {
            ModuloDestino = ModuloSistema.Engenharia,
            Tipo = TipoNotificacao.Aviso,
            Titulo = $"PV {pv.Codigo} teve itens alterados",
            Mensagem = $"Cliente: {clienteNome}. Motivo: {justificativa}. Avaliar impacto em BOM/estrutura.",
            OrigemTabela = "Comercial_PedidosVenda",
            OrigemId = pv.Id
        });

        await _notificacoes.Create(new NotificacaoCreateDTO
        {
            ModuloDestino = ModuloSistema.Producao,
            Tipo = TipoNotificacao.Aviso,
            Titulo = $"PV {pv.Codigo} teve itens alterados",
            Mensagem = $"Cliente: {clienteNome}. Motivo: {justificativa}. Avaliar impacto em OPs.",
            OrigemTabela = "Comercial_PedidosVenda",
            OrigemId = pv.Id
        });

        if (status == StatusPedidoVenda.Andamento || status == StatusPedidoVenda.Pausado)
        {
            await _notificacoes.Create(new NotificacaoCreateDTO
            {
                ModuloDestino = ModuloSistema.Almoxarifado,
                Tipo = TipoNotificacao.Aviso,
                Titulo = $"PV {pv.Codigo} teve itens alterados",
                Mensagem = $"Cliente: {clienteNome}. Motivo: {justificativa}. Avaliar impacto em materiais reservados.",
                OrigemTabela = "Comercial_PedidosVenda",
                OrigemId = pv.Id
            });
        }
    }

    // ========================================================================
    // PRIVATES
    // ========================================================================

    private async Task<string> GerarCodigo()
    {
        var agora = DateTime.UtcNow;
        var prefixo = $"PV.{agora:yyyy}.{agora:MM}";

        var ultimo = await _context.PedidosVenda
            .Where(p => p.Codigo.StartsWith(prefixo))
            .OrderByDescending(p => p.Codigo)
            .FirstOrDefaultAsync();

        var sequencial = 1;

        if (ultimo != null)
        {
            var partes = ultimo.Codigo.Split('.');
            if (partes.Length == 4 && int.TryParse(partes[3], out var num))
                sequencial = num + 1;
        }

        return $"{prefixo}.{sequencial:D4}";
    }

    private static (bool Valida, string? Erro) TransicaoPermitida(StatusPedidoVenda atual, StatusPedidoVenda novo)
    {
        if (atual == StatusPedidoVenda.Entregue && novo != StatusPedidoVenda.Devolvido)
            return (false, "PV Entregue só pode ir para Devolvido");

        if (atual == StatusPedidoVenda.Cancelado && novo != StatusPedidoVenda.Reaberto)
            return (false, "PV Cancelado só pode ser movido para Reaberto");

        if (novo == StatusPedidoVenda.Devolvido && atual != StatusPedidoVenda.Entregue)
            return (false, "Devolvido só pode ser aplicado a PV Entregue");

        if (novo == StatusPedidoVenda.Reaberto && atual != StatusPedidoVenda.Cancelado)
            return (false, "Reaberto só pode ser aplicado a PV Cancelado");

        return (true, null);
    }

    private static int NivelFluxo(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.AguardandoNS       => 0,
        StatusPedidoVenda.RecebidoNS         => 1,
        StatusPedidoVenda.AguardandoRetorno  => 2,
        StatusPedidoVenda.Liberado           => 3,
        StatusPedidoVenda.Andamento          => 4,
        StatusPedidoVenda.Concluido          => 5,
        StatusPedidoVenda.AEntregar          => 6,
        StatusPedidoVenda.Entregue           => 7,
        _ => -1
    };

    private static bool ExigeJustificativa(StatusPedidoVenda atual, StatusPedidoVenda novo)
    {
        if (novo == StatusPedidoVenda.Pausado) return true;
        if (novo == StatusPedidoVenda.Cancelado) return true;
        if (novo == StatusPedidoVenda.Reaberto) return true;
        if (novo == StatusPedidoVenda.Devolvido) return true;

        if (EhStatusDeFluxo(atual) && EhStatusDeFluxo(novo) && NivelFluxo(novo) < NivelFluxo(atual))
            return true;

        return false;
    }

    private static bool EhStatusDeFluxo(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.Pausado    => false,
        StatusPedidoVenda.Cancelado  => false,
        StatusPedidoVenda.Reaberto   => false,
        StatusPedidoVenda.Devolvido  => false,
        _ => true
    };

    /// <summary>
    /// Status iniciais — edição livre, sem justificativa, sem notificação.
    /// </summary>
    public static bool StatusPermiteEdicao(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.AguardandoNS      => true,
        StatusPedidoVenda.RecebidoNS        => true,
        StatusPedidoVenda.AguardandoRetorno => true,
        StatusPedidoVenda.Liberado          => true,
        _ => false
    };

    /// <summary>
    /// Status avançados — edição permitida COM justificativa obrigatória + registra no histórico + notifica.
    /// Entregue/Devolvido/Cancelado/Reaberto continuam BLOQUEADOS (não caem aqui).
    /// </summary>
    public static bool StatusPermiteEdicaoComJustificativa(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.Andamento => true,
        StatusPedidoVenda.Concluido => true,
        StatusPedidoVenda.AEntregar => true,
        StatusPedidoVenda.Pausado   => true,
        _ => false
    };

    private static EventoPedidoVenda MapearEvento(StatusPedidoVenda anterior, StatusPedidoVenda novo)
    {
        if (anterior == StatusPedidoVenda.Cancelado && novo == StatusPedidoVenda.Reaberto)
            return EventoPedidoVenda.Reaberto;

        if (anterior == StatusPedidoVenda.Pausado)
            return EventoPedidoVenda.Retomado;

        if (novo == StatusPedidoVenda.Pausado)    return EventoPedidoVenda.Pausado;
        if (novo == StatusPedidoVenda.Cancelado)  return EventoPedidoVenda.Cancelado;
        if (novo == StatusPedidoVenda.Devolvido)  return EventoPedidoVenda.Devolvido;

        if (novo == StatusPedidoVenda.RecebidoNS)        return EventoPedidoVenda.NsRecebido;
        if (novo == StatusPedidoVenda.AguardandoRetorno) return EventoPedidoVenda.RetornoSolicitado;
        if (novo == StatusPedidoVenda.Liberado)          return EventoPedidoVenda.Aprovado;

        if (novo == StatusPedidoVenda.Andamento) return EventoPedidoVenda.ProducaoIniciada;
        if (novo == StatusPedidoVenda.Concluido) return EventoPedidoVenda.ProducaoConcluida;
        if (novo == StatusPedidoVenda.AEntregar) return EventoPedidoVenda.LiberadoEntrega;
        if (novo == StatusPedidoVenda.Entregue)  return EventoPedidoVenda.Entregue;

        return EventoPedidoVenda.Criado;
    }

    private void RegistrarEvento(
        int pedidoVendaId,
        EventoPedidoVenda evento,
        StatusPedidoVenda? statusAnterior,
        StatusPedidoVenda? statusNovo,
        string? justificativa,
        string? detalhe)
    {
        var msg = justificativa;
        if (!string.IsNullOrWhiteSpace(detalhe))
            msg = string.IsNullOrWhiteSpace(justificativa) ? detalhe : $"{justificativa} [{detalhe}]";

        _context.PedidoVendaHistorico.Add(new PedidoVendaHistorico
        {
            PedidoVendaId = pedidoVendaId,
            Evento = evento,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            Justificativa = msg,
            DataHora = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        });
    }

    private static PedidoVendaResponseDTO ToResponseDTO(PedidoVenda p, List<PedidoVendaItem> itens) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        ClienteId = p.ClienteId,
        ClienteNome = p.Cliente?.Pessoa?.Nome ?? string.Empty,
        Tipo = p.Tipo,
        Status = p.Status,
        Data = p.Data,
        DataEntrega = p.DataEntrega,
        Observacoes = p.Observacoes,
        Itens = itens.Select(i => new PedidoVendaItemResponseDTO
        {
            Id = i.Id,
            PedidoVendaId = i.PedidoVendaId,
            Quantidade = i.Quantidade,
            Descricao = i.Descricao,
            Observacao = i.Observacao,
            CriadoEm = i.CriadoEm,
            ModificadoEm = i.ModificadoEm
        }).ToList(),
        TotalItens = itens.Count,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}
