using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class ProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<Produto>> GetAll()
    {
        return await _context.Produtos.ToListAsync();
    }

    public async Task<Produto?> GetById(int id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<Produto> Create(Produto produto)
    {
        produto.CriadoEm = DateTime.UtcNow;
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<bool> Update(int id, Produto produto)
    {
        var existente = await _context.Produtos.FindAsync(id);

        if (existente == null)
            return false;

        existente.Codigo = produto.Codigo;
        existente.Descricao = produto.Descricao;
        existente.DescricaoCompleta = produto.DescricaoCompleta;
        existente.Unidade = produto.Unidade;
        existente.Tipo = produto.Tipo;
        existente.Peso = produto.Peso;
        existente.Ativo = produto.Ativo;
        existente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return false;

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return true;
    }
}