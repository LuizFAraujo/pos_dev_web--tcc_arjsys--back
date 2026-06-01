using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api_ArjSys_Tcc.DTOs.Shared;

/// <summary>
/// Operadores de filtro suportados em buscas paginadas.
/// Serializados como camelCase no JSON via JsonStringEnumMemberName em cada membro.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Operador>))]
public enum Operador
{
    [JsonStringEnumMemberName("igual")] Igual,
    [JsonStringEnumMemberName("diferente")] Diferente,
    [JsonStringEnumMemberName("contem")] Contem,
    [JsonStringEnumMemberName("naoContem")] NaoContem,
    [JsonStringEnumMemberName("comecaCom")] ComecaCom,
    [JsonStringEnumMemberName("terminaCom")] TerminaCom,
    [JsonStringEnumMemberName("maiorQue")] MaiorQue,
    [JsonStringEnumMemberName("menorQue")] MenorQue,
    [JsonStringEnumMemberName("maiorOuIgual")] MaiorOuIgual,
    [JsonStringEnumMemberName("menorOuIgual")] MenorOuIgual,
    [JsonStringEnumMemberName("entre")] Entre,
    [JsonStringEnumMemberName("em")] Em,
    [JsonStringEnumMemberName("naoEm")] NaoEm,
    [JsonStringEnumMemberName("nulo")] Nulo,
    [JsonStringEnumMemberName("naoNulo")] NaoNulo
}

/// <summary>
/// Direção de ordenação de uma coluna.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Direcao>))]
public enum Direcao
{
    [JsonStringEnumMemberName("asc")] Asc,
    [JsonStringEnumMemberName("desc")] Desc
}

/// <summary>
/// Lógica de combinação entre uma condição e a próxima dentro do mesmo filtro de coluna.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<Logica>))]
public enum Logica
{
    [JsonStringEnumMemberName("e")] E,
    [JsonStringEnumMemberName("ou")] Ou
}

/// <summary>
/// Uma condição individual de filtro: operador + valor + lógica com a próxima condição.
/// Valor é JsonElement para suportar tanto string única quanto array de strings
/// (operadores Em/NaoEm/Entre). Operadores Nulo/NaoNulo dispensam valor.
/// </summary>
public class Condicao
{
    public Operador Operador { get; set; }
    public JsonElement? Valor { get; set; }
    public Logica? Logica { get; set; }
}

/// <summary>
/// Filtro de uma coluna específica com 1 ou mais condições combinadas por E/OU.
/// </summary>
public class FiltroColuna
{
    public string Coluna { get; set; } = string.Empty;
    public List<Condicao> Condicoes { get; set; } = new();
}

/// <summary>
/// Critério de ordenação por coluna. Várias ordenações podem ser aplicadas em sequência.
/// </summary>
public class Ordenacao
{
    public string Coluna { get; set; } = string.Empty;
    public Direcao Direcao { get; set; } = Direcao.Asc;
}

/// <summary>
/// Body padrão de endpoints de busca paginada (POST /buscar).
/// Suporta paginação, busca textual global, filtros multi-condição e ordenação multi-coluna.
/// Tamanho = 0 retorna tudo (sem paginação aplicada).
/// </summary>
public class BuscaRequest
{
    public int Pagina { get; set; } = 1;
    public int Tamanho { get; set; } = 0;
    public string? Busca { get; set; }
    public List<FiltroColuna>? Filtros { get; set; }
    public List<Ordenacao>? Ordenacoes { get; set; }

    /// <summary>
    /// Colunas onde aplicar a busca textual (Busca). Quando vazio/null,
    /// usa o default definido pelo service (colunasBuscaGlobal). Quando vier,
    /// substitui o default permitindo o front escolher onde pesquisar.
    /// </summary>
    public List<string>? ColunasBusca { get; set; }
}
