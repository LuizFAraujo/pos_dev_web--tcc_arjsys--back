using System.Globalization;

namespace ProtheusImporter.Core;

/// <summary>
/// Leitor simples de arquivo .ini (secoes entre colchetes, pares chave=valor).
/// Ignora linhas vazias e comentarios iniciados com # ou ;.
/// </summary>
public sealed class IniConfig
{
    private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Carrega o arquivo .ini a partir do caminho absoluto informado.
    /// </summary>
    public static IniConfig Carregar(string caminho)
    {
        if (!File.Exists(caminho))
            throw new FileNotFoundException($"Arquivo de configuracao nao encontrado: {caminho}", caminho);

        var ini = new IniConfig();
        var secaoAtual = string.Empty;

        foreach (var linhaBruta in File.ReadAllLines(caminho))
        {
            var linha = linhaBruta.Trim();
            if (linha.Length == 0) continue;
            if (linha.StartsWith('#') || linha.StartsWith(';')) continue;

            if (linha.StartsWith('[') && linha.EndsWith(']'))
            {
                secaoAtual = linha[1..^1].Trim();
                if (!ini._data.ContainsKey(secaoAtual))
                    ini._data[secaoAtual] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            var idx = linha.IndexOf('=');
            if (idx < 0) continue;

            var chave = linha[..idx].Trim();
            var valor = linha[(idx + 1)..].Trim();

            if (!ini._data.ContainsKey(secaoAtual))
                ini._data[secaoAtual] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            ini._data[secaoAtual][chave] = valor;
        }

        return ini;
    }

    /// <summary>
    /// Retorna valor da chave na secao, ou padrao se nao existir/vazio.
    /// </summary>
    public string GetString(string secao, string chave, string padrao = "")
    {
        if (_data.TryGetValue(secao, out var s) && s.TryGetValue(chave, out var v) && !string.IsNullOrWhiteSpace(v))
            return v;
        return padrao;
    }

    /// <summary>
    /// Retorna valor inteiro da chave, ou padrao se nao existir/invalido.
    /// </summary>
    public int GetInt(string secao, string chave, int padrao)
    {
        var raw = GetString(secao, chave);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : padrao;
    }

    /// <summary>
    /// Retorna valor booleano da chave (true/false/1/0/sim/nao), ou padrao se ausente.
    /// </summary>
    public bool GetBool(string secao, string chave, bool padrao)
    {
        var raw = GetString(secao, chave).ToLowerInvariant();
        return raw switch
        {
            "true" or "1" or "sim" or "yes" => true,
            "false" or "0" or "nao" or "não" or "no" => false,
            _ => padrao
        };
    }
}
