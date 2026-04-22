using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.Models.Admin;

public class Pessoa : BaseEntity
{
    /// <summary>
    /// Código humano único por pessoa, prefixo por tipo (CLI-0001, FUN-0001, FOR-0001).
    /// Gerado automaticamente pelo back — não aceito no payload de criação.
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

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
