using Api_ArjSys_Tcc.Models.Producao.Enums;

namespace Api_ArjSys_Tcc.Models.Producao;

/// <summary>
/// Log de eventos da Ordem de Produção.
/// Registra mudanças de status e apontamentos de produção.
/// </summary>
public class OrdemProducaoHistorico : BaseEntity
{
    /// <summary>FK para a Ordem de Produção</summary>
    public int OrdemProducaoId { get; set; }
    public OrdemProducao OrdemProducao { get; set; } = null!;

    /// <summary>Tipo do evento</summary>
    public EventoOrdemProducao Evento { get; set; }

    /// <summary>Status da OP antes da mudança (null em Criada e Apontamento)</summary>
    public StatusOrdemProducao? StatusAnterior { get; set; }

    /// <summary>Status da OP após a mudança (null em Apontamento)</summary>
    public StatusOrdemProducao? StatusNovo { get; set; }

    /// <summary>Justificativa (obrigatória em Pausada e Cancelada)</summary>
    public string? Justificativa { get; set; }

    /// <summary>Detalhe livre (usado em Apontamento: "Parafuso M10 +5un")</summary>
    public string? Detalhe { get; set; }

    /// <summary>Data/hora do evento</summary>
    public DateTime DataHora { get; set; }
}
