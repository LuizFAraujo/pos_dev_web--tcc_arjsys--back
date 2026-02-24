namespace Api_ArjSys_Tcc.Models.Admin;

public class Cliente : BaseEntity
{
    public int PessoaId { get; set; }
    public Pessoa Pessoa { get; set; } = null!;

    public string? RazaoSocial { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? ContatoComercial { get; set; }
}