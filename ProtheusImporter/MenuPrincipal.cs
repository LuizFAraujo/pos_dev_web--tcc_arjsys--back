using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using Api_ArjSys_Tcc.Data;
using Api_ArjSys_Tcc.Models.Engenharia;
using ProtheusImporter.Core;
using ProtheusImporter.Importers;

namespace ProtheusImporter.UI;

/// <summary>
/// Menu interativo principal (Spectre.Console).
/// Orquestra os fluxos SB1 e SG1: prompts, analise, preview, confirmacao, execucao.
/// </summary>
public sealed class MenuPrincipal
{
    private readonly IniConfig _config;
    private readonly string _pastaExe;
    private readonly string _dbPath;

    public MenuPrincipal(IniConfig config, string pastaExe)
    {
        _config = config;
        _pastaExe = pastaExe;

        var dbConfig = config.GetString("Banco", "DbPath");
        _dbPath = Path.IsPathRooted(dbConfig)
            ? dbConfig
            : Path.GetFullPath(Path.Combine(pastaExe, string.IsNullOrEmpty(dbConfig) ? @"..\app\Database\ArjSysDB.db" : dbConfig));
    }

    public void Executar()
    {
        RenderizarCabecalho();

        while (true)
        {
            var menu = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .HideHeaders()
                .AddColumn(new TableColumn(""));

            menu.AddRow("[cyan]1[/].  Importar SB1 [grey](Produtos)[/]");
            menu.AddRow("[cyan]2[/].  Importar SG1 [grey](Estruturas / BOM)[/]");
            menu.AddRow("[cyan]3[/].  Ver configuracao atual");
            menu.AddRow("[cyan]4[/].  Sair");

            AnsiConsole.Write(new Panel(menu)
                .Header("[bold cyan] O que deseja fazer? [/]")
                .BorderColor(Color.Cyan1)
                .Padding(1, 0));

            var opcao = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Escolha[/] [[[cyan]1-4[/]]]:")
                    .PromptStyle("green")
                    .Validate(v =>
                    {
                        var t = v.Trim();
                        return t is "1" or "2" or "3" or "4"
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Opcao invalida. Use 1, 2, 3 ou 4.[/]");
                    }))
                .Trim();

            AnsiConsole.WriteLine();

            try
            {
                switch (opcao)
                {
                    case "1": FluxoSB1(); break;
                    case "2": FluxoSG1(); break;
                    case "3": MostrarConfig(); break;
                    case "4": return;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.Write(new Panel(new Markup($"[red]{Markup.Escape(ex.Message)}[/]"))
                    .Header("[red]Erro[/]")
                    .BorderColor(Color.Red));
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Pressione qualquer tecla pra voltar ao menu...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
            RenderizarCabecalho();
        }
    }

    private void RenderizarCabecalho()
    {
        AnsiConsole.Write(new FigletText("ProtheusImporter").Color(Color.Cyan1));
        AnsiConsole.MarkupLine($"[grey]Banco:[/] {Markup.Escape(_dbPath)}");
        AnsiConsole.MarkupLine($"[grey]Pasta exe:[/] {Markup.Escape(_pastaExe)}");
        AnsiConsole.WriteLine();
    }

    private void MostrarConfig()
    {
        var tbl = new Table().BorderColor(Color.Grey);
        tbl.AddColumn("Secao").AddColumn("Chave").AddColumn("Valor");

        void Add(string s, string k) =>
            tbl.AddRow(s, k, Markup.Escape(_config.GetString(s, k, "(vazio)")));

        Add("Banco", "DbPath");
        Add("CSV", "Separador");
        Add("CSV", "ArquivoSB1");
        Add("CSV", "ArquivoSG1");
        Add("CSV", "PastaCSV");
        Add("CSV", "MaxLinhasBuscaHeader");
        Add("Importacao", "BatchSize");
        Add("Importacao", "SomarSequenciasRepetidas");
        Add("Confirmacoes", "ConfirmarAntesDeImportar");
        Add("Confirmacoes", "ConfirmarReset");
        Add("Confirmacoes", "ConfirmarUpsert");
        Add("Perigo", "PermitirResetCascata");
        Add("Relatorio", "GerarRelatorio");
        Add("Relatorio", "CaminhoRelatorio");
        Add("Relatorio", "NomeRelatorio");
        Add("Relatorio", "Sobrescrever");

        AnsiConsole.Write(tbl);
    }

    // ========================================================================
    // Fluxo SB1 (Produtos)
    // ========================================================================

    private void FluxoSB1()
    {
        var csvPath = ResolverCsvPath("ArquivoSB1", "SB1");
        if (csvPath is null) return;

        var modo = PromptModo();
        var opts = MontarOptions(csvPath, modo,
            obrigatorias: new[] { ImportadorSB1.COL_CODIGO, ImportadorSB1.COL_DESCRICAO },
            opcionais: new[] { ImportadorSB1.COL_ATIVO, ImportadorSB1.COL_UNIDADE, ImportadorSB1.COL_TIPO });

        using var db = CriarContext();

        // Reset em cascata (so quando modo = Reset + flag PermitirResetCascata = true).
        // Executado ANTES da analise/importacao normal pra zerar as dependencias.
        if (modo == ModoImportacao.ResetarEInserir && opts.PermitirResetCascata)
        {
            if (!FluxoResetCascataSB1(db)) return;
        }

        var importer = ImportadorSB1.Criar(db, opts);

        var quandoIniciou = DateTime.Now;

        AnsiConsole.MarkupLine("[yellow]Analisando CSV...[/]");
        var preview = importer.Analisar();

        MostrarPreview(preview, modo);

        if (!ConfirmarExecucao(modo)) return;

        AnsiConsole.MarkupLine("[yellow]Importando...[/]");
        ImportResult resultado = null!;
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .Start(ctx =>
            {
                var total = preview.TotalAGravar;
                var task = ctx.AddTask("[green]Gravando produtos[/]", maxValue: Math.Max(1, total));

                resultado = importer.Aplicar(preview, onProgress: (feitos, totalReal) =>
                {
                    task.MaxValue = Math.Max(1, totalReal);
                    task.Value = feitos;
                });

                task.Value = task.MaxValue;
            });

        resultado.TipoImportacao = "SB1 - Produtos";
        resultado.ArquivoCsv = csvPath;
        resultado.Modo = modo == ModoImportacao.ResetarEInserir ? "Resetar" : "Atualizar (UPSERT)";
        resultado.QuandoIniciou = quandoIniciou;

        MostrarResultadoFinal(resultado);
        GravarRelatorio(resultado);
    }

    /// <summary>
    /// Fluxo do reset em cascata: exibe tabelas afetadas, pede confirmacao
    /// digitando uma frase exata, e executa o DELETE em cascata.
    /// Retorna true se executou (pode seguir pra importacao), false se cancelou.
    /// </summary>
    private bool FluxoResetCascataSB1(AppDbContext db)
    {
        var contagem = ImportadorSB1.ContarDependentes(db);

        var tbl = new Table().BorderColor(Color.Red);
        tbl.AddColumn("Tabela").AddColumn(new TableColumn("Registros").RightAligned());

        long total = 0;
        foreach (var (nome, qtd) in contagem)
        {
            tbl.AddRow(Markup.Escape(nome), qtd.ToString("N0"));
            total += qtd;
        }
        tbl.AddEmptyRow();
        tbl.AddRow("[bold]TOTAL[/]", $"[bold]{total:N0}[/]");

        AnsiConsole.Write(new Panel(tbl)
            .Header("[red bold]RESET EM CASCATA — registros que serao APAGADOS[/]")
            .BorderColor(Color.Red));

        AnsiConsole.MarkupLine("[red bold]ATENCAO:[/] [red]esta acao e IRREVERSIVEL.[/]");
        AnsiConsole.MarkupLine("[red]Todas as tabelas acima serao esvaziadas antes da importacao.[/]");
        AnsiConsole.MarkupLine("[red]Use apenas em ambiente de teste ou implantacao inicial.[/]");
        AnsiConsole.WriteLine();

        const string fraseConfirmacao = "SIM";

        var digitado = AnsiConsole.Prompt(
            new TextPrompt<string>($"Pra confirmar, digite [cyan]{fraseConfirmacao}[/] em maiusculo (ou qualquer outra coisa pra cancelar):")
                .AllowEmpty());

        if (!string.Equals(digitado.Trim(), fraseConfirmacao, StringComparison.Ordinal))
        {
            AnsiConsole.MarkupLine("[yellow]Cancelado. Nada foi apagado.[/]");
            return false;
        }

        AnsiConsole.MarkupLine("[red]Executando reset em cascata...[/]");
        ImportadorSB1.ExecutarResetCascata(db);
        AnsiConsole.MarkupLine($"[green]Reset concluido. {total:N0} registros removidos no total.[/]");
        AnsiConsole.WriteLine();

        return true;
    }

    // ========================================================================
    // Fluxo SG1 (Estruturas / BOM)
    // ========================================================================

    private void FluxoSG1()
    {
        var csvPath = ResolverCsvPath("ArquivoSG1", "SG1");
        if (csvPath is null) return;

        var modo = PromptModo();
        var opts = MontarOptions(csvPath, modo,
            obrigatorias: new[]
            {
                ImportadorSG1.COL_CODIGO_PAI,
                ImportadorSG1.COL_ORDEM_ITEM,
                ImportadorSG1.COL_SEQUENCIA,
                ImportadorSG1.COL_QUANTIDADE,
                ImportadorSG1.COL_COMPONENTE,
                ImportadorSG1.COL_REV_FINAL
            },
            opcionais: Array.Empty<string>());

        using var db = CriarContext();

        var quandoIniciou = DateTime.Now;

        AnsiConsole.MarkupLine("[yellow]Analisando CSV e cruzando com Produtos...[/]");
        var preview = ImportadorSG1.Analisar(db, opts);

        MostrarPreview(preview, modo);

        if (!ConfirmarExecucao(modo)) return;

        AnsiConsole.MarkupLine("[yellow]Importando...[/]");
        ImportResult resultado = null!;
        AnsiConsole.Progress()
            .AutoClear(false)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .Start(ctx =>
            {
                var total = preview.TotalAGravar;
                var task = ctx.AddTask("[green]Gravando estruturas[/]", maxValue: Math.Max(1, total));

                resultado = ImportadorSG1.Aplicar(db, opts, preview, onProgress: (feitos, totalReal) =>
                {
                    task.MaxValue = Math.Max(1, totalReal);
                    task.Value = feitos;
                });

                task.Value = task.MaxValue;
            });

        resultado.TipoImportacao = "SG1 - Estruturas / BOM";
        resultado.ArquivoCsv = csvPath;
        resultado.Modo = modo == ModoImportacao.ResetarEInserir ? "Resetar" : "Atualizar (UPSERT)";
        resultado.QuandoIniciou = quandoIniciou;

        MostrarResultadoFinal(resultado);
        GravarRelatorio(resultado);
    }

    /// <summary>
    /// Grava o arquivo de log de acordo com as configs [Relatorio] do .ini.
    /// Se desligado, nao faz nada. Em caso de erro de IO, exibe sem abortar.
    /// </summary>
    private void GravarRelatorio(ImportResult r)
    {
        try
        {
            var caminho = RelatorioLog.Escrever(_config, _pastaExe, r);
            if (caminho is not null)
            {
                AnsiConsole.MarkupLine($"[grey]Relatorio gravado em:[/] {Markup.Escape(caminho)}");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Falha ao gravar relatorio:[/] {Markup.Escape(ex.Message)}");
        }
    }

    // ========================================================================
    // Helpers compartilhados
    // ========================================================================

    private AppDbContext CriarContext()
    {
        var optsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optsBuilder.UseSqlite($"Data Source={_dbPath}");
        return new AppDbContext(optsBuilder.Options);
    }

    /// <summary>
    /// Resolve o caminho do CSV: primeiro tenta config (PastaCSV + ArquivoXXX);
    /// se nao achar, pede pro usuario digitar.
    /// </summary>
    private string? ResolverCsvPath(string chaveArquivo, string rotulo)
    {
        var pastaCsv = _config.GetString("CSV", "PastaCSV");
        var nomeArquivo = _config.GetString("CSV", chaveArquivo);

        var basePath = string.IsNullOrWhiteSpace(pastaCsv)
            ? _pastaExe
            : (Path.IsPathRooted(pastaCsv) ? pastaCsv : Path.Combine(_pastaExe, pastaCsv));

        var candidato = !string.IsNullOrWhiteSpace(nomeArquivo)
            ? Path.Combine(basePath, nomeArquivo)
            : string.Empty;

        if (!string.IsNullOrEmpty(candidato) && File.Exists(candidato))
        {
            AnsiConsole.MarkupLine($"[grey]CSV encontrado no config:[/] {Markup.Escape(candidato)}");
            return candidato;
        }

        AnsiConsole.MarkupLine($"[yellow]CSV {rotulo} nao encontrado no config.[/]");
        var informado = AnsiConsole.Ask<string>($"Caminho completo do CSV {rotulo} (ou 'c' pra cancelar):");
        if (string.Equals(informado.Trim(), "c", StringComparison.OrdinalIgnoreCase)) return null;

        if (!File.Exists(informado))
        {
            AnsiConsole.MarkupLine($"[red]Arquivo nao encontrado: {Markup.Escape(informado)}[/]");
            return null;
        }

        return informado;
    }

    private ModoImportacao PromptModo()
    {
        var tbl = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .HideHeaders()
            .AddColumn(new TableColumn(""));

        tbl.AddRow("[cyan]1[/].  Atualizar [grey](UPSERT — novos inseridos, existentes atualizados)[/]");
        tbl.AddRow("[cyan]2[/].  Resetar [grey](apaga tudo antes e insere)[/]");

        AnsiConsole.Write(new Panel(tbl)
            .Header("[bold yellow] Modo de importacao [/]")
            .BorderColor(Color.Yellow)
            .Padding(1, 0));

        var escolha = AnsiConsole.Prompt(
            new TextPrompt<string>("[bold]Escolha[/] [[[cyan]1-2[/]]]:")
                .PromptStyle("green")
                .Validate(v =>
                {
                    var t = v.Trim();
                    return t is "1" or "2"
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Opcao invalida. Use 1 ou 2.[/]");
                }))
            .Trim();

        return escolha == "2" ? ModoImportacao.ResetarEInserir : ModoImportacao.AtualizarIncremental;
    }

    private ImportOptions MontarOptions(string csvPath, ModoImportacao modo,
        IReadOnlyList<string> obrigatorias, IReadOnlyList<string> opcionais)
    {
        var sep = _config.GetString("CSV", "Separador", ";");
        var separador = sep.Length > 0 ? sep[0] : ';';

        return new ImportOptions
        {
            CsvPath = csvPath,
            Separador = separador,
            Encoding = System.Text.Encoding.GetEncoding(1252),
            ColunasObrigatorias = obrigatorias,
            ColunasOpcionais = opcionais,
            MaxLinhasBuscaHeader = _config.GetInt("CSV", "MaxLinhasBuscaHeader", 20),
            BatchSize = _config.GetInt("Importacao", "BatchSize", 500),
            Modo = modo,
            PermitirResetCascata = _config.GetBool("Perigo", "PermitirResetCascata", false),
            SomarSequenciasRepetidas = _config.GetBool("Importacao", "SomarSequenciasRepetidas", true)
        };
    }

    private void MostrarPreview<TEntity, TKey>(
        PreviewResult<TEntity, TKey> preview, ModoImportacao modo)
        where TEntity : class
        where TKey : notnull
    {
        var tbl = new Table().BorderColor(Color.Grey);
        tbl.AddColumn("Metrica").AddColumn("Valor");
        tbl.AddRow("Linhas lidas no CSV", preview.LinhasLidas.ToString("N0"));
        tbl.AddRow("Registros ja existentes no banco", preview.TotalExistentes.ToString("N0"));
        tbl.AddRow("[green]Novos (serao inseridos)[/]", preview.InsercoesPrevistas.Count.ToString("N0"));
        tbl.AddRow("[yellow]Atualizacoes (serao modificadas)[/]", preview.AtualizacoesPrevistas.Count.ToString("N0"));
        tbl.AddRow("Repetidos no CSV", preview.RepetidosTotal.ToString("N0"));
        if (preview.PaisComRevisao > 0)
        {
            tbl.AddRow("Codigos-pai (total apos filtro de rev.)", preview.PaisComRevisao.ToString("N0"));
            tbl.AddRow("Pais com multiplas revisoes", preview.PaisFiltrados.ToString("N0"));
            tbl.AddRow("Linhas descartadas (rev. antiga)", preview.LinhasDescartadasRevAntiga.ToString("N0"));
        }
        tbl.AddRow("[red]Rejeitadas[/]", preview.Rejeitados.ToString("N0"));
        tbl.AddRow("Tempo de analise", $"{preview.Duracao.TotalSeconds:F2}s");

        AnsiConsole.Write(new Panel(tbl).Header("[bold]Previa da importacao[/]").BorderColor(Color.Cyan1));

        if (modo == ModoImportacao.ResetarEInserir)
        {
            AnsiConsole.MarkupLine("[red bold]ATENCAO:[/] [red]modo RESET — todos os registros atuais da tabela serao apagados.[/]");
        }

        // Amostra de repetidos (ate 5).
        if (preview.Repetidos.Count > 0)
        {
            var amostra = preview.Repetidos.Take(5).ToList();
            AnsiConsole.MarkupLine($"[yellow]Primeiros {amostra.Count} repetidos:[/]");
            foreach (var r in amostra)
            {
                var txt = string.IsNullOrEmpty(r.Detalhe)
                    ? $"- CÓDIGO: {r.Chave} (linhas: {string.Join(", ", r.Linhas.Take(6))}{(r.Linhas.Count > 6 ? "..." : "")})"
                    : $"- CÓDIGO: {r.Chave} - {r.Detalhe} (linhas: {string.Join(", ", r.Linhas.Take(6))}{(r.Linhas.Count > 6 ? "..." : "")})";
                AnsiConsole.MarkupLine($"  [grey]{Markup.Escape(txt)}[/]");
            }
            if (preview.Repetidos.Count > 5)
                AnsiConsole.MarkupLine($"  [grey]... (+{preview.Repetidos.Count - 5} nao exibidos. Ver relatorio completo.)[/]");
        }

        // Amostra de rejeicoes SG1 (ate 5).
        if (preview.RejeicoesSG1.Count > 0)
        {
            var amostra = preview.RejeicoesSG1.Take(5).ToList();
            AnsiConsole.MarkupLine($"[red]Primeiras {amostra.Count} rejeicoes (nao encontrados em Produtos):[/]");
            foreach (var r in amostra)
            {
                var pai = r.PaiEncontrado ? "OK" : "[red]FALTA[/]";
                var comp = r.ComponenteEncontrado ? "OK" : "[red]FALTA[/]";
                AnsiConsole.MarkupLine($"  [grey]Linha {r.Linha}: pai={Markup.Escape(r.CodigoPai)} ({pai}), comp={Markup.Escape(r.CodigoComponente)} ({comp})[/]");
            }
            if (preview.RejeicoesSG1.Count > 5)
                AnsiConsole.MarkupLine($"  [grey]... (+{preview.RejeicoesSG1.Count - 5} nao exibidas. Ver relatorio completo.)[/]");
        }

        // Outras mensagens (usado no SB1 e casos gerais).
        if (preview.Mensagens.Count > 0)
        {
            var amostra = preview.Mensagens.Take(5).ToList();
            AnsiConsole.MarkupLine($"[yellow]Primeiras {amostra.Count} outras divergencias:[/]");
            foreach (var m in amostra) AnsiConsole.MarkupLine($"  [grey]- {Markup.Escape(m)}[/]");
            if (preview.Mensagens.Count > 5)
                AnsiConsole.MarkupLine($"  [grey]... (+{preview.Mensagens.Count - 5} nao exibidas. Ver relatorio completo.)[/]");
        }
    }

    private bool ConfirmarExecucao(ModoImportacao modo)
    {
        var chave = modo == ModoImportacao.ResetarEInserir ? "ConfirmarReset" : "ConfirmarUpsert";
        var pedirConfirmacao = _config.GetBool("Confirmacoes", chave, true)
                            && _config.GetBool("Confirmacoes", "ConfirmarAntesDeImportar", true);

        if (!pedirConfirmacao)
        {
            AnsiConsole.MarkupLine("[grey]Confirmacao desativada no .ini — seguindo direto.[/]");
            return true;
        }

        return AnsiConsole.Confirm("Deseja prosseguir com a importacao?", defaultValue: false);
    }

    private void MostrarResultadoFinal(ImportResult r)
    {
        var tbl = new Table().BorderColor(Color.Green);
        tbl.AddColumn("Metrica").AddColumn("Valor");
        tbl.AddRow("Linhas lidas", r.LinhasLidas.ToString("N0"));
        tbl.AddRow("[green]Inseridos[/]", r.Novos.ToString("N0"));
        tbl.AddRow("[yellow]Atualizados[/]", r.Atualizados.ToString("N0"));
        tbl.AddRow("Repetidos no CSV", r.RepetidosTotal.ToString("N0"));
        if (r.PaisComRevisao > 0)
        {
            tbl.AddRow("Codigos-pai (total apos filtro de rev.)", r.PaisComRevisao.ToString("N0"));
            tbl.AddRow("Pais com multiplas revisoes", r.PaisFiltrados.ToString("N0"));
            tbl.AddRow("Linhas descartadas (rev. antiga)", r.LinhasDescartadasRevAntiga.ToString("N0"));
        }
        tbl.AddRow("[red]Rejeitados[/]", r.Rejeitados.ToString("N0"));
        tbl.AddRow("Tempo total", $"{r.Duracao.TotalSeconds:F2}s");

        AnsiConsole.Write(new Panel(tbl).Header("[bold green]Importacao concluida[/]").BorderColor(Color.Green));
    }
}
