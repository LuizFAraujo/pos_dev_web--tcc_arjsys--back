using System.Security.Cryptography;
using System.Text;
using SkiaSharp;

namespace Api_ArjSys_Tcc.Services.Shared.Thumbnail;

/// <summary>
/// Responsável APENAS pelo cache em disco das miniaturas.
/// Calcula a chave única, monta o caminho do arquivo, verifica se já existe
/// e grava de forma atômica. Não sabe renderizar nem codificar nada.
/// </summary>
public class ThumbnailCache(string diretorioCache)
{
    private readonly string _diretorioCache = diretorioCache;

    /// <summary>
    /// Calcula a chave de cache a partir do arquivo de origem e da largura.
    /// A chave inclui a data de modificação do arquivo: se o documento for
    /// substituído, a chave muda e a miniatura se regenera sozinha, sem
    /// precisar de invalidação manual.
    /// Devolve o hash (nome do arquivo no cache) e o ETag (mesmo hash entre
    /// aspas, no formato que o HTTP espera).
    /// </summary>
    public (string Hash, string ETag) CalcularChave(string caminhoArquivo, int largura)
    {
        var assinatura = $"{caminhoArquivo}|{File.GetLastWriteTimeUtc(caminhoArquivo):O}|w={largura}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(assinatura));
        var hash = Convert.ToHexString(bytes).ToLowerInvariant();
        return (hash, $"\"{hash}\"");
    }

    /// <summary>
    /// Caminho completo do arquivo de miniatura dentro do diretório de cache.
    /// </summary>
    public string CaminhoArquivo(string hash) => Path.Combine(_diretorioCache, $"{hash}.webp");

    /// <summary>
    /// Indica se a miniatura já existe no cache.
    /// </summary>
    public bool Existe(string hash) => File.Exists(CaminhoArquivo(hash));

    /// <summary>
    /// Grava a miniatura no cache de forma atômica: escreve primeiro num arquivo
    /// temporário (.tmp) e só então renomeia por cima do definitivo (.webp).
    /// Assim nunca se serve um arquivo pela metade caso duas requisições gerem
    /// a mesma miniatura ao mesmo tempo ou caso a escrita falhe no meio.
    /// </summary>
    public void Salvar(string hash, SKData dados)
    {
        Directory.CreateDirectory(_diretorioCache);

        var temporario = Path.Combine(_diretorioCache, $"{hash}.tmp");

        using (var arquivo = File.Create(temporario))
            dados.SaveTo(arquivo);

        File.Move(temporario, CaminhoArquivo(hash), overwrite: true);
    }

    /// <summary>
    /// Remove miniaturas do cache. Sem data, apaga tudo (inclui .tmp órfãos de
    /// escritas interrompidas). Com data, apaga só os arquivos gerados ANTES dela
    /// (os mais antigos), preservando os recentes. Devolve a quantidade removida.
    /// Arquivos em uso ou já removidos são ignorados, sem interromper a limpeza.
    /// </summary>
    public int Limpar(DateTime? anteriorA = null)
    {
        if (!Directory.Exists(_diretorioCache))
            return 0;

        var arquivos = Directory.EnumerateFiles(_diretorioCache, "*.webp")
            .Concat(Directory.EnumerateFiles(_diretorioCache, "*.tmp"));

        var removidos = 0;

        foreach (var arquivo in arquivos)
        {
            // Mantém os gerados na data de corte ou depois dela.
            if (anteriorA.HasValue && File.GetLastWriteTimeUtc(arquivo) >= anteriorA.Value)
                continue;

            try
            {
                File.Delete(arquivo);
                removidos++;
            }
            catch
            {
                // Arquivo em uso ou já removido: ignora e segue limpando o resto.
            }
        }

        return removidos;
    }
}
