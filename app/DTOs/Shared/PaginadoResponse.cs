namespace Api_ArjSys_Tcc.DTOs.Shared;

/// <summary>
/// Resposta padrão de endpoints paginados.
/// Encapsula a lista da página corrente e metadados de paginação
/// (total geral, página atual, tamanho da página, total de páginas).
/// </summary>
public class PaginadoResponse<T>
{
    public List<T> Itens { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int TotalPaginas { get; set; }
}
