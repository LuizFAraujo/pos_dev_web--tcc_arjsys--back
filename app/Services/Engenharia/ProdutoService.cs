using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class ProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<ProdutoResponseDTO>> GetAll()
    {
        return await _context.Produtos
            .Select(p => ToResponseDTO(p))
            .ToListAsync();
    }

    public async Task<ProdutoResponseDTO?> GetById(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        return produto == null ? null : ToResponseDTO(produto);
    }

    public async Task<ProdutoResponseDTO> Create(ProdutoCreateDTO dto)
    {
        var produto = new Produto
        {
            Codigo = dto.Codigo,
            Descricao = dto.Descricao,
            DescricaoCompleta = dto.DescricaoCompleta,
            Unidade = dto.Unidade,
            Tipo = dto.Tipo,
            Peso = dto.Peso,
            Ativo = dto.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return ToResponseDTO(produto);
    }

    public async Task<bool> Update(int id, ProdutoCreateDTO dto)
    {
        var existente = await _context.Produtos.FindAsync(id);

        if (existente == null)
            return false;

        existente.Codigo = dto.Codigo;
        existente.Descricao = dto.Descricao;
        existente.DescricaoCompleta = dto.DescricaoCompleta;
        existente.Unidade = dto.Unidade;
        existente.Tipo = dto.Tipo;
        existente.Peso = dto.Peso;
        existente.Ativo = dto.Ativo;
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

    private static ProdutoResponseDTO ToResponseDTO(Produto p) => new()
    {
        Id = p.Id,
        Codigo = p.Codigo,
        Descricao = p.Descricao,
        DescricaoCompleta = p.DescricaoCompleta,
        Unidade = p.Unidade,
        Tipo = p.Tipo,
        Peso = p.Peso,
        Ativo = p.Ativo,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}