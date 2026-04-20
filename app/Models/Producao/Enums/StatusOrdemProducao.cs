namespace Api_ArjSys_Tcc.Models.Producao.Enums;

/// <summary>
/// Status de uma Ordem de Produção.
///
/// Fluxo normal: Pendente → Andamento → Concluida
/// Especiais (manuais, com justificativa): Pausada, Cancelada
///
/// Status de Master e Filhas são independentes — para saber o andamento geral
/// de uma Master, consultar o status de suas filhas.
/// </summary>
public enum StatusOrdemProducao
{
    /// <summary>OP criada, aguardando início da produção.</summary>
    Pendente,

    /// <summary>Produção em andamento.</summary>
    Andamento,

    /// <summary>Produção interrompida (manual, com justificativa).</summary>
    Pausada,

    /// <summary>Produção finalizada (terminal do fluxo normal).</summary>
    Concluida,

    /// <summary>OP cancelada (manual, com justificativa, terminal).</summary>
    Cancelada
}
