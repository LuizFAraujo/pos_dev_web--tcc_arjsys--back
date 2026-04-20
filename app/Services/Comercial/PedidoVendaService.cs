using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço do Pedido de Venda.
/// Gerencia CRUD, transições de status com justificativa condicional e histórico de eventos.
/// Fluxo depende do Tipo:
///   Normal   → Liberado → Andamento → Concluido → AEntregar → Entregue
///   PreVenda → AguardandoNS → RecebidoNS → AguardandoRetorno → Liberado → (fluxo Normal)
/// Status especiais: Pausado, Cancelado, Reaberto, Devolvido.
/// </summary>
public class PedidoVendaService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista todos os PVs. Suporta paginação opcional.
    /// </summary>
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

    /// <summary>
    /// Busca PV por ID, incluindo itens.
    /// </summary>
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

    /// <summary>
    /// Cria Pedido de Venda. Status inicial é definido pelo tipo:
    ///   Normal   → Liberado
    ///   PreVenda → AguardandoNS
    /// Registra evento Criado no histórico.
    /// </summary>
    public async Task<(PedidoVendaResponseDTO? Item, string? Erro)> Create(PedidoVendaCreateDTO dto)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == dto.ClienteId);

        if (cliente == null)
            return (null, "Cliente não encontrado");

        var statusInicial = dto.Tipo == TipoPedidoVenda.PreVenda
            ? StatusPedidoVenda.AguardandoNS
            : StatusPedidoVenda.Liberado;

        var codigo = await GerarCodigo();
        var agora = DateTime.UtcNow;

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

        RegistrarEvento(pedido.Id, EventoPedidoVenda.Criado, null, statusInicial, null);
        await _context.SaveChangesAsync();

        pedido.Cliente = cliente;
        return (ToResponseDTO(pedido, []), null);
    }

    /// <summary>
    /// Atualiza dados cadastrais do PV. Permitido apenas em status de fluxo inicial
    /// (AguardandoNS, RecebidoNS, AguardandoRetorno, Liberado).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int id, PedidoVendaCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        if (!StatusPermiteEdicao(pedido.Status))
            return (false, "Só é possível editar o PV nos status iniciais do fluxo (AguardandoNS, RecebidoNS, AguardandoRetorno, Liberado)");

        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);

        if (cliente == null)
            return (false, "Cliente não encontrado");

        pedido.ClienteId = dto.ClienteId;
        pedido.Tipo = dto.Tipo;
        pedido.Data = dto.Data ?? pedido.Data;
        pedido.DataEntrega = dto.DataEntrega;
        pedido.Observacoes = dto.Observacoes;
        pedido.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Altera status do PV com validação de transição e justificativa.
    /// Regras:
    /// - Pausar/Cancelar proibidos em Entregue.
    /// - Devolvido só pode vir de Entregue.
    /// - Cancelado só pode ir para Reaberto.
    /// - Justificativa obrigatória em: Pausado, Cancelado, Reaberto, Devolvido e retrocesso.
    /// Registra evento no histórico com statusAnterior, statusNovo e justificativa.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> AlterarStatus(int id, StatusPedidoVendaDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(id);

        if (pedido == null)
            return (false, null);

        var statusAnterior = pedido.Status;
        var novoStatus = dto.NovoStatus;

        if (statusAnterior == novoStatus)
            return (false, $"O pedido já está com status {novoStatus}");

        var (transicaoValida, erroTransicao) = TransicaoPermitida(statusAnterior, novoStatus);
        if (!transicaoValida)
            return (false, erroTransicao);

        var exigeJustificativa = ExigeJustificativa(statusAnterior, novoStatus);
        var justificativa = dto.Justificativa?.Trim();

        if (exigeJustificativa && string.IsNullOrWhiteSpace(justificativa))
            return (false, "Justificativa é obrigatória para esta transição de status");

        pedido.Status = novoStatus;
        pedido.ModificadoEm = DateTime.UtcNow;

        var evento = MapearEvento(statusAnterior, novoStatus);
        RegistrarEvento(id, evento, statusAnterior, novoStatus, justificativa);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Exclui PV. Permitido apenas nos primeiros status (AguardandoNS, Liberado) e sem NS vinculado.
    /// </summary>
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

        var itens = await _context.PedidosVendaItens.Where(i => i.PedidoVendaId == id).ToListAsync();
        _context.PedidosVendaItens.RemoveRange(itens);

        var historico = await _context.PedidoVendaHistorico.Where(h => h.PedidoVendaId == id).ToListAsync();
        _context.PedidoVendaHistorico.RemoveRange(historico);

        _context.PedidosVenda.Remove(pedido);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Retorna o histórico de eventos de um PV, ordenado do mais recente pro mais antigo.
    /// </summary>
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

    /// <summary>
    /// Gera o próximo código no formato PV.AAAA.MM.NNNN.
    /// </summary>
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

    /// <summary>
    /// Valida se a transição de status é permitida.
    /// Regras de bloqueio:
    /// - Entregue: não aceita Pausar nem Cancelar (só Devolvido).
    /// - Cancelado: só pode ir para Reaberto.
    /// - Devolvido: só pode vir de Entregue.
    /// - Reaberto é status permanente até Comercial movê-lo manualmente para Liberado.
    /// </summary>
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

    /// <summary>
    /// Nível ordinal do status no fluxo — usado para detectar retrocesso.
    /// Status especiais têm nível -1 (fora do fluxo linear).
    /// </summary>
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

    /// <summary>
    /// Determina se a transição exige justificativa obrigatória.
    /// Regra: ir para Pausado/Cancelado/Reaberto/Devolvido OU retroceder no fluxo.
    /// </summary>
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

    /// <summary>
    /// Indica se o status faz parte do fluxo linear (não é especial).
    /// </summary>
    private static bool EhStatusDeFluxo(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.Pausado    => false,
        StatusPedidoVenda.Cancelado  => false,
        StatusPedidoVenda.Reaberto   => false,
        StatusPedidoVenda.Devolvido  => false,
        _ => true
    };

    /// <summary>
    /// Indica se o status permite edição dos dados cadastrais do PV.
    /// Permitido nos status iniciais; bloqueado após produção começar.
    /// </summary>
    private static bool StatusPermiteEdicao(StatusPedidoVenda status) => status switch
    {
        StatusPedidoVenda.AguardandoNS      => true,
        StatusPedidoVenda.RecebidoNS        => true,
        StatusPedidoVenda.AguardandoRetorno => true,
        StatusPedidoVenda.Liberado          => true,
        _ => false
    };

    /// <summary>
    /// Mapeia a transição de status para o evento correspondente no histórico.
    /// </summary>
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

    /// <summary>
    /// Registra um evento no histórico do PV.
    /// </summary>
    private void RegistrarEvento(
        int pedidoVendaId,
        EventoPedidoVenda evento,
        StatusPedidoVenda? statusAnterior,
        StatusPedidoVenda? statusNovo,
        string? justificativa)
    {
        _context.PedidoVendaHistorico.Add(new PedidoVendaHistorico
        {
            PedidoVendaId = pedidoVendaId,
            Evento = evento,
            StatusAnterior = statusAnterior,
            StatusNovo = statusNovo,
            Justificativa = justificativa,
            DataHora = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Converte entidade PV + itens em DTO de resposta.
    /// </summary>
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
