namespace Api_ArjSys_Tcc.Services.Shared.Thumbnail;

/// <summary>
/// Orquestrador da geração de miniaturas. É GENÉRICO: recebe um caminho de
/// arquivo e uma largura, não sabe nada de produto nem de setor. Por isso fica
/// em Services/Shared e pode ser usado por qualquer parte do sistema.
///
/// Coordena os três auxiliares de responsabilidade única:
///   - ThumbnailCache    (cache em disco)
///   - ThumbnailRenderer (rasteriza o arquivo em imagem)
///   - ThumbnailEncoder  (codifica a imagem em webp)
///
/// É a única classe deste conjunto registrada na Injeção de Dependência.
/// </summary>
public class ThumbnailService
{
    private readonly ThumbnailCache _cache;
    private readonly ILogger<ThumbnailService> _logger;

    public ThumbnailService(IConfiguration configuracao, ILogger<ThumbnailService> logger)
    {
        _logger = logger;

        // Diretório do cache configurável via appsettings (chave "Thumbnails:CacheDir").
        // Vazio  -> "thumbs/" ao lado da aplicação (padrão).
        // Relativo -> resolvido a partir da pasta da aplicação.
        // Absoluto -> usado como está (ex: uma pasta fixa que sobrevive a deploys).
        var configurado = configuracao["Thumbnails:CacheDir"];
        var diretorio = string.IsNullOrWhiteSpace(configurado)
            ? Path.Combine(AppContext.BaseDirectory, "thumbs")
            : Path.IsPathRooted(configurado)
                ? configurado
                : Path.Combine(AppContext.BaseDirectory, configurado);

        _cache = new ThumbnailCache(diretorio);
    }

    /// <summary>
    /// Gera (ou recupera do cache) a miniatura webp do arquivo informado.
    /// Devolve o caminho do arquivo de cache e o ETag em caso de sucesso;
    /// em caso de falha, Erro vem preenchido (e os demais nulos).
    /// </summary>
    public (string? CaminhoArquivo, string? ETag, string? Erro) Gerar(string caminhoArquivo, int largura)
    {
        if (!File.Exists(caminhoArquivo))
            return (null, null, "Arquivo de origem não encontrado");

        var (hash, etag) = _cache.CalcularChave(caminhoArquivo, largura);

        // Já existe no cache: serve direto, sem renderizar de novo.
        if (_cache.Existe(hash))
            return (_cache.CaminhoArquivo(hash), etag, null);

        try
        {
            using var bitmap = ThumbnailRenderer.Renderizar(caminhoArquivo, largura);
            using var dados = ThumbnailEncoder.ParaWebp(bitmap);
            _cache.Salvar(hash, dados);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Formato sem suporte para miniatura: {Arquivo}", caminhoArquivo);
            return (null, null, "Formato de arquivo sem suporte para miniatura");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao gerar miniatura de {Arquivo}", caminhoArquivo);
            return (null, null, "Falha ao gerar a miniatura");
        }

        return (_cache.CaminhoArquivo(hash), etag, null);
    }

    /// <summary>
    /// Limpa o cache de miniaturas.
    ///   - Sem parâmetros: apaga tudo.
    ///   - Com 'dias': apaga as geradas há mais de N dias.
    ///   - Com 'ate': apaga as geradas antes dessa data.
    /// Se ambos vierem, 'ate' tem precedência. Devolve a quantidade removida.
    /// </summary>
    public int LimparCache(int? dias = null, DateTime? ate = null)
    {
        DateTime? corte = ate ?? (dias.HasValue ? DateTime.UtcNow.AddDays(-dias.Value) : null);

        var removidas = _cache.Limpar(corte);

        _logger.LogInformation(
            "Cache de miniaturas limpo: {Quantidade} removidas (corte: {Corte})",
            removidas,
            corte?.ToString("o") ?? "tudo");

        return removidas;
    }
}
