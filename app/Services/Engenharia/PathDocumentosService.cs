using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class PathDocumentosService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<PathDocumentosResponseDTO>> GetAll()
    {
        return await _context.PathDocumentos
            .Include(p => p.GrupoProduto)
            .OrderBy(p => p.GrupoProduto.Codigo)
            .Select(p => ToResponseDTO(p))
            .ToListAsync();
    }

    public async Task<PathDocumentosResponseDTO?> GetById(int id)
    {
        var item = await _context.PathDocumentos
            .Include(p => p.GrupoProduto)
            .FirstOrDefaultAsync(p => p.Id == id);

        return item == null ? null : ToResponseDTO(item);
    }

    public async Task<(PathDocumentosResponseDTO? Item, string? Erro)> Create(PathDocumentosCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Path))
            return (null, "Path é obrigatório");

        // Validar que o GrupoProduto existe e é Coluna1
        var grupo = await _context.GruposProdutos.FindAsync(dto.GrupoProdutoId);

        if (grupo == null)
            return (null, "Grupo de produto não encontrado");

        if (grupo.Nivel != NivelGrupo.Coluna1)
            return (null, "Apenas grupos de nível Coluna1 (prefixos) podem ter path alternativo");

        // Validar unique (1 path por prefixo)
        var existe = await _context.PathDocumentos
            .AnyAsync(p => p.GrupoProdutoId == dto.GrupoProdutoId);

        if (existe)
            return (null, "Já existe um path cadastrado para este prefixo");

        var item = new PathDocumentos
        {
            GrupoProdutoId = dto.GrupoProdutoId,
            Path = dto.Path.TrimEnd('\\', '/'),
            ControlarPorPrefixo = dto.ControlarPorPrefixo,
            Ativo = dto.Ativo,
            CriadoEm = DateTime.UtcNow
        };

        _context.PathDocumentos.Add(item);
        await _context.SaveChangesAsync();

        // Recarregar com Include pra montar o response
        await _context.Entry(item).Reference(p => p.GrupoProduto).LoadAsync();

        return (ToResponseDTO(item), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, PathDocumentosUpdateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Path))
            return (false, "Path é obrigatório");

        var item = await _context.PathDocumentos.FindAsync(id);

        if (item == null)
            return (false, null);

        item.Path = dto.Path.TrimEnd('\\', '/');
        item.ControlarPorPrefixo = dto.ControlarPorPrefixo;
        item.Ativo = dto.Ativo;
        item.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var item = await _context.PathDocumentos.FindAsync(id);

        if (item == null)
            return (false, null);

        _context.PathDocumentos.Remove(item);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static PathDocumentosResponseDTO ToResponseDTO(PathDocumentos p) => new()
    {
        Id = p.Id,
        GrupoProdutoId = p.GrupoProdutoId,
        GrupoCodigo = p.GrupoProduto.Codigo,
        GrupoDescricao = p.GrupoProduto.Descricao,
        Path = p.Path,
        ControlarPorPrefixo = p.ControlarPorPrefixo,
        Ativo = p.Ativo,
        CriadoEm = p.CriadoEm,
        ModificadoEm = p.ModificadoEm
    };
}