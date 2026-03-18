namespace Api_ArjSys_Tcc.DTOs.Engenharia;

/// <summary>Entrada — usado no POST</summary>
public class PathDocumentosCreateDTO
{
    /// <summary>FK para GrupoProduto (apenas Coluna1 — prefixos)</summary>
    public int GrupoProdutoId { get; set; }

    /// <summary>Caminho alternativo para documentos deste prefixo</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Se true, organiza em subpasta do prefixo dentro do path</summary>
    public bool ControlarPorPrefixo { get; set; } = false;

    /// <summary>Se false, ignora este path e o prefixo usa o path raiz</summary>
    public bool Ativo { get; set; } = true;
}

/// <summary>Entrada — usado no PUT (sem GrupoProdutoId, não pode trocar o prefixo)</summary>
public class PathDocumentosUpdateDTO
{
    /// <summary>Caminho alternativo para documentos deste prefixo</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Se true, organiza em subpasta do prefixo dentro do path</summary>
    public bool ControlarPorPrefixo { get; set; } = false;

    /// <summary>Se false, ignora este path e o prefixo usa o path raiz</summary>
    public bool Ativo { get; set; } = true;
}

/// <summary>Saída — retorno dos endpoints</summary>
public class PathDocumentosResponseDTO
{
    public int Id { get; set; }
    public int GrupoProdutoId { get; set; }

    /// <summary>Código do prefixo (ex: "10", "30")</summary>
    public string GrupoCodigo { get; set; } = string.Empty;

    /// <summary>Descrição do grupo/prefixo</summary>
    public string GrupoDescricao { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;
    public bool ControlarPorPrefixo { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}