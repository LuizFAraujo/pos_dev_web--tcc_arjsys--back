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
        // Total geral (tabela inteira, sem filtros nem busca) — usado no rodapé
        var totalGeral = await query.CountAsync(cancellationToken);

        query = AplicarFiltrosEBusca(query, req, mapaColunas, colunasBuscaGlobal);

        var total = await query.CountAsync(cancellationToken);

        query = AplicarOrdenacao(query, req.Ordenacoes, mapaColunas);

        var projetada = query.Select(projecao);

        if (req.Tamanho > 0)
            projetada = projetada
                .Skip((req.Pagina - 1) * req.Tamanho)
                .Take(req.Tamanho);

        var itens = await projetada.ToListAsync(cancellationToken);

        return MontarResposta(itens, total, totalGeral, req);
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
        var totalGeral = await query.CountAsync(cancellationToken);

        query = AplicarFiltrosEBusca(query, req, mapaColunas, colunasBuscaGlobal);

        var total = await query.CountAsync(cancellationToken);

        query = AplicarOrdenacao(query, req.Ordenacoes, mapaColunas);

        if (req.Tamanho > 0)
            query = query
                .Skip((req.Pagina - 1) * req.Tamanho)
                .Take(req.Tamanho);

        var itens = await query.ToListAsync(cancellationToken);

        return MontarResposta(itens, total, totalGeral, req);
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

        if (!string.IsNullOrWhiteSpace(req.Busca))
        {
            // Front pode sobrescrever as colunas via req.ColunasBusca; senao
            // usa o default do service (colunasBuscaGlobal).
            var colunas = (req.ColunasBusca != null && req.ColunasBusca.Count > 0)
                ? req.ColunasBusca.ToArray()
                : colunasBuscaGlobal;

            if (colunas != null && colunas.Length > 0)
                query = AplicarBuscaGlobal(query, req.Busca, colunas, mapaColunas);
        }

        return query;
    }

    private static PaginadoResponse<T> MontarResposta<T>(List<T> itens, int total, int totalGeral, BuscaRequest req)
        => new()
        {
            Itens = itens,
            Total = total,
            TotalGeral = totalGeral,
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

            Expression? colunaExpr = null;

            if (prop.Type == typeof(string))
            {
                // String: EF.Functions.Like → LIKE → case-insensitive no SQLite
                colunaExpr = ConstruirLike(prop, termo, curinga: "%{0}%");
            }
            else if (prop.Type.IsEnum)
            {
                // Enum: enumera membros cujo NOME contem o termo (case-insensitive)
                // e gera OR de igualdades. EF traduz pra WHERE col IN (...) no SQL.
                // Funciona porque o padrao do projeto eh HasConversion<string>() nos enums.
                colunaExpr = ConstruirEnumContem(prop, termo);
            }
            // outros tipos: pula

            if (colunaExpr == null) continue;

            expressaoFinal = expressaoFinal == null
                ? colunaExpr
                : Expression.OrElse(expressaoFinal, colunaExpr);
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

        // Operadores de string — usam EF.Functions.Like (LIKE no SQL).
        // No SQLite, LIKE é case-insensitive por default pra ASCII; em Postgres,
        // futuramente trocar pra EF.Functions.ILike. Acento ainda nao normaliza.
        if (op == Operador.Contem)
            return ConstruirLike(prop, ExtrairString(v), curinga: "%{0}%");

        if (op == Operador.NaoContem)
        {
            var like = ConstruirLike(prop, ExtrairString(v), curinga: "%{0}%");
            return like == null ? null : Expression.Not(like);
        }

        if (op == Operador.ComecaCom)
            return ConstruirLike(prop, ExtrairString(v), curinga: "{0}%");

        if (op == Operador.TerminaCom)
            return ConstruirLike(prop, ExtrairString(v), curinga: "%{0}");

        // Operadores de comparação com valor único
        var convertido = ConverterJsonElement(v, prop.Type);
        if (convertido == null && !PermitirNull(prop.Type)) return null;

        // Em string: LOWER nos dois lados pra comparacao case-insensitive
        // (dados gravados ficam intactos; so a comparacao normaliza)
        var (propComp, valorComp) = NormalizarStringParaLower(prop, convertido);
        var constante = Expression.Constant(valorComp, prop.Type);

        return op switch
        {
            Operador.Igual => Expression.Equal(propComp, constante),
            Operador.Diferente => Expression.NotEqual(propComp, constante),
            Operador.MaiorQue => Expression.GreaterThan(propComp, constante),
            Operador.MenorQue => Expression.LessThan(propComp, constante),
            Operador.MaiorOuIgual => Expression.GreaterThanOrEqual(propComp, constante),
            Operador.MenorOuIgual => Expression.LessThanOrEqual(propComp, constante),
            _ => null
        };
    }

    /// <summary>
    /// Para strings, aplica string.ToLower() na propriedade e converte o valor para lower.
    /// Pra outros tipos, devolve sem mudanca. EF Core traduz ToLower() para LOWER(col) no SQL,
    /// e funciona tanto no SQLite quanto no Postgres (futuro).
    /// </summary>
    private static (Expression Prop, object? Valor) NormalizarStringParaLower(Expression prop, object? valor)
    {
        if (prop.Type != typeof(string)) return (prop, valor);
        var metodo = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        var propLower = Expression.Call(prop, metodo);
        var valorLower = valor is string s ? s.ToLowerInvariant() : valor;
        return (propLower, valorLower);
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

    /// <summary>
    /// Constroi expressao EF.Functions.Like(prop, pattern) onde pattern = prefixo + valor + sufixo.
    /// O LIKE no SQLite é case-insensitive por padrao pra ASCII (não pra acentos).
    /// Caracteres especiais de LIKE (% _) digitados pelo usuario funcionam como curinga
    /// — aceitavel pra busca interativa de cadastros.
    /// </summary>
    private static Expression? ConstruirLike(Expression prop, string? valor, string curinga)
    {
        if (valor == null || prop.Type != typeof(string)) return null;

        var (prefixo, sufixo) = ExtrairCuringa(curinga);
        var pattern = prefixo + valor + sufixo;

        var metodo = typeof(Microsoft.EntityFrameworkCore.DbFunctionsExtensions)
            .GetMethod(
                nameof(Microsoft.EntityFrameworkCore.DbFunctionsExtensions.Like),
                new[]
                {
                    typeof(Microsoft.EntityFrameworkCore.DbFunctions),
                    typeof(string),
                    typeof(string),
                });
        if (metodo == null) return null;

        var efFunctions = Expression.Constant(Microsoft.EntityFrameworkCore.EF.Functions);
        return Expression.Call(metodo, efFunctions, prop, Expression.Constant(pattern));
    }

    private static (string Prefixo, string Sufixo) ExtrairCuringa(string padrao) => padrao switch
    {
        "%{0}%" => ("%", "%"),
        "{0}%" => ("", "%"),
        "%{0}" => ("%", ""),
        _ => ("", ""),
    };

    /// <summary>
    /// Para enum: enumera os membros cujo nome contem o termo (case-insensitive)
    /// e gera (prop == M1 || prop == M2 || ...). EF Core traduz isso pra
    /// "WHERE col IN ('M1','M2',...)" no SQL, funcionando independente de o enum
    /// estar persistido como string ou int.
    /// </summary>
    private static Expression? ConstruirEnumContem(Expression prop, string termo)
    {
        if (!prop.Type.IsEnum) return null;

        var termoLower = termo.ToLowerInvariant();
        var matching = Enum.GetNames(prop.Type)
            .Where(name => name.ToLowerInvariant().Contains(termoLower))
            .Select(name => Enum.Parse(prop.Type, name))
            .ToArray();

        if (matching.Length == 0) return null;

        Expression? cadeia = null;
        foreach (var membro in matching)
        {
            var eq = Expression.Equal(prop, Expression.Constant(membro, prop.Type));
            cadeia = cadeia == null ? eq : Expression.OrElse(cadeia, eq);
        }
        return cadeia;
    }

    private static Expression? ConstruirEntre(Expression prop, JsonElement valor)
    {
        if (valor.ValueKind != JsonValueKind.Array || valor.GetArrayLength() != 2) return null;

        var min = ConverterJsonElement(valor[0], prop.Type);
        var max = ConverterJsonElement(valor[1], prop.Type);
        if (min == null || max == null) return null;

        // Em string: LOWER nos dois lados pra range case-insensitive
        var (propMin, minNorm) = NormalizarStringParaLower(prop, min);
        var (propMax, maxNorm) = NormalizarStringParaLower(prop, max);

        return Expression.AndAlso(
            Expression.GreaterThanOrEqual(propMin, Expression.Constant(minNorm, prop.Type)),
            Expression.LessThanOrEqual(propMax, Expression.Constant(maxNorm, prop.Type)));
    }

    private static Expression? ConstruirEmOuNaoEm(Expression prop, JsonElement valor, bool negar)
    {
        if (valor.ValueKind != JsonValueKind.Array) return null;

        var tipoLista = typeof(List<>).MakeGenericType(prop.Type);
        var lista = (IList)Activator.CreateInstance(tipoLista)!;
        var ehString = prop.Type == typeof(string);

        foreach (var item in valor.EnumerateArray())
        {
            var convertido = ConverterJsonElement(item, prop.Type);
            if (convertido == null) continue;
            // Em string: normaliza itens pra lower antes de entrar na lista
            if (ehString && convertido is string s)
                lista.Add(s.ToLowerInvariant());
            else
                lista.Add(convertido);
        }

        if (lista.Count == 0) return null;

        var contains = tipoLista.GetMethod("Contains", new[] { prop.Type });
        if (contains == null) return null;

        // Em string: comparar com prop.ToLower() pra fechar o ciclo case-insensitive
        Expression propComp = prop;
        if (ehString)
        {
            var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            propComp = Expression.Call(prop, toLower);
        }

        Expression call = Expression.Call(Expression.Constant(lista, tipoLista), contains, propComp);
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
