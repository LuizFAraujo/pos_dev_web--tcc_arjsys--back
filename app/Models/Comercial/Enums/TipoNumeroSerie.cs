namespace Api_ArjSys_Tcc.Models.Comercial.Enums;

/// <summary>
/// Tipo do Número de Série.
/// Normal = venda já realizada.
/// VendaFutura = pré-pedido, depende de aprovação externa.
/// </summary>
public enum TipoNumeroSerie
{
    Normal,
    VendaFutura
}