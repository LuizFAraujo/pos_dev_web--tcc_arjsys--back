using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.DTOs.Engenharia;

public class GrupoProdutoCreateDTO
{
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public NivelGrupo Nivel { get; set; }
    public int QtdCaracteres { get; set; }
    public string? PathDocumentos { get; set; }
    public bool Ativo { get; set; } = true;
}

public class GrupoProdutoResponseDTO
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public NivelGrupo Nivel { get; set; }
    public int QtdCaracteres { get; set; }
    public string? PathDocumentos { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}