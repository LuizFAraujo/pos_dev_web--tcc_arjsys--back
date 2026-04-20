using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

/// <summary>
/// Serviço dos itens do Pedido de Venda.
/// Itens são descrição livre (sem vínculo com Produto cadastrado).
/// </summary>
public class PedidoVendaItemService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista todos os itens de um PV.
    /// </summary>
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
    /// Adiciona item ao PV. Permitido em Aguardando ou EmAndamento.
    /// </summary>
    public async Task<(PedidoVendaItemResponseDTO? Item, string? Erro)> Create(int pedidoId, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (null, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Aguardando && pedido.Status != StatusPedidoVenda.EmAndamento)
            return (null, "Só é possível adicionar itens em pedidos com status Aguardando ou Em Andamento");

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
        await _context.SaveChangesAsync();

        return (ToResponseDTO(item), null);
    }

    /// <summary>
    /// Atualiza item do PV. Permitido em Aguardando ou EmAndamento.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Update(int pedidoId, int id, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Aguardando && pedido.Status != StatusPedidoVenda.EmAndamento)
            return (false, "Só é possível editar itens em pedidos com status Aguardando ou Em Andamento");

        var item = await _context.PedidosVendaItens.FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, null);

        if (dto.Quantidade <= 0)
            return (false, "Quantidade deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(dto.Descricao))
            return (false, "Descrição do item é obrigatória");

        item.Quantidade = dto.Quantidade;
        item.Descricao = dto.Descricao.Trim();
        item.Observacao = string.IsNullOrWhiteSpace(dto.Observacao) ? null : dto.Observacao.Trim();
        item.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Remove item do PV. Permitido em Aguardando ou EmAndamento.
    /// </summary>
    public async Task<(bool Sucesso, string? Erro)> Delete(int pedidoId, int id)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Aguardando && pedido.Status != StatusPedidoVenda.EmAndamento)
            return (false, "Só é possível remover itens em pedidos com status Aguardando ou Em Andamento");

        var item = await _context.PedidosVendaItens.FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, null);

        _context.PedidosVendaItens.Remove(item);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    /// <summary>
    /// Converte entidade em DTO de resposta.
    /// </summary>
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
