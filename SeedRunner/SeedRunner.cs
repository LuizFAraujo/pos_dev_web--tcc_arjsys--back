using Microsoft.Data.Sqlite;

namespace SeedRunner;

/// <summary>
/// Executor de SEEDs SQL para o banco SQLite do ARJSYS.
/// Projeto standalone, paralelo ao app/, fora do .slnx principal — uso apenas em dev.
/// Lê seed-config.ini (DbPath + SqlFolder) e seed-order.txt (lista de .sql) localizados
/// na mesma pasta do projeto. Suporta paths relativos (../docs/SQL) ou absolutos (C:\..., \\server\share).
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var pastaProjeto = LocalizarPastaProjeto();

            if (pastaProjeto == null)
            {
                Console.Error.WriteLine("[FATAL] Não foi possível localizar a pasta do SeedRunner (esperado: pasta contendo seed-config.ini).");
                return 2;
            }

            Console.WriteLine($"Pasta do SeedRunner: {pastaProjeto}");

            var configPath = Path.Combine(pastaProjeto, "seed-config.ini");
            var orderPath = Path.Combine(pastaProjeto, "seed-order.txt");

            if (!File.Exists(configPath))
            {
                Console.Error.WriteLine($"[ERRO] Arquivo de config não encontrado: {configPath}");
                return 2;
            }

            if (!File.Exists(orderPath))
            {
                Console.Error.WriteLine($"[ERRO] Arquivo de ordem não encontrado: {orderPath}");
                return 2;
            }

            var config = LerConfig(configPath);

            if (!config.TryGetValue("DbPath", out var dbPathConfig) || string.IsNullOrWhiteSpace(dbPathConfig))
            {
                Console.Error.WriteLine("[ERRO] Chave 'DbPath' não encontrada ou vazia em seed-config.ini");
                return 2;
            }

            if (!config.TryGetValue("SqlFolder", out var sqlFolderConfig) || string.IsNullOrWhiteSpace(sqlFolderConfig))
            {
                Console.Error.WriteLine("[ERRO] Chave 'SqlFolder' não encontrada ou vazia em seed-config.ini");
                return 2;
            }

            var dbPath = ResolverPath(dbPathConfig, pastaProjeto);
            var sqlFolder = ResolverPath(sqlFolderConfig, pastaProjeto);

            Console.WriteLine($"Banco:      {dbPath}");
            Console.WriteLine($"Pasta SQL:  {sqlFolder}");

            if (!File.Exists(dbPath))
            {
                Console.Error.WriteLine($"[ERRO] Banco de dados não encontrado: {dbPath}");
                return 2;
            }

            if (!Directory.Exists(sqlFolder))
            {
                Console.Error.WriteLine($"[ERRO] Pasta de SQLs não encontrada: {sqlFolder}");
                return 2;
            }

            var arquivos = LerOrdem(orderPath);

            if (arquivos.Count == 0)
            {
                Console.WriteLine("Nenhum arquivo listado em seed-order.txt — nada a fazer.");
                return 0;
            }

            Console.WriteLine($"Arquivos na fila: {arquivos.Count}");
            Console.WriteLine();

            var connString = $"Data Source={dbPath}";
            using var conn = new SqliteConnection(connString);
            conn.Open();

            // Habilita enforcement de FK no SQLite (por padrão vem desligado)
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();
            }

            var totalInicio = DateTime.UtcNow;
            var totalStatements = 0;

            foreach (var arquivo in arquivos)
            {
                var caminhoSql = Path.Combine(sqlFolder, arquivo);

                if (!File.Exists(caminhoSql))
                {
                    Console.Error.WriteLine($"  [ERRO] Arquivo não encontrado: {caminhoSql}");
                    return 3;
                }

                Console.WriteLine($"[RUN] {arquivo}");

                var inicio = DateTime.UtcNow;
                var sql = File.ReadAllText(caminhoSql);
                var statements = SplitStatements(sql);

                using var transacao = conn.BeginTransaction();
                var execStatements = 0;

                try
                {
                    foreach (var stmt in statements)
                    {
                        if (string.IsNullOrWhiteSpace(stmt)) continue;

                        using var cmd = conn.CreateCommand();
                        cmd.Transaction = transacao;
                        cmd.CommandText = stmt;
                        cmd.ExecuteNonQuery();
                        execStatements++;
                    }

                    transacao.Commit();
                }
                catch (Exception ex)
                {
                    transacao.Rollback();
                    Console.Error.WriteLine($"  [ERRO] Falha ao executar {arquivo}: {ex.Message}");
                    Console.Error.WriteLine($"         (transação revertida — nenhum dado do arquivo foi gravado)");
                    return 4;
                }

                totalStatements += execStatements;
                var duracao = (DateTime.UtcNow - inicio).TotalMilliseconds;
                Console.WriteLine($"       OK — {execStatements} statement(s) em {duracao:F0} ms");
            }

            Console.WriteLine();
            var duracaoTotal = (DateTime.UtcNow - totalInicio).TotalSeconds;
            Console.WriteLine($"Concluído: {arquivos.Count} arquivo(s), {totalStatements} statement(s) em {duracaoTotal:F2} s");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex}");
            return 1;
        }
    }

    /// <summary>
    /// Localiza a pasta do projeto SeedRunner (a que contém seed-config.ini).
    /// Sobe a árvore a partir de AppContext.BaseDirectory até achar o arquivo.
    /// Necessário porque, quando rodado via 'dotnet run', BaseDirectory aponta pra bin/Debug/netX.0/.
    /// </summary>
    private static string? LocalizarPastaProjeto()
    {
        var atual = new DirectoryInfo(AppContext.BaseDirectory);

        while (atual != null)
        {
            if (File.Exists(Path.Combine(atual.FullName, "seed-config.ini")))
                return atual.FullName;

            atual = atual.Parent;
        }

        return null;
    }

    /// <summary>
    /// Resolve um path do arquivo de config.
    /// Se o path for absoluto (C:\..., /home/..., \\server\share\...), usa direto.
    /// Se for relativo, resolve a partir da pasta do projeto.
    /// </summary>
    private static string ResolverPath(string pathConfig, string pastaBase)
    {
        var path = pathConfig.Trim();

        // Expande variáveis de ambiente tipo %USERPROFILE%
        path = Environment.ExpandEnvironmentVariables(path);

        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(pastaBase, path));
    }

    /// <summary>
    /// Lê arquivo .ini simples no formato chave=valor.
    /// Ignora linhas em branco, comentários (#, ;) e cabeçalhos de seção ([...]).
    /// </summary>
    private static Dictionary<string, string> LerConfig(string path)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var linhaRaw in File.ReadAllLines(path))
        {
            var linha = linhaRaw.Trim();

            if (string.IsNullOrEmpty(linha)) continue;
            if (linha.StartsWith('#') || linha.StartsWith(';')) continue;
            if (linha.StartsWith('[')) continue;

            var idx = linha.IndexOf('=');
            if (idx <= 0) continue;

            var chave = linha[..idx].Trim();
            var valor = linha[(idx + 1)..].Trim();

            dict[chave] = valor;
        }

        return dict;
    }

    /// <summary>
    /// Lê lista de arquivos .sql a executar, um por linha.
    /// Ignora linhas vazias e comentários (# no início).
    /// </summary>
    private static List<string> LerOrdem(string path)
    {
        var lista = new List<string>();

        foreach (var linhaRaw in File.ReadAllLines(path))
        {
            var linha = linhaRaw.Trim();

            if (string.IsNullOrEmpty(linha)) continue;
            if (linha.StartsWith('#')) continue;

            lista.Add(linha);
        }

        return lista;
    }

    /// <summary>
    /// Divide um script SQL em statements individuais usando ; como separador.
    /// Respeita aspas simples, aspas duplas, comentários de linha (--) e comentários de bloco (/* */).
    /// </summary>
    private static List<string> SplitStatements(string sql)
    {
        var statements = new List<string>();
        var atual = new System.Text.StringBuilder();

        var emAspasSimples = false;
        var emAspasDuplas = false;
        var emComentarioLinha = false;
        var emComentarioBloco = false;

        for (int i = 0; i < sql.Length; i++)
        {
            var c = sql[i];
            var prox = i + 1 < sql.Length ? sql[i + 1] : '\0';

            if (emComentarioLinha)
            {
                atual.Append(c);
                if (c == '\n') emComentarioLinha = false;
                continue;
            }

            if (emComentarioBloco)
            {
                atual.Append(c);
                if (c == '*' && prox == '/')
                {
                    atual.Append(prox);
                    i++;
                    emComentarioBloco = false;
                }
                continue;
            }

            if (!emAspasSimples && !emAspasDuplas)
            {
                if (c == '-' && prox == '-')
                {
                    atual.Append(c);
                    emComentarioLinha = true;
                    continue;
                }

                if (c == '/' && prox == '*')
                {
                    atual.Append(c);
                    emComentarioBloco = true;
                    continue;
                }
            }

            if (c == '\'' && !emAspasDuplas)
            {
                // Duplicação '' dentro de string — mantém o estado
                if (emAspasSimples && prox == '\'')
                {
                    atual.Append(c);
                    atual.Append(prox);
                    i++;
                    continue;
                }

                emAspasSimples = !emAspasSimples;
                atual.Append(c);
                continue;
            }

            if (c == '"' && !emAspasSimples)
            {
                emAspasDuplas = !emAspasDuplas;
                atual.Append(c);
                continue;
            }

            if (c == ';' && !emAspasSimples && !emAspasDuplas)
            {
                var stmt = atual.ToString().Trim();
                if (!string.IsNullOrEmpty(stmt))
                    statements.Add(stmt);
                atual.Clear();
                continue;
            }

            atual.Append(c);
        }

        var final = atual.ToString().Trim();
        if (!string.IsNullOrEmpty(final))
            statements.Add(final);

        return statements;
    }
}
