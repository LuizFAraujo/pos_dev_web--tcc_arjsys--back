namespace Api_ArjSys_Tcc.DTOs.Admin;

public class FuncionarioCreateDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class FuncionarioResponseDTO
{
    public int Id { get; set; }
    public int PessoaId { get; set; }

    /// <summary>Código humano único (ex: "FUN-0042"). Gerado automaticamente.</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
