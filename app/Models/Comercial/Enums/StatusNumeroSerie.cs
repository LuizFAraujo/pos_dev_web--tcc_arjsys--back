namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Status do Número de Série.
/// Aguardando = NS criado antes da aprovação do PV (venda futura).
/// AguardandoEntrega = projeto finalizado, pronto pra entregar.
/// </summary>
public enum StatusNumeroSerie
{
    Aguardando,
    EmAndamento,
    AguardandoEntrega,
    Entregue,
    Cancelado
}