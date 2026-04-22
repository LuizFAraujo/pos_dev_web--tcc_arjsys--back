namespace Api_ArjSys_Tcc.DTOs.Admin;

public class ClienteCreateDTO
{
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? RazaoSocial { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? ContatoComercial { get; set; }
}

public class ClienteResponseDTO
{
    public int Id { get; set; }
    public int PessoaId { get; set; }

    /// <summary>Código humano único (ex: "CLI-0042"). Gerado automaticamente.</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public string? RazaoSocial { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? ContatoComercial { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
