using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Api_ArjSys_Tcc.DTOs.Shared;

namespace Api_ArjSys_Tcc.Data.Busca;

/// <summary>
/// Engine de busca paginada genérica para IQueryable.
/// Aplica filtros multi-condição (com lógica E/OU), busca global,
/// ordenação multi-coluna e paginação conforme o contrato BuscaRequest/PaginadoResponse.
/// </summary>
public static class BuscaExtensions
{
    /// <summary>
    /// Aplica filtros, busca global, ordenação e paginação, projeta para o DTO de saída
    /// e retorna a resposta paginada. Total é calculado após filtros e antes da paginação.
    /// </summary>
    public static async Task<PaginadoResponse<TDto>> AplicarBuscaAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        BuscaRequest req,
        IDictionary<string, string> mapaColunas,
        Expression<Func<TEntity, TDto>> projecao,
        string[]? colunasBuscaGlobal = null,
        CancellationToken cancellationToken = default)
    {
        query = AplicarFiltrosEBusca(query, req, mapaColunas, colunasBuscaGlobal);

        var total = await query.CountAsync(cancellationToken);

        query = AplicarOrdenacao(query, req.Ordenacoes, mapaColunas);

        var projetada = query.Select(projecao);

        if (req.Tamanho > 0)
            projetada = projetada
                .Skip((req.Pagina - 1) * req.Tamanho)
                .Take(req.Tamanho);

        var itens = await projetada.ToListAsync(cancellationToken);

        return MontarResposta(itens, total, req);
    }

    /// <summary>
    /// Versão sem projeção: aplica filtros, busca global, ordenação e paginação
    /// e retorna a entidade pura.
    /// </summary>
    public static async Task<PaginadoResponse<TEntity>> AplicarBuscaAsync<TEntity>(
        this IQueryable<TEntity> query,
        BuscaRequest req,
        IDictionary<string, string> mapaColunas,
        string[]? colunasBuscaGlobal = null,
        CancellationToken cancellationToken = default)
    {
        query = AplicarFiltrosEBusca(query, req, mapaColunas, colunasBuscaGlobal);

        var total = await query.CountAsync(cancellationToken);

        query = AplicarOrdenacao(query, req.Ordenacoes, mapaColunas);

        if (req.Tamanho > 0)
            query = query
                .Skip((req.Pagina - 1) * req.Tamanho)
                .Take(req.Tamanho);

        var itens = await query.ToListAsync(cancellationToken);

        return MontarResposta(itens, total, req);
    }

    // ============================================
    // PIPELINE INTERNO
    // ============================================

    private static IQueryable<T> AplicarFiltrosEBusca<T>(
        IQueryable<T> query,
        BuscaRequest req,
        IDictionary<string, string> mapaColunas,
        string[]? colunasBuscaGlobal)
    {
        if (req.Filtros != null)
            foreach (var filtro in req.Filtros)
                query = AplicarFiltroColuna(query, filtro, mapaColunas);

        if (!string.IsNullOrWhiteSpace(req.Busca)
            && colunasBuscaGlobal != null && colunasBuscaGlobal.Length > 0)
            query = AplicarBuscaGlobal(query, req.Busca, colunasBuscaGlobal, mapaColunas);

        return query;
    }

    private static PaginadoResponse<T> MontarResposta<T>(List<T> itens, int total, BuscaRequest req)
        => new()
        {
            Itens = itens,
            Total = total,
            Pagina = req.Pagina,
            Tamanho = req.Tamanho,
            TotalPaginas = req.Tamanho > 0
                ? (int)Math.Ceiling((double)total / req.Tamanho)
                : 1
        };

    // ============================================
    // FILTRO POR COLUNA (multi-condições com E/OU)
    // ============================================

    private static IQueryable<T> AplicarFiltroColuna<T>(
        IQueryable<T> query,
        FiltroColuna filtro,
        IDictionary<string, string> mapaColunas)
    {
        if (filtro.Condicoes.Count == 0) return query;
        if (!mapaColunas.TryGetValue(filtro.Coluna, out var caminho)) return query;

        var param = Expression.Parameter(typeof(T), "x");
        var prop = BuildAcessoPropriedade(param, caminho);

        Expression? expressaoFinal = null;
        Logica logicaProxima = Logica.E;

        foreach (var condicao in filtro.Condicoes)
        {
            var expressaoCondicao = BuildExpressionFiltro(prop, condicao.Operador, condicao.Valor);
            if (expressaoCondicao == null) continue;

            if (expressaoFinal == null)
            {
                expressaoFinal = expressaoCondicao;
            }
            else
            {
                expressaoFinal = logicaProxima == Logica.Ou
                    ? Expression.OrElse(expressaoFinal, expressaoCondicao)
                    : Expression.AndAlso(expressaoFinal, expressaoCondicao);
            }

            logicaProxima = condicao.Logica ?? Logica.E;
        }

        if (expressaoFinal == null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expressaoFinal, param);
        return query.Where(lambda);
    }

    // ============================================
    // BUSCA GLOBAL (mesmo termo em N colunas, OR)
    // ============================================

    private static IQueryable<T> AplicarBuscaGlobal<T>(
        IQueryable<T> query,
        string termo,
        string[] colunasBuscaGlobal,
        IDictionary<string, string> mapaColunas)
    {
        var param = Expression.Parameter(typeof(T), "x");
        Expression? expressaoFinal = null;

        foreach (var coluna in colunasBuscaGlobal)
        {
            if (!mapaColunas.TryGetValue(coluna, out var caminho)) continue;

            var prop = BuildAcessoPropriedade(param, caminho);
            if (prop.Type != typeof(string)) continue;

            var contemExpr = ConstruirChamadaString(prop, "Contains", termo);
            if (contemExpr == null) continue;

            expressaoFinal = expressaoFinal == null
                ? contemExpr
                : Expression.OrElse(expressaoFinal, contemExpr);
        }

        if (expressaoFinal == null) return query;

        var lambda = Expression.Lambda<Func<T, bool>>(expressaoFinal, param);
        return query.Where(lambda);
    }

    // ============================================
    // ORDENAÇÃO MULTI-COLUNA
    // ============================================

    private static IQueryable<T> AplicarOrdenacao<T>(
        IQueryable<T> query,
        List<Ordenacao>? ordenacoes,
        IDictionary<string, string> mapaColunas)
    {
        if (ordenacoes == null || ordenacoes.Count == 0) return query;

        IOrderedQueryable<T>? ordenada = null;

        foreach (var ord in ordenacoes)
        {
            if (!mapaColunas.TryGetValue(ord.Coluna, out var caminho)) continue;

            var param = Expression.Parameter(typeof(T), "x");
            var prop = BuildAcessoPropriedade(param, caminho);
            var lambda = Expression.Lambda(prop, param);

            var nomeMetodo = ordenada == null
                ? (ord.Direcao == Direcao.Asc ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending))
                : (ord.Direcao == Direcao.Asc ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending));

            var metodoGenerico = typeof(Queryable).GetMethods()
                .First(m => m.Name == nomeMetodo && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), prop.Type);

            var queryAlvo = (object)(ordenada ?? query);
            ordenada = (IOrderedQueryable<T>)metodoGenerico.Invoke(null, new[] { queryAlvo, lambda })!;
        }

        return ordenada ?? query;
    }

    // ============================================
    // BUILDER DE EXPRESSION POR OPERADOR
    // ============================================

    private static Expression? BuildExpressionFiltro(Expression prop, Operador op, JsonElement? valor)
    {
        // Operadores que não usam valor
        if (op == Operador.Nulo) return ComparacaoComNull(prop, igual: true);
        if (op == Operador.NaoNulo) return ComparacaoComNull(prop, igual: false);

        if (!valor.HasValue) return null;
        var v = valor.Value;

        // Operadores que esperam array
        if (op == Operador.Em) return ConstruirEmOuNaoEm(prop, v, negar: false);
        if (op == Operador.NaoEm) return ConstruirEmOuNaoEm(prop, v, negar: true);
        if (op == Operador.Entre) return ConstruirEntre(prop, v);

        // Operadores de string
        if (op == Operador.Contem)
            return ConstruirChamadaString(prop, "Contains", ExtrairString(v));

        if (op == Operador.NaoContem)
        {
            var contains = ConstruirChamadaString(prop, "Contains", ExtrairString(v));
            return contains == null ? null : Expression.Not(contains);
        }

        if (op == Operador.ComecaCom)
            return ConstruirChamadaString(prop, "StartsWith", ExtrairString(v));

        if (op == Operador.TerminaCom)
            return ConstruirChamadaString(prop, "EndsWith", ExtrairString(v));

        // Operadores de comparação com valor único
        var convertido = ConverterJsonElement(v, prop.Type);
        if (convertido == null && !PermitirNull(prop.Type)) return null;

        var constante = Expression.Constant(convertido, prop.Type);

        return op switch
        {
            Operador.Igual => Expression.Equal(prop, constante),
            Operador.Diferente => Expression.NotEqual(prop, constante),
            Operador.MaiorQue => Expression.GreaterThan(prop, constante),
            Operador.MenorQue => Expression.LessThan(prop, constante),
            Operador.MaiorOuIgual => Expression.GreaterThanOrEqual(prop, constante),
            Operador.MenorOuIgual => Expression.LessThanOrEqual(prop, constante),
            _ => null
        };
    }

    private static Expression ComparacaoComNull(Expression prop, bool igual)
    {
        var propNullable = PermitirNull(prop.Type)
            ? prop
            : Expression.Convert(prop, MakeNullable(prop.Type));
        var nullConst = Expression.Constant(null, propNullable.Type);
        return igual
            ? Expression.Equal(propNullable, nullConst)
            : Expression.NotEqual(propNullable, nullConst);
    }

    private static Expression? ConstruirChamadaString(Expression prop, string metodo, string? valor)
    {
        if (valor == null || prop.Type != typeof(string)) return null;
        var info = typeof(string).GetMethod(metodo, new[] { typeof(string) });
        if (info == null) return null;
        return Expression.Call(prop, info, Expression.Constant(valor));
    }

    private static Expression? ConstruirEntre(Expression prop, JsonElement valor)
    {
        if (valor.ValueKind != JsonValueKind.Array || valor.GetArrayLength() != 2) return null;

        var min = ConverterJsonElement(valor[0], prop.Type);
        var max = ConverterJsonElement(valor[1], prop.Type);
        if (min == null || max == null) return null;

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(prop, Expression.Constant(min, prop.Type)),
            Expression.LessThanOrEqual(prop, Expression.Constant(max, prop.Type)));
    }

    private static Expression? ConstruirEmOuNaoEm(Expression prop, JsonElement valor, bool negar)
    {
        if (valor.ValueKind != JsonValueKind.Array) return null;

        var tipoLista = typeof(List<>).MakeGenericType(prop.Type);
        var lista = (IList)Activator.CreateInstance(tipoLista)!;

        foreach (var item in valor.EnumerateArray())
        {
            var convertido = ConverterJsonElement(item, prop.Type);
            if (convertido != null) lista.Add(convertido);
        }

        if (lista.Count == 0) return null;

        var contains = tipoLista.GetMethod("Contains", new[] { prop.Type });
        if (contains == null) return null;

        Expression call = Expression.Call(Expression.Constant(lista, tipoLista), contains, prop);
        return negar ? Expression.Not(call) : call;
    }

    // ============================================
    // ACESSO A PROPRIEDADE (dot notation)
    // ============================================

    private static Expression BuildAcessoPropriedade(ParameterExpression param, string caminho)
    {
        Expression atual = param;
        foreach (var nome in caminho.Split('.'))
            atual = Expression.Property(atual, nome);
        return atual;
    }

    // ============================================
    // CONVERSÃO DE JsonElement → tipo destino
    // ============================================

    private static object? ConverterJsonElement(JsonElement valor, Type tipoDestino)
    {
        var tipoBase = Nullable.GetUnderlyingType(tipoDestino) ?? tipoDestino;

        try
        {
            if (valor.ValueKind == JsonValueKind.Null) return null;

            if (tipoBase == typeof(string))
                return valor.ValueKind == JsonValueKind.String ? valor.GetString() : valor.ToString();

            if (tipoBase == typeof(int))
                return valor.ValueKind == JsonValueKind.String
                    ? int.Parse(valor.GetString()!, CultureInfo.InvariantCulture)
                    : valor.GetInt32();

            if (tipoBase == typeof(long))
                return valor.ValueKind == JsonValueKind.String
                    ? long.Parse(valor.GetString()!, CultureInfo.InvariantCulture)
                    : valor.GetInt64();

            if (tipoBase == typeof(decimal))
                return valor.ValueKind == JsonValueKind.String
                    ? decimal.Parse(valor.GetString()!, CultureInfo.InvariantCulture)
                    : valor.GetDecimal();

            if (tipoBase == typeof(double))
                return valor.ValueKind == JsonValueKind.String
                    ? double.Parse(valor.GetString()!, CultureInfo.InvariantCulture)
                    : valor.GetDouble();

            if (tipoBase == typeof(bool))
                return valor.ValueKind == JsonValueKind.String
                    ? bool.Parse(valor.GetString()!)
                    : valor.GetBoolean();

            if (tipoBase == typeof(DateTime))
                return valor.ValueKind == JsonValueKind.String
                    ? DateTime.Parse(valor.GetString()!, CultureInfo.InvariantCulture)
                    : valor.GetDateTime();

            if (tipoBase.IsEnum)
            {
                var raw = valor.ValueKind == JsonValueKind.String ? valor.GetString() : valor.GetRawText();
                return Enum.Parse(tipoBase, raw!, ignoreCase: true);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static string? ExtrairString(JsonElement valor) =>
        valor.ValueKind == JsonValueKind.String ? valor.GetString() : valor.ToString();

    private static bool PermitirNull(Type tipo) =>
        !tipo.IsValueType || Nullable.GetUnderlyingType(tipo) != null;

    private static Type MakeNullable(Type tipo) =>
        tipo.IsValueType && Nullable.GetUnderlyingType(tipo) == null
            ? typeof(Nullable<>).MakeGenericType(tipo)
            : tipo;
}
