using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

public class PermissaoService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<PermissaoResponseDTO>> GetByFuncionarioId(int funcionarioId)
    {
        return await _context.Permissoes
            .Where(p => p.FuncionarioId == funcionarioId)
            .Include(p => p.Funcionario)
                .ThenInclude(f => f.Pessoa)
            .OrderBy(p => p.Modulo)
            .Select(p => ToResponseDTO(p))
            .ToListAsync();
    }

    public async Task<(PermissaoResponseDTO? Item, string? Erro)> Create(PermissaoCreateDTO dto)
    {
        var funcExiste = await _context.Funcionarios.AnyAsync(f => f.Id == dto.FuncionarioId);

        if (!funcExiste)
            return (null, "Funcionário não encontrado");

        var existeDuplicado = await _context.Permissoes
            .AnyAsync(p => p.FuncionarioId == dto.FuncionarioId && p.Modulo == dto.Modulo);

        if (existeDuplicado)
            return (null, "Já existe permissão para este módulo neste funcionário");

        var permissao = new Permissao
        {
            FuncionarioId = dto.FuncionarioId,
            Modulo = dto.Modulo,
            Nivel = dto.Nivel,
            CriadoEm = DateTime.UtcNow
        };

        _context.Permissoes.Add(permissao);
        await _context.SaveChangesAsync();

        await _context.Entry(permissao).Reference(p => p.Funcionario).LoadAsync();
        await _context.Entry(permissao.Funcionario).Reference(f => f.Pessoa).LoadAsync();

        return (ToResponseDTO(permissao), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, PermissaoCreateDTO dto)
    {
        var permissao = await _context.Permissoes.FindAsync(id);

        if (permissao == null)
            return (false, "Permissão não encontrada");

        permissao.Nivel = dto.Nivel;
        permissao.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var permissao = await _context.Permissoes.FindAsync(id);

        if (permissao == null)
            return (false, "Permissão não encontrada");

        _context.Permissoes.Remove(permissao);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static PermissaoResponseDTO ToResponseDTO(Permissao p) => new()
    {
        Id = p.Id,
        FuncionarioId = p.FuncionarioId,
        FuncionarioNome = p.Funcionario.Pessoa.Nome,
        Modulo = p.Modulo,
        Nivel = p.Nivel,
        CriadoEm = p.CriadoEm
    };
}