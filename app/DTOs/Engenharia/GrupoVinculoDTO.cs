namespace Api_ArjSys_Tcc.DTOs.Engenharia;

public class GrupoVinculoCreateDTO
{
    public int GrupoPaiId { get; set; }
    public int GrupoFilhoId { get; set; }
}

public class GrupoVinculoResponseDTO
{
    public int Id { get; set; }
    public int GrupoPaiId { get; set; }
    public string GrupoPaiCodigo { get; set; } = string.Empty;
    public string GrupoPaiDescricao { get; set; } = string.Empty;
    public string GrupoPaiNivel { get; set; } = string.Empty;
    public int GrupoFilhoId { get; set; }
    public string GrupoFilhoCodigo { get; set; } = string.Empty;
    public string GrupoFilhoDescricao { get; set; } = string.Empty;
    public string GrupoFilhoNivel { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}