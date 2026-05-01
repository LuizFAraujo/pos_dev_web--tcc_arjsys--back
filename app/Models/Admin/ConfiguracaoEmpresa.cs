namespace Api_ArjSys_Tcc.Models.Admin;

/// <summary>
/// Configurações gerais da empresa. Singleton — sempre 1 registro com Id = 1.
///
/// Por enquanto guarda:
///   - AnoFundacao: usado pra calcular a "idade" no código de NS (II.MM.AA.NNNNN).
///   - Configurado: flag que indica se o usuário já confirmou o AnoFundacao
///     (impede emissão de NS antes da configuração inicial).
///
/// Futuramente pode crescer (nome empresa, CNPJ, logo, etc).
/// </summary>
public class ConfiguracaoEmpresa : BaseEntity
{
    /// <summary>
    /// Ano de fundação da empresa. Usado para calcular a "idade" no código de NS.
    /// </summary>
    public int AnoFundacao { get; set; }

    /// <summary>
    /// Indica se o AnoFundacao já foi confirmado pelo usuário (vira true no
    /// primeiro PUT bem-sucedido). NS só pode ser emitido com isso = true.
    /// </summary>
    public bool Configurado { get; set; } = false;
}
