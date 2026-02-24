using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using Api_ArjSys_Tcc.Models.Engenharia.Enums;
using Api_ArjSys_Tcc.DTOs.Engenharia;

namespace Api_ArjSys_Tcc.Services.Engenharia;

public class GrupoVinculoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<GrupoVinculoResponseDTO>> GetAll()
    {
        return await _context.GruposVinculos
            .Include(v => v.GrupoPai)
            .Include(v => v.GrupoFilho)
            .OrderBy(v => v.GrupoPai.Nivel)
            .ThenBy(v => v.GrupoPai.Codigo)
            .ThenBy(v => v.GrupoFilho.Codigo)
            .Select(v => ToResponseDTO(v))
            .ToListAsync();
    }

    public async Task<List<GrupoVinculoResponseDTO>> GetByPaiId(int paiId)
    {
        return await _context.GruposVinculos
            .Where(v => v.GrupoPaiId == paiId)
            .Include(v => v.GrupoPai)
            .Include(v => v.GrupoFilho)
            .OrderBy(v => v.GrupoFilho.Codigo)
            .Select(v => ToResponseDTO(v))
            .ToListAsync();
    }

    public async Task<(GrupoVinculoResponseDTO? Item, string? Erro)> Create(GrupoVinculoCreateDTO dto)
    {
        if (dto.GrupoPaiId == dto.GrupoFilhoId)
            return (null, "Um grupo não pode ser vinculado a ele mesmo");

        var pai = await _context.GruposProdutos.FindAsync(dto.GrupoPaiId);
        var filho = await _context.GruposProdutos.FindAsync(dto.GrupoFilhoId);

        if (pai == null)
            return (null, "Grupo pai não encontrado");

        if (filho == null)
            return (null, "Grupo filho não encontrado");

        // Valida níveis consecutivos: Grupo→Subgrupo, Subgrupo→Familia
        var nivelPaiEsperado = filho.Nivel switch
        {
            NivelGrupo.Coluna2 => NivelGrupo.Coluna1,
            NivelGrupo.Coluna3 => NivelGrupo.Coluna2,
            _ => (NivelGrupo?)null
        };

        if (nivelPaiEsperado == null)
            return (null, "Itens de Coluna 1 não podem ser filhos de ninguém");

        if (pai.Nivel != nivelPaiEsperado)
            return (null, $"Nível inválido: {filho.Nivel} só pode ser filho de {nivelPaiEsperado}");

        var existeDuplicado = await _context.GruposVinculos
            .AnyAsync(v => v.GrupoPaiId == dto.GrupoPaiId && v.GrupoFilhoId == dto.GrupoFilhoId);

        if (existeDuplicado)
            return (null, "Este vínculo já existe");

        var vinculo = new GrupoVinculo
        {
            GrupoPaiId = dto.GrupoPaiId,
            GrupoFilhoId = dto.GrupoFilhoId,
            CriadoEm = DateTime.UtcNow
        };

        _context.GruposVinculos.Add(vinculo);
        await _context.SaveChangesAsync();

        await _context.Entry(vinculo).Reference(v => v.GrupoPai).LoadAsync();
        await _context.Entry(vinculo).Reference(v => v.GrupoFilho).LoadAsync();

        return (ToResponseDTO(vinculo), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var vinculo = await _context.GruposVinculos.FindAsync(id);

        if (vinculo == null)
            return (false, "Vínculo não encontrado");

        _context.GruposVinculos.Remove(vinculo);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static GrupoVinculoResponseDTO ToResponseDTO(GrupoVinculo v) => new()
    {
        Id = v.Id,
        GrupoPaiId = v.GrupoPaiId,
        GrupoPaiCodigo = v.GrupoPai.Codigo,
        GrupoPaiDescricao = v.GrupoPai.Descricao,
        GrupoPaiNivel = v.GrupoPai.Nivel.ToString(),
        GrupoFilhoId = v.GrupoFilhoId,
        GrupoFilhoCodigo = v.GrupoFilho.Codigo,
        GrupoFilhoDescricao = v.GrupoFilho.Descricao,
        GrupoFilhoNivel = v.GrupoFilho.Nivel.ToString(),
        CriadoEm = v.CriadoEm
    };
}