using System.Text;
using Spectre.Console;
using ProtheusImporter.Core;
using ProtheusImporter.UI;

// Garante suporte a Windows-1252 em .NET 10 (nao vem por default no core).
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

Console.OutputEncoding = Encoding.UTF8;

try
{
    var pastaExe = AppContext.BaseDirectory;
    var configPath = Path.Combine(pastaExe, "importer-config.ini");

    if (!File.Exists(configPath))
    {
        AnsiConsole.MarkupLine($"[red]Arquivo de configuracao nao encontrado:[/] {Markup.Escape(configPath)}");
        AnsiConsole.MarkupLine("[grey]Copie o 'importer-config.ini' pra pasta do executavel.[/]");
        return 2;
    }

    var config = IniConfig.Carregar(configPath);
    var menu = new MenuPrincipal(config, pastaExe);
    menu.Executar();

    return 0;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}
