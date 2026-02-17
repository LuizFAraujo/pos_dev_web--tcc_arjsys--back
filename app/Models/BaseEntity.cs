namespace Api_ArjSys_Tcc.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ModificadoEm { get; set; }
    public string? CriadoPor { get; set; }
    public string? ModificadoPor { get; set; }
}