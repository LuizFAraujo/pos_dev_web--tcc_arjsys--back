namespace Api_ArjSys_Tcc.DTOs.Admin;

// Classe âncora - evita rename automático do VS Code
public partial class ConfiguracaoEmpresaDTO { }

/// <summary>
/// Saída do endpoint GET - dados readonly da config singleton.
/// Configurado indica se o AnoFundacao já foi confirmado pelo usuário.
/// </summary>
public class ConfiguracaoEmpresaResponseDTO
{
    public int Id { get; set; }
    public int AnoFundacao { get; set; }
    public bool Configurado { get; set; }
}

/// <summary>
/// Entrada do PUT - apenas o ano de fundação.
/// Update normal: bloqueado se já houver NS emitido.
/// Update admin-override: permite alterar mesmo após NS (NS já gerados ficam intactos).
/// </summary>
public class ConfiguracaoEmpresaUpdateDTO
{
    public int AnoFundacao { get; set; }
}
