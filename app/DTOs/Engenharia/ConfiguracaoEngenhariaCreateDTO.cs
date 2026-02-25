namespace Api_ArjSys_Tcc.DTOs.Engenharia;

public class ConfiguracaoEngenhariaCreateDTO
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}

public class ConfiguracaoEngenhariaResponseDTO
{
    public int Id { get; set; }
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}