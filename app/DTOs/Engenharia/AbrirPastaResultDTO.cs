namespace Api_ArjSys_Tcc.DTOs.Engenharia;

/// <summary>Resultado ao abrir pasta de documentos de um produto</summary>
public class AbrirPastaResultDTO
{
    /// <summary>Caminho completo da pasta</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Se true, o Explorer foi aberto com sucesso</summary>
    public bool Aberto { get; set; }
}

/// <summary>Resultado ao abrir arquivo de documento de um produto</summary>
public class AbrirDocumentoResultDTO
{
    /// <summary>Caminho completo do arquivo</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Extensão do arquivo aberto (ex: "pdf")</summary>
    public string Extensao { get; set; } = string.Empty;

    /// <summary>Se true, o arquivo foi aberto com sucesso</summary>
    public bool Aberto { get; set; }
}

/// <summary>Lista de extensões de documentos encontrados para um produto</summary>
public class ExtensoesDocumentoResultDTO
{
    /// <summary>Caminho completo da pasta</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Extensões encontradas (ex: ["pdf", "dwg", "slddrw"])</summary>
    public List<string> Extensoes { get; set; } = [];
}