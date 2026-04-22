using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Admin;
using Api_ArjSys_Tcc.Models.Admin.Enums;
using Api_ArjSys_Tcc.DTOs.Admin;

namespace Api_ArjSys_Tcc.Services.Admin;

public class ClienteService(AppDbContext context)
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Lista clientes. Suporta filtro por texto (busca em nome, codigo, cpfCnpj e cidade).
    /// </summary>
    public async Task<List<ClienteResponseDTO>> GetAll(string? busca = null)
    {
        var query = _context.Clientes.Include(c => c.Pessoa).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(c =>
                c.Pessoa.Nome.Contains(termo) ||
                c.Pessoa.Codigo.Contains(termo) ||
                (c.Pessoa.CpfCnpj != null && c.Pessoa.CpfCnpj.Contains(termo)) ||
                (c.Pessoa.Cidade != null && c.Pessoa.Cidade.Contains(termo)));
        }

        return await query
            .OrderBy(c => c.Pessoa.Nome)
            .Select(c => ToResponseDTO(c))
            .ToListAsync();
    }

    public async Task<ClienteResponseDTO?> GetById(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == id);

        return cliente == null ? null : ToResponseDTO(cliente);
    }

    /// <summary>
    /// Cria cliente. Código é gerado automaticamente (CLI-NNNN).
    /// Em caso de colisão no unique index (concorrência), tenta novamente até 3 vezes.
    /// </summary>
    public async Task<(ClienteResponseDTO? Item, string? Erro)> Create(ClienteCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return (null, "Nome é obrigatório");

        const int maxTentativas = 3;

        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                var codigo = await CodigoPessoaGenerator.GerarProximo(_context, TipoPessoa.Cliente);

                var pessoa = new Pessoa
                {
                    Codigo = codigo,
                    Nome = dto.Nome,
                    CpfCnpj = dto.CpfCnpj,
                    Telefone = dto.Telefone,
                    Email = dto.Email,
                    Endereco = dto.Endereco,
                    Cidade = dto.Cidade,
                    Estado = dto.Estado,
                    Cep = dto.Cep,
                    Tipo = TipoPessoa.Cliente,
                    CriadoEm = DateTime.UtcNow
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();

                var cliente = new Cliente
                {
                    PessoaId = pessoa.Id,
                    RazaoSocial = dto.RazaoSocial,
                    InscricaoEstadual = dto.InscricaoEstadual,
                    ContatoComercial = dto.ContatoComercial,
                    CriadoEm = DateTime.UtcNow
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                cliente.Pessoa = pessoa;
                return (ToResponseDTO(cliente), null);
            }
            catch (DbUpdateException) when (tentativa < maxTentativas)
            {
                // Colisão no unique index — limpa tracking e tenta de novo
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    entry.State = EntityState.Detached;
            }
        }

        return (null, "Não foi possível gerar código único após várias tentativas. Tente novamente.");
    }

    public async Task<(bool Sucesso, string? Erro)> Update(int id, ClienteCreateDTO dto)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return (false, "Cliente não encontrado");

        // Codigo não muda no update
        cliente.Pessoa.Nome = dto.Nome;
        cliente.Pessoa.CpfCnpj = dto.CpfCnpj;
        cliente.Pessoa.Telefone = dto.Telefone;
        cliente.Pessoa.Email = dto.Email;
        cliente.Pessoa.Endereco = dto.Endereco;
        cliente.Pessoa.Cidade = dto.Cidade;
        cliente.Pessoa.Estado = dto.Estado;
        cliente.Pessoa.Cep = dto.Cep;
        cliente.Pessoa.ModificadoEm = DateTime.UtcNow;

        cliente.RazaoSocial = dto.RazaoSocial;
        cliente.InscricaoEstadual = dto.InscricaoEstadual;
        cliente.ContatoComercial = dto.ContatoComercial;
        cliente.ModificadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Sucesso, string? Erro)> Delete(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Pessoa)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
            return (false, "Cliente não encontrado");

        _context.Clientes.Remove(cliente);
        _context.Pessoas.Remove(cliente.Pessoa);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    private static ClienteResponseDTO ToResponseDTO(Cliente c) => new()
    {
        Id = c.Id,
        PessoaId = c.PessoaId,
        Codigo = c.Pessoa.Codigo,
        Nome = c.Pessoa.Nome,
        CpfCnpj = c.Pessoa.CpfCnpj,
        Telefone = c.Pessoa.Telefone,
        Email = c.Pessoa.Email,
        Endereco = c.Pessoa.Endereco,
        Cidade = c.Pessoa.Cidade,
        Estado = c.Pessoa.Estado,
        Cep = c.Pessoa.Cep,
        RazaoSocial = c.RazaoSocial,
        InscricaoEstadual = c.InscricaoEstadual,
        ContatoComercial = c.ContatoComercial,
        Ativo = c.Pessoa.Ativo,
        CriadoEm = c.CriadoEm,
        ModificadoEm = c.ModificadoEm
    };
}
