using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Admin;
using Api_ArjSys_Tcc.DTOs.Comercial;
using Api_ArjSys_Tcc.Services.Admin;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço dos itens do Pedido de Venda (endpoints individuais).
/// Itens são descrição livre (sem vínculo com Produto cadastrado).
///
/// Regras de edição por status:
/// - AguardandoNS/RecebidoNS/AguardandoRetorno/Liberado: livre, sem justificativa
/// - Andamento/Concluido/AEntregar/Pausado: justificativa OBRIGATÓRIA,
///   registra evento ItensAlterados no histórico e notifica Eng/Prod/Almox
/// - Entregue/Devolvido/Cancelado/Reaberto: BLOQUEADO
/// </summary>
public class PedidoVendaItemService(AppDbContext context, NotificacaoService notificacoes)
{
    private readonly AppDbContext _context = context;
    private readonly NotificacaoService _notificacoes = notificacoes;

    public async Task<List<PedidoVendaItemResponseDTO>> GetByPedidoId(int pedidoId)
    {
        return await _context.PedidosVendaItens
            .Where(i => i.PedidoVendaId == pedidoId)
            .OrderBy(i => i.Id)
            .Select(i => new PedidoVendaItemResponseDTO
            {
                Id = i.Id,
                PedidoVendaId = i.PedidoVendaId,
                Quantidade = i.Quantidade,
                Descricao = i.Descricao,
                Observacao = i.Observacao,
                CriadoEm = i.CriadoEm,
                ModificadoEm = i.ModificadoEm
            })
            .ToListAsync();
    }

    /// <summary>
    /// Adiciona item ao PV. Em status avançado exige justificativa.
    /// </summary>
    public async Task<(PedidoVendaItemResponseDTO? Item, string? Erro)> Create(int pedidoId, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            return (null, "Pedido não encontrado");

        var (podeEditar, erroStatus, statusAvancado) = AvaliarPermissao(pedido.Status);
        if (!podeEditar)
            return (null, erroStatus);

        var justificativa = dto.Justificativa?.Trim();
        if (statusAvancado && string.IsNullOrWhiteSpace(justificativa))
            return (null, $"Justificativa é obrigatória para editar itens em PV com status {pedido.Status}.");

        if (dto.Quantidade <= 0)
            return (null, "Quantidade deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            return (null, "Descrição do item é obrigatória");

        var item = new PedidoVendaItem
        {
            PedidoVendaId = pedidoId,
            Quantidade = dto.Quantidade,
            Descricao = dto.Descricao.Trim(),
            Observacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim(),
            CriadoEm = DateTime.UtcNow
        };

        _context.PedidosVendaItens.Add(item);

        if (statusAvancado)
        {
            var detalhe = $"+1 adicionado (item '{item.Descricao}', qtd {item.Quantidade})";
            RegistrarEventoItensAlterados(pedidoId, pedido.Status, justificativa!, detalhe);
        }

        await _context.SaveChangesAsync();

        if (statusAvancado)
            await NotificarEdicaoItensStatusAvancado(pedido, justificativa!, pedido.Status);

        return (ToResponseDTO(item), null);
    }

    /// <summary>
    /// Atualiza item do PV. Em status avançado exige justificativa.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int pedidoId, int id, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        var (podeEditar, erroStatus, statusAvancado) = AvaliarPermissao(pedido.Status);
        if (!podeEditar)
            return (false, erroStatus);

        var justificativa = dto.Justificativa?.Trim();
        if (statusAvancado && string.IsNullOrWhiteSpace(justificativa))
            return (false, $"Justificativa é obrigatória para editar itens em PV com status {pedido.Status}.");

        var item = await _context.PedidosVendaItens
            .FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, null);

        if (dto.Quantidade <= 0)
            return (false, "Quantidade deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            return (false, "Descrição do item é obrigatória");

        var mudou = item.Quantidade != dto.Quantidade
                 || item.Descricao != dto.Descricao.Trim()
                 || (item.Observacao ?? "") != (dto.Observacao?.Trim() ?? "");

        item.Quantidade = dto.Quantidade;
        item.Descricao = dto.Descricao.Trim();
        item.Observacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim();
        item.ModificadoEm = DateTime.UtcNow;

        if (statusAvancado && mudou)
        {
            var detalhe = $"1 alterado (item '{item.Descricao}', nova qtd {item.Quantidade})";
            RegistrarEventoItensAlterados(pedidoId, pedido.Status, justificativa!, detalhe);
        }

        await _context.SaveChangesAsync();

        if (statusAvancado && mudou)
            await NotificarEdicaoItensStatusAvancado(pedido, justificativa!, pedido.Status);

        return (true, null);
    }

    /// <summary>
    /// Remove item do PV. Em status avançado exige justificativa (via query param).
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Delete(int pedidoId, int id, string? justificativa)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Cliente).ThenInclude(c => c.Pessoa)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        var (podeEditar, erroStatus, statusAvancado) = AvaliarPermissao(pedido.Status);
        if (!podeEditar)
            return (false, erroStatus);

        justificativa = justificativa?.Trim();
        if (statusAvancado && string.IsNullOrWhiteSpace(justificativa))
            return (false, $"Justificativa é obrigatória para remover itens em PV com status {pedido.Status}.");

        var item = await _context.PedidosVendaItens
            .FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, null);

        _context.PedidosVendaItens.Remove(item);

        if (statusAvancado)
        {
            var detalhe = $"-1 removido (item '{item.Descricao}', qtd {item.Quantidade})";
            RegistrarEventoItensAlterados(pedidoId, pedido.Status, justificativa!, detalhe);
        }

        await _context.SaveChangesAsync();

        if (statusAvancado)
            await NotificarEdicaoItensStatusAvancado(pedido, justificativa!, pedido.Status);

        return (true, null);
    }

    // ========================================================================
    // PRIVATES
    // ========================================================================

    /// <summary>
    /// Retorna (permitido, mensagemErro, statusAvancado).
    /// - status inicial: (true, null, false)
    /// - status avançado: (true, null, true) - exige justificativa
    /// - status bloqueado: (false, msg, false)
    /// </summary>
    private static (bool PodeEditar, string? Erro, bool StatusAvancado) AvaliarPermissao(StatusPedidoVenda status)
    {
        if (PedidoVendaService.StatusPermiteEdicao(status))
            return (true, null, false);

        if (PedidoVendaService.StatusPermiteEdicaoComJustificativa(status))
            return (true, null, true);

        return (false, $"Pedido com status {status} não aceita edição de itens.", false);
    }

    private void RegistrarEventoItensAlterados(
        int pedidoVendaId,
        StatusPedidoVenda status,
        string justificativa,
        string detalhe)
    {
        _context.PedidoVendaHistorico.Add(new PedidoVendaHistorico
        {
            PedidoVendaId = pedidoVendaId,
            Evento = EventoPedidoVenda.ItensAlterados,
            StatusAnterior = status,
            StatusNovo = status,
            Justificativa = $"{justificativa} [{detalhe}]",
            DataHora = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        });
    }

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

    private static PedidoVendaItemResponseDTO ToResponseDTO(PedidoVendaItem i) => new()
    {
        Id = i.Id,
        PedidoVendaId = i.PedidoVendaId,
        Quantidade = i.Quantidade,
        Descricao = i.Descricao,
        Observacao = i.Observacao,
        CriadoEm = i.CriadoEm,
        ModificadoEm = i.ModificadoEm
    };
}
