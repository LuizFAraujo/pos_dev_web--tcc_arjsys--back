namespace Api_ArjSys_Tcc.Models.Producao.Enums;

/// <summary>
/// Eventos registrados no histórico da Ordem de Produção.
/// </summary>
public enum EventoOrdemProducao
{
    /// <summary>OP criada (evento inicial, sem StatusAnterior).</summary>
    Criada,

    /// <summary>Produção iniciada (Pendente → Andamento).</summary>
    Iniciada,

    /// <summary>OP pausada (manual, com justificativa).</summary>
    Pausada,

    /// <summary>OP retomada (saiu de Pausada).</summary>
    Retomada,

    /// <summary>Produção finalizada (Andamento → Concluida).</summary>
    Concluida,

    /// <summary>OP cancelada (manual, com justificativa).</summary>
    Cancelada,

    /// <summary>Apontamento de quantidade produzida em um item.</summary>
    Apontamento
}
