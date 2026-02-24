namespace Api_ArjSys_Tcc.Models.Admin;

public class Funcionario : BaseEntity
{
    public int PessoaId { get; set; }
    public Pessoa Pessoa { get; set; } = null!;

    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
}