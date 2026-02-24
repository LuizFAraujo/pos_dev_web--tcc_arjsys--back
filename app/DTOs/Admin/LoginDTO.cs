using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.DTOs.Admin;

public class LoginRequestDTO
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class LoginResponseDTO
{
    public int FuncionarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Setor { get; set; }
    public List<PermissaoResponseDTO> Permissoes { get; set; } = [];
}