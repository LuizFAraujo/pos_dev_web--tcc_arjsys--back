using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

public class AuthService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    public async Task<(LoginResponseDTO? Dados, string? Erro)> Login(LoginRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Usuario) || string.IsNullOrWhiteSpace(dto.Senha))
            return (null, "Usuário e senha são obrigatórios");

        var funcionario = await _context.Funcionarios
            .Include(f => f.Pessoa)
            .FirstOrDefaultAsync(f => f.Usuario == dto.Usuario);

        if (funcionario == null)
            return (null, "Usuário ou senha inválidos");

        var senhaHash = HashSenha(dto.Senha);

        if (funcionario.SenhaHash != senhaHash)
            return (null, "Usuário ou senha inválidos");

        if (!funcionario.Pessoa.Ativo)
            return (null, "Usuário inativo");

        var permissoes = await _context.Permissoes
            .Where(p => p.FuncionarioId == funcionario.Id)
            .Include(p => p.Funcionario)
                .ThenInclude(f => f.Pessoa)
            .Select(p => new PermissaoResponseDTO
            {
                Id = p.Id,
                FuncionarioId = p.FuncionarioId,
                FuncionarioNome = p.Funcionario.Pessoa.Nome,
                Modulo = p.Modulo,
                Nivel = p.Nivel,
                CriadoEm = p.CriadoEm
            })
            .ToListAsync();

        return (new LoginResponseDTO
        {
            FuncionarioId = funcionario.Id,
            Nome = funcionario.Pessoa.Nome,
            Usuario = funcionario.Usuario,
            Cargo = funcionario.Cargo,
            Setor = funcionario.Setor,
            Permissoes = permissoes
        }, null);
    }

    private static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLower();
    }
}