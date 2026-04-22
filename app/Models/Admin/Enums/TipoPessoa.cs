namespace Api_ArjSys_Tcc.Models.Admin.Enums;

/// <summary>
/// Tipo de pessoa no sistema.
/// Usado como prefixo do código humano (CLI-/FUN-/FOR-) e para roteamento
/// entre entidades derivadas (Cliente, Funcionario, Fornecedor).
/// </summary>
public enum TipoPessoa
{
    Cliente,
    Funcionario,
    Ambos,
    Fornecedor
}
