using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.DTOs.Admin;

/// <summary>
/// DTO de criação de notificação.
/// </summary>
public class NotificacaoCreateDTO
{
    public ModuloSistema ModuloDestino { get; set; }
    public TipoNotificacao Tipo { get; set; } = TipoNotificacao.Info;
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string? OrigemTabela { get; set; }
    public int? OrigemId { get; set; }
}

/// <summary>
/// DTO de resposta de notificação.
/// </summary>
public class NotificacaoResponseDTO
{
    public int Id { get; set; }
    public ModuloSistema ModuloDestino { get; set; }
    public TipoNotificacao Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool Lida { get; set; }
    public DateTime? DataLeitura { get; set; }
    public string? OrigemTabela { get; set; }
    public int? OrigemId { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ModificadoEm { get; set; }
}
