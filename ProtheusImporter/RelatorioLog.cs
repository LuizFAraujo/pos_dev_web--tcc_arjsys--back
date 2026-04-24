using System.Text;

namespace ProtheusImporter.Core;

/// <summary>
/// Gerador de relatorio em arquivo texto.
///
/// Configuracao (.ini secao [Relatorio]):
///   GerarRelatorio   = liga/desliga
///   CaminhoRelatorio = pasta (vazio = pasta do exe)
///   NomeRelatorio    = nome do arquivo (default: ProtheusImporter.log)
///   Sobrescrever     = true apaga arquivo anterior; false faz append com divisor
///
/// Formato:
///
///   =========================================
///   SB1 - Produtos
///   =========================================
///    Data/hora:     ...
///    ...
///
///    RESUMO
///    ---
///    ...
///
///    REPETIDOS NO CSV (N)
///    ---
///    - CÓDIGO: 30.GRD.FR01.005.EC3T
///      LINHAS: 7922, 8013, 12030
///
///    NÃO ENCONTRADOS REFERENCIA EM SB1 (PRODUTOS) (N)
///    ---
///    ------ LINHA: 68142
///      CÓDIGO PAI: 10.ASA.MB02.019.0000 (ENCONTRADO)
///      COMPONENTE: 72.0.ZBSR.6M15.00000 (NÃO ENCONTRADO)
/// </summary>
public static class RelatorioLog
{
    /// <summary>
    /// Grava o relatorio da execucao atual. Se a flag no .ini estiver desligada, retorna null.
    /// </summary>
    public static string? Escrever(IniConfig config, string pastaExe, ImportResult resultado)
    {
        if (!config.GetBool("Relatorio", "GerarRelatorio", false))
            return null;

        var caminhoConfig = config.GetString("Relatorio", "CaminhoRelatorio");
        var nome = config.GetString("Relatorio", "NomeRelatorio", "ProtheusImporter.log");
        var sobrescrever = config.GetBool("Relatorio", "Sobrescrever", false);

        var pastaBase = string.IsNullOrWhiteSpace(caminhoConfig)
            ? pastaExe
            : (Path.IsPathRooted(caminhoConfig) ? caminhoConfig : Path.Combine(pastaExe, caminhoConfig));

        Directory.CreateDirectory(pastaBase);
        var caminhoArquivo = Path.Combine(pastaBase, nome);

        var conteudo = Montar(resultado);

        if (sobrescrever)
        {
            File.WriteAllText(caminhoArquivo, conteudo, Encoding.UTF8);
        }
        else
        {
            if (File.Exists(caminhoArquivo) && new FileInfo(caminhoArquivo).Length > 0)
            {
                File.AppendAllText(caminhoArquivo, Environment.NewLine + Environment.NewLine, Encoding.UTF8);
            }
            File.AppendAllText(caminhoArquivo, conteudo, Encoding.UTF8);
        }

        return caminhoArquivo;
    }

    /// <summary>
    /// Monta o texto do relatorio de uma execucao.
    /// </summary>
    private static string Montar(ImportResult r)
    {
        var sb = new StringBuilder();
        var linhaDupla = new string('=', 60);
        var meia = new string('-', 60);

        sb.AppendLine(linhaDupla);
        sb.AppendLine(r.TipoImportacao);
        sb.AppendLine(linhaDupla);
        sb.AppendLine($" Data/hora:     {r.QuandoIniciou:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($" Arquivo CSV:   {r.ArquivoCsv}");
        sb.AppendLine($" Modo:          {r.Modo}");
        sb.AppendLine($" Duracao total: {r.Duracao.TotalSeconds:F2}s");
        sb.AppendLine();

        sb.AppendLine(" RESUMO");
        sb.AppendLine(meia);
        sb.AppendLine($"   Linhas lidas no CSV..........: {r.LinhasLidas,8:N0}");
        sb.AppendLine($"   Inseridos....................: {r.Novos,8:N0}");
        sb.AppendLine($"   Atualizados..................: {r.Atualizados,8:N0}");
        sb.AppendLine($"   Repetidos no CSV.............: {r.RepetidosTotal,8:N0}");
        sb.AppendLine($"   Rejeitados...................: {r.Rejeitados,8:N0}");
        sb.AppendLine();

        if (r.PaisComRevisao > 0)
        {
            sb.AppendLine(" FILTRO DE REVISAO (Rev. Final)");
            sb.AppendLine(meia);
            sb.AppendLine(" Apenas a maior Rev. Final de cada codigo pai foi importada.");
            sb.AppendLine($"   Codigos-pai distintos........: {r.PaisComRevisao,8:N0}");
            sb.AppendLine($"   Pais com multiplas revisoes..: {r.PaisFiltrados,8:N0}");
            sb.AppendLine($"   Linhas descartadas (rev antiga): {r.LinhasDescartadasRevAntiga,6:N0}");
            sb.AppendLine();
        }

        if (r.Repetidos.Count > 0)
        {
            sb.AppendLine($" REPETIDOS NO CSV ({r.Repetidos.Count})");
            sb.AppendLine(meia);
            foreach (var rep in r.Repetidos)
            {
                if (string.IsNullOrEmpty(rep.Detalhe))
                {
                    sb.AppendLine($"- CÓDIGO: {rep.Chave}");
                }
                else
                {
                    sb.AppendLine($"- CÓDIGO: {rep.Chave} - {rep.Detalhe}");
                }
                sb.AppendLine($"  LINHAS: {string.Join(", ", rep.Linhas)}");
            }
            sb.AppendLine();
        }

        if (r.RejeicoesSG1.Count > 0)
        {
            sb.AppendLine($" NÃO ENCONTRADOS REFERENCIA EM SB1 (PRODUTOS) ({r.RejeicoesSG1.Count})");
            sb.AppendLine(meia);
            foreach (var rej in r.RejeicoesSG1)
            {
                sb.AppendLine($"------ LINHA: {rej.Linha}");
                sb.AppendLine($"  CÓDIGO PAI: {rej.CodigoPai} ({(rej.PaiEncontrado ? "ENCONTRADO" : "NÃO ENCONTRADO")})");
                sb.AppendLine($"  COMPONENTE: {rej.CodigoComponente} ({(rej.ComponenteEncontrado ? "ENCONTRADO" : "NÃO ENCONTRADO")})");
            }
            sb.AppendLine();
        }

        if (r.Mensagens.Count > 0)
        {
            sb.AppendLine($" OUTRAS DIVERGENCIAS ({r.Mensagens.Count})");
            sb.AppendLine(meia);
            foreach (var m in r.Mensagens) sb.AppendLine($"   - {m}");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
