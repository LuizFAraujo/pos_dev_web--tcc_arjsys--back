using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.Models.Admin;

public class Permissao : BaseEntity
{
    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; } = null!;

    public ModuloSistema Modulo { get; set; }
    public NivelAcesso Nivel { get; set; }
}