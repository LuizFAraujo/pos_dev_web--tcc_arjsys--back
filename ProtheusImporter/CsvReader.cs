using System.Text;

namespace ProtheusImporter.Core;

/// <summary>
/// Linha do CSV ja parseada: mapeia nome da coluna -> valor (string crua, sem trim).
/// Preserva os caracteres exatos do CSV (nao aplica normalizacao) — isso e
/// intencional pra flagrar cadastros quebrados no Protheus.
/// </summary>
public sealed class CsvRow
{
    private readonly Dictionary<string, string> _celulas;

    public CsvRow(Dictionary<string, string> celulas)
    {
        _celulas = celulas;
    }

    /// <summary>
    /// Retorna valor da coluna. Se a coluna nao existir no header, retorna string vazia.
    /// Nao faz Trim — a string vem exatamente como no CSV (sem aspas externas).
    /// </summary>
    public string Get(string coluna)
    {
        return _celulas.TryGetValue(coluna, out var v) ? v : string.Empty;
    }

    /// <summary>
    /// Retorna true se a coluna existe no header do CSV (independente do valor).
    /// </summary>
    public bool TemColuna(string coluna) => _celulas.ContainsKey(coluna);
}

/// <summary>
/// Parser CSV que respeita aspas duplas, suporta separador configuravel e
/// detecta automaticamente a linha de cabecalho procurando por todas as colunas
/// obrigatorias informadas.
/// </summary>
public static class CsvReader
{
    /// <summary>
    /// Le o CSV completo e retorna (header, linhas).
    /// Procura o header nas primeiras MaxLinhasBuscaHeader linhas.
    /// Lanca InvalidDataException se header nao for encontrado com as colunas obrigatorias.
    /// </summary>
    public static (IReadOnlyList<string> Header, IReadOnlyList<CsvRow> Linhas) Ler(ImportOptions opts)
    {
        if (!File.Exists(opts.CsvPath))
            throw new FileNotFoundException($"CSV nao encontrado: {opts.CsvPath}", opts.CsvPath);

        var todasLinhas = File.ReadAllLines(opts.CsvPath, opts.Encoding);

        var (indiceHeader, header) = LocalizarHeader(todasLinhas, opts);
        if (indiceHeader < 0)
        {
            throw new InvalidDataException(
                $"Nao encontrei a linha de cabecalho nas primeiras {opts.MaxLinhasBuscaHeader} linhas. " +
                $"Colunas obrigatorias esperadas: {string.Join(", ", opts.ColunasObrigatorias)}.");
        }

        var linhas = new List<CsvRow>(capacity: Math.Max(0, todasLinhas.Length - indiceHeader - 1));

        for (int i = indiceHeader + 1; i < todasLinhas.Length; i++)
        {
            var bruta = todasLinhas[i];
            if (string.IsNullOrWhiteSpace(bruta)) continue;

            var campos = ParsearLinha(bruta, opts.Separador);

            // Padroniza tamanho: linhas com menos campos que o header sao complementadas com vazio.
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int c = 0; c < header.Count; c++)
            {
                dict[header[c]] = c < campos.Count ? campos[c] : string.Empty;
            }

            linhas.Add(new CsvRow(dict));
        }

        return (header, linhas);
    }

    /// <summary>
    /// Procura a linha que contem todas as colunas obrigatorias. Retorna (indice, header).
    /// Se nao achar, retorna (-1, []).
    /// </summary>
    private static (int Indice, IReadOnlyList<string> Header) LocalizarHeader(string[] linhas, ImportOptions opts)
    {
        var limite = Math.Min(linhas.Length, opts.MaxLinhasBuscaHeader);
        for (int i = 0; i < limite; i++)
        {
            var bruta = linhas[i];
            if (string.IsNullOrWhiteSpace(bruta)) continue;

            var campos = ParsearLinha(bruta, opts.Separador);
            if (campos.Count < opts.ColunasObrigatorias.Count) continue;

            var setCampos = new HashSet<string>(campos, StringComparer.Ordinal);
            var temTodas = opts.ColunasObrigatorias.All(setCampos.Contains);

            if (temTodas) return (i, campos);
        }

        return (-1, Array.Empty<string>());
    }

    /// <summary>
    /// Parser simples para uma linha CSV respeitando aspas duplas como qualificador.
    /// Regras:
    /// - Tudo entre aspas duplas e conteudo literal (incluindo o separador).
    /// - "" dentro de campo com aspas = uma aspa literal.
    /// - Separador fora de aspas = divisor de coluna.
    /// - Nao faz trim nos valores — preserva os caracteres como vieram.
    /// </summary>
    private static List<string> ParsearLinha(string linha, char separador)
    {
        var resultado = new List<string>();
        var buffer = new StringBuilder();
        bool dentroAspas = false;

        for (int i = 0; i < linha.Length; i++)
        {
            var ch = linha[i];

            if (dentroAspas)
            {
                if (ch == '"')
                {
                    // "" dentro de aspas = uma aspa literal
                    if (i + 1 < linha.Length && linha[i + 1] == '"')
                    {
                        buffer.Append('"');
                        i++;
                    }
                    else
                    {
                        dentroAspas = false;
                    }
                }
                else
                {
                    buffer.Append(ch);
                }
            }
            else
            {
                if (ch == '"')
                {
                    dentroAspas = true;
                }
                else if (ch == separador)
                {
                    resultado.Add(buffer.ToString());
                    buffer.Clear();
                }
                else
                {
                    buffer.Append(ch);
                }
            }
        }

        resultado.Add(buffer.ToString());
        return resultado;
    }
}
