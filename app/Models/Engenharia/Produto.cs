using Api_ArjSys_Tcc.Models.Engenharia.Enums;

namespace Api_ArjSys_Tcc.Models.Engenharia;

/// <summary>
/// Produto cadastrado no módulo de Engenharia.
/// Código segue máscara: XX.YYY.ZZZZ.NNN.0000 (Coluna1.Coluna2.Coluna3.Seq.Fixo)
/// </summary>
public class Produto : BaseEntity
{
    /// <summary>Código único do produto (máscara inteligente)</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Descrição curta do produto</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Descrição completa/detalhada (opcional)</summary>
    public string? DescricaoCompleta { get; set; }

    /// <summary>Unidade de medida (UN, KG, MT, etc.)</summary>
    public UnidadeMedida Unidade { get; set; }

    /// <summary>Tipo do produto (Fabricado, Comprado, etc.)</summary>
    public TipoProduto Tipo { get; set; }

    /// <summary>Peso em kg (opcional)</summary>
    public decimal? Peso { get; set; }

    /// <summary>Se false, produto inativo</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Indica se existe pasta com nome do código no path esperado</summary>
    public bool TemPasta { get; set; } = false;

    /// <summary>Indica se existe arquivo {codigo}.* dentro da pasta</summary>
    public bool TemDocumento { get; set; } = false;
}