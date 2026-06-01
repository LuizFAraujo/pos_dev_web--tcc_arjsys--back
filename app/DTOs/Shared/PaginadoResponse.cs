namespace Api_ArjSys_Tcc.DTOs.Shared;

/// <summary>
/// Resposta padrão de endpoints paginados.
/// Encapsula a lista da página corrente e metadados de paginação
/// (total filtrado, total geral da tabela, página atual, tamanho da página, total de páginas).
/// </summary>
public class PaginadoResponse<T>
{
    public List<T> Itens { get; set; } = new();

    /// <summary>Total de registros após filtros e busca.</summary>
    public int Total { get; set; }

    /// <summary>
    /// Total de registros da tabela INTEIRA, sem filtros nem busca.
    /// Usado no rodapé do grid como "N de TotalGeral registros".
    /// </summary>
    public int TotalGeral { get; set; }

    public int Pagina { get; set; }
    public int Tamanho { get; set; }
    public int TotalPaginas { get; set; }
}
