using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.Models.Admin;

public class Pessoa : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }
    public TipoPessoa Tipo { get; set; }
    public bool Ativo { get; set; } = true;
}