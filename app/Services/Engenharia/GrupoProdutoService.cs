using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class GrupoProdutoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<GrupoProdutoResponseDTO>> GetAll()
    {
        return await _context.GruposProdutos
            .OrderBy(g => g.Nivel)
            .ThenBy(g => g.Codigo)
            .Select(g => ToResponseDTO(g))
            .ToListAsync();
    }

    public async Task<List<GrupoProdutoResponseDTO>> GetByNivel(string nivel)
    {
        return await _context.GruposProdutos
            .Where(g => g.Nivel.ToString() == nivel)
            .OrderBy(g => g.Codigo)
            .Select(g => ToResponseDTO(g))
            .ToListAsync();
    }

    public async Task<GrupoProdutoResponseDTO?> GetById(int id)
    {
        var grupo = await _context.GruposProdutos.FindAsync(id);
        return grupo == null ? null : ToResponseDTO(grupo);
    }

    public async Task<(GrupoProdutoResponseDTO? Item, string? Erro)> Create(GrupoProdutoCreateDTO dto)
    {
        var existeDuplicado = await _context.GruposProdutos
            .AnyAsync(g => g.Codigo == dto.Codigo && g.Nivel == dto.Nivel);

        if (existeDuplicado)
            return (null, "Já existe um grupo com este código neste nível");

        if (dto.QtdCaracteres <= 0)
            return (null, "Quantidade de caracteres deve ser maior que zero");

        var grupo = new GrupoProduto
        {
            Codigo = dto.Codigo,
            Descricao = dto.Descricao,
            Nivel = dto.Nivel,
            QtdCaracteres = dto.QtdCaracteres,
            PathDocumentos = dto.PathDocumentos,
            Ativo = dto.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        _context.GruposProdutos.Add(grupo);
        await _context.SaveChangesAsync();
        return (ToResponseDTO(grupo), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, GrupoProdutoCreateDTO dto)
    {
        var existente = await _context.GruposProdutos.FindAsync(id);

        if (existente == null)
            return (false, "Grupo não encontrado");

        var existeDuplicado = await _context.GruposProdutos
            .AnyAsync(g => g.Codigo == dto.Codigo && g.Nivel == dto.Nivel && g.Id != id);

        if (existeDuplicado)
            return (false, "Já existe um grupo com este código neste nível");

        existente.Codigo = dto.Codigo;
        existente.Descricao = dto.Descricao;
        existente.Nivel = dto.Nivel;
        existente.QtdCaracteres = dto.QtdCaracteres;
        existente.PathDocumentos = dto.PathDocumentos;
        existente.Ativo = dto.Ativo;
        existente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var grupo = await _context.GruposProdutos.FindAsync(id);

        if (grupo == null)
            return (false, "Grupo não encontrado");

        var temVinculos = await _context.GruposVinculos
            .AnyAsync(v => v.GrupoPaiId == id || v.GrupoFilhoId == id);

        if (temVinculos)
            return (false, "Não é possível excluir grupo que possui vínculos");

        _context.GruposProdutos.Remove(grupo);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public string MontarPathDocumento(GrupoProduto grupo, string codigoProduto, string pathRaiz)
    {
        var basePath = string.IsNullOrWhiteSpace(grupo.PathDocumentos)
            ? Path.Combine(pathRaiz, grupo.Codigo)
            : grupo.PathDocumentos;

        return Path.Combine(basePath, codigoProduto);
    }

    private static GrupoProdutoResponseDTO ToResponseDTO(GrupoProduto g) => new()
    {
        Id = g.Id,
        Codigo = g.Codigo,
        Descricao = g.Descricao,
        Nivel = g.Nivel,
        QtdCaracteres = g.QtdCaracteres,
        PathDocumentos = g.PathDocumentos,
        Ativo = g.Ativo,
        CriadoEm = g.CriadoEm,
        ModificadoEm = g.ModificadoEm
    };
}