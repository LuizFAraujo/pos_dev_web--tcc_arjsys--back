using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Comercial;
using Api_ArjSys_Tcc.Models.Comercial.Enums;
using Api_ArjSys_Tcc.DTOs.Comercial;

namespace Api_ArjSys_Tcc.Services.Comercial;

public class PedidoVendaItemService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<PedidoVendaItemResponseDTO>> GetByPedidoId(int pedidoId)
    {
        return await _context.PedidosVendaItens
            .Where(i => i.PedidoVendaId == pedidoId)
            .Include(i => i.Produto)
            .OrderBy(i => i.Id)
            .Select(i => ToResponseDTO(i))
            .ToListAsync();
    }

    public async Task<(PedidoVendaItemResponseDTO? Item, string? Erro)> Create(int pedidoId, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (null, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Orcamento)
            return (null, "Só é possível adicionar itens em pedidos com status Orçamento");

        var produto = await _context.Produtos.FindAsync(dto.ProdutoId);

        if (produto == null)
            return (null, "Produto não encontrado");

        if (dto.Quantidade <= 0)
            return (null, "Quantidade deve ser maior que zero");

        var duplicado = await _context.PedidosVendaItens
            .AnyAsync(i => i.PedidoVendaId == pedidoId && i.ProdutoId == dto.ProdutoId);

        if (duplicado)
            return (null, "Este produto já está no pedido");

        var item = new PedidoVendaItem
        {
            PedidoVendaId = pedidoId,
            ProdutoId = dto.ProdutoId,
            Quantidade = dto.Quantidade,
            PrecoUnitario = dto.PrecoUnitario,
            CriadoEm = DateTime.UtcNow
        };

        _context.PedidosVendaItens.Add(item);
        await _context.SaveChangesAsync();

        item.Produto = produto;
        return (ToResponseDTO(item), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int pedidoId, int id, PedidoVendaItemCreateDTO dto)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Orcamento)
            return (false, "Só é possível editar itens em pedidos com status Orçamento");

        var item = await _context.PedidosVendaItens.FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, "Item não encontrado neste pedido");

        if (dto.Quantidade <= 0)
            return (false, "Quantidade deve ser maior que zero");

        item.Quantidade = dto.Quantidade;
        item.PrecoUnitario = dto.PrecoUnitario;
        item.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int pedidoId, int id)
    {
        var pedido = await _context.PedidosVenda.FindAsync(pedidoId);

        if (pedido == null)
            return (false, "Pedido não encontrado");

        if (pedido.Status != StatusPedidoVenda.Orcamento)
            return (false, "Só é possível remover itens em pedidos com status Orçamento");

        var item = await _context.PedidosVendaItens.FirstOrDefaultAsync(i => i.Id == id && i.PedidoVendaId == pedidoId);

        if (item == null)
            return (false, "Item não encontrado neste pedido");

        _context.PedidosVendaItens.Remove(item);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static PedidoVendaItemResponseDTO ToResponseDTO(PedidoVendaItem i) => new()
    {
        Id = i.Id,
        PedidoVendaId = i.PedidoVendaId,
        ProdutoId = i.ProdutoId,
        ProdutoCodigo = i.Produto.Codigo,
        ProdutoDescricao = i.Produto.Descricao,
        Quantidade = i.Quantidade,
        PrecoUnitario = i.PrecoUnitario,
        Subtotal = i.Quantidade * i.PrecoUnitario,
        CriadoEm = i.CriadoEm
    };
}