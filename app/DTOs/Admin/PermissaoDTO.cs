using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.DTOs.Admin;

public class PermissaoCreateDTO
{
    public int FuncionarioId { get; set; }
    public ModuloSistema Modulo { get; set; }
    public NivelAcesso Nivel { get; set; }
}

public class PermissaoResponseDTO
{
    public int Id { get; set; }
    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;
    public ModuloSistema Modulo { get; set; }
    public NivelAcesso Nivel { get; set; }
    public DateTime CriadoEm { get; set; }
}