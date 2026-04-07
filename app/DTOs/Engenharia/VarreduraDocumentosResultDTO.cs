namespace Api_ArjSys_Tcc.DTOs.Engenharia;

public class VarreduraDocumentosResultDTO
{
    /// <summary>
    /// Total de produtos no filtro (antes de paginar)
    /// </summary>
    public int TotalGeral { get; set; }

    /// <summary>
    /// Quantos foram verificados neste lote
    /// </summary>
    public int TotalVerificados { get; set; }

    public int ComPasta { get; set; }
    public int ComDocumento { get; set; }
    public int PastaVazia { get; set; }
    public int SemPasta { get; set; }
    public int Atualizados { get; set; }
}