namespace Api_ArjSys_Tcc.Models.Engenharia;

/// <summary>
/// Path alternativo de documentos por prefixo (GrupoProduto Coluna1).
/// Substitui o campo PathDocumentos do GrupoProduto para a lógica de varredura.
/// </summary>
public class PathDocumentos : BaseEntity
{
    /// <summary>FK para GrupoProduto (apenas Coluna1 — prefixos)</summary>
    public int GrupoProdutoId { get; set; }
    public GrupoProduto GrupoProduto { get; set; } = null!;

    /// <summary>Caminho alternativo para documentos deste prefixo</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Se true, organiza em subpasta do prefixo dentro do path</summary>
    public bool ControlarPorPrefixo { get; set; } = false;

    /// <summary>Se false, ignora este path e o prefixo usa o path raiz</summary>
    public bool Ativo { get; set; } = true;
}