using Api_ArjSys_Tcc.Models.Admin.Enums;

namespace Api_ArjSys_Tcc.Models.Admin;

/// <summary>
/// Notificação genérica direcionada a um módulo do sistema.
/// Pode registrar origem (tabela + id) para rastreabilidade, mas origem é opcional.
/// </summary>
public class Notificacao : BaseEntity
{
    /// <summary>Módulo destinatário da notificação</summary>
    public ModuloSistema ModuloDestino { get; set; }

    /// <summary>Tipo da notificação (define visual/cor no front)</summary>
    public TipoNotificacao Tipo { get; set; } = TipoNotificacao.Info;

    /// <summary>Título curto</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Corpo/mensagem</summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Flag de lida</summary>
    public bool Lida { get; set; }

    /// <summary>Quando a notificação foi marcada como lida (null enquanto não lida)</summary>
    public DateTime? DataLeitura { get; set; }

    /// <summary>Tabela de origem (opcional, rastreabilidade). Ex: "Comercial_PedidosVenda"</summary>
    public string? OrigemTabela { get; set; }

    /// <summary>Id do registro de origem (opcional, rastreabilidade)</summary>
    public int? OrigemId { get; set; }
}
