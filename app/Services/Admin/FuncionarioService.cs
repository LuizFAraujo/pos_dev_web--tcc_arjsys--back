using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

public class FuncionarioService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<List<FuncionarioResponseDTO>> GetAll()
    {
        return await _context.Funcionarios
            .Include(f => f.Pessoa)
            .OrderBy(f => f.Pessoa.Nome)
            .Select(f => ToResponseDTO(f))
            .ToListAsync();
    }

    public async Task<FuncionarioResponseDTO?> GetById(int id)
    {
        var func = await _context.Funcionarios
            .Include(f => f.Pessoa)
            .FirstOrDefaultAsync(f => f.Id == id);

        return func == null ? null : ToResponseDTO(func);
    }

    public async Task<(FuncionarioResponseDTO? Item, string? Erro)> Create(FuncionarioCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return (null, "Nome é obrigatório");

        if (string.IsNullOrWhiteSpace(dto.Usuario))
            return (null, "Usuário é obrigatório");

        if (string.IsNullOrWhiteSpace(dto.Senha))
            return (null, "Senha é obrigatória");

        var usuarioExiste = await _context.Funcionarios
            .AnyAsync(f => f.Usuario == dto.Usuario);

        if (usuarioExiste)
            return (null, "Este usuário já está em uso");

        var pessoa = new Pessoa
        {
            Nome = dto.Nome,
            CpfCnpj = dto.CpfCnpj,
            Telefone = dto.Telefone,
            Email = dto.Email,
            Endereco = dto.Endereco,
            Cidade = dto.Cidade,
            Estado = dto.Estado,
            Cep = dto.Cep,
            Tipo = TipoPessoa.Funcionario,
            CriadoEm = DateTime.UtcNow
        };

        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        var funcionario = new Funcionario
        {
            PessoaId = pessoa.Id,
            Cargo = dto.Cargo,
            Setor = dto.Setor,
            Usuario = dto.Usuario,
            SenhaHash = HashSenha(dto.Senha),
            CriadoEm = DateTime.UtcNow
        };

        _context.Funcionarios.Add(funcionario);
        await _context.SaveChangesAsync();

        funcionario.Pessoa = pessoa;
        return (ToResponseDTO(funcionario), null);
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, FuncionarioCreateDTO dto)
    {
        var func = await _context.Funcionarios
            .Include(f => f.Pessoa)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (func == null)
            return (false, "Funcionário não encontrado");

        var usuarioExiste = await _context.Funcionarios
            .AnyAsync(f => f.Usuario == dto.Usuario && f.Id != id);

        if (usuarioExiste)
            return (false, "Este usuário já está em uso");

        func.Pessoa.Nome = dto.Nome;
        func.Pessoa.CpfCnpj = dto.CpfCnpj;
        func.Pessoa.Telefone = dto.Telefone;
        func.Pessoa.Email = dto.Email;
        func.Pessoa.Endereco = dto.Endereco;
        func.Pessoa.Cidade = dto.Cidade;
        func.Pessoa.Estado = dto.Estado;
        func.Pessoa.Cep = dto.Cep;
        func.Pessoa.ModificadoEm = DateTime.UtcNow;

        func.Cargo = dto.Cargo;
        func.Setor = dto.Setor;
        func.Usuario = dto.Usuario;
        func.ModificadoEm = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.Senha))
            func.SenhaHash = HashSenha(dto.Senha);

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var func = await _context.Funcionarios
            .Include(f => f.Pessoa)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (func == null)
            return (false, "Funcionário não encontrado");

        _context.Funcionarios.Remove(func);
        _context.Pessoas.Remove(func.Pessoa);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLower();
    }

    private static FuncionarioResponseDTO ToResponseDTO(Funcionario f) => new()
    {
        Id = f.Id,
        PessoaId = f.PessoaId,
        Nome = f.Pessoa.Nome,
        CpfCnpj = f.Pessoa.CpfCnpj,
        Telefone = f.Pessoa.Telefone,
        Email = f.Pessoa.Email,
        Endereco = f.Pessoa.Endereco,
        Cidade = f.Pessoa.Cidade,
        Estado = f.Pessoa.Estado,
        Cep = f.Pessoa.Cep,
        Cargo = f.Cargo,
        Setor = f.Setor,
        Usuario = f.Usuario,
        Ativo = f.Pessoa.Ativo,
        CriadoEm = f.CriadoEm,
        ModificadoEm = f.ModificadoEm
    };
}