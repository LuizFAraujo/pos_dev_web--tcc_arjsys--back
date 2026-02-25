namespace Api_ArjSys_Tcc.Models.Engenharia;

public class ConfiguracaoEngenharia : BaseEntity
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}