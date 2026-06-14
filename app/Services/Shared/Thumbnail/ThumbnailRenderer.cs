using PDFtoImage;
using SkiaSharp;

namespace Api_ArjSys_Tcc.Services.Shared.Thumbnail;

/// <summary>
/// Responsável APENAS por rasterizar o arquivo de origem em imagem (SKBitmap).
/// Decide o que fazer pela extensão do arquivo, num único ponto central.
/// Hoje suporta PDF. Imagens (jpg/png/etc) e outros formatos entram aqui,
/// numa branch a mais, sem mexer no resto do conjunto.
/// </summary>
public static class ThumbnailRenderer
{
    /// <summary>
    /// Rasteriza a primeira página/imagem do arquivo na largura pedida.
    /// Lança NotSupportedException se a extensão ainda não tiver suporte
    /// (o orquestrador captura isso e devolve erro tratado).
    /// </summary>
    public static SKBitmap Renderizar(string caminhoArquivo, int largura)
    {
        var extensao = Path.GetExtension(caminhoArquivo).TrimStart('.').ToLowerInvariant();

        return extensao switch
        {
            "pdf" => RenderizarPdf(caminhoArquivo, largura),
            // Futuro: "jpg" or "jpeg" or "png" or "webp" or "bmp" => RenderizarImagem(caminhoArquivo, largura),
            _ => throw new NotSupportedException($"Formato sem suporte para miniatura: .{extensao}")
        };
    }

    /// <summary>
    /// Renderiza a primeira página de um PDF como imagem, na largura pedida,
    /// mantendo a proporção e com fundo branco (desenhos técnicos costumam ser
    /// paisagem em fundo claro). Usa PDFtoImage (PDFium via SkiaSharp), que roda
    /// no Windows sem instalar nada no sistema nem exigir permissão de administrador.
    /// </summary>
    private static SKBitmap RenderizarPdf(string caminhoArquivo, int largura)
    {
        using var entrada = File.OpenRead(caminhoArquivo);

        return Conversion.ToImage(
            entrada,
            options: new RenderOptions(
                Width: largura,
                WithAspectRatio: true,
                BackgroundColor: SKColors.White));
    }
}
