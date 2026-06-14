using SkiaSharp;

namespace Api_ArjSys_Tcc.Services.Shared.Thumbnail;

/// <summary>
/// Responsável APENAS por codificar a imagem (SKBitmap) em webp.
/// webp comprime melhor que png/jpg, deixando a miniatura bem leve para
/// carregar rápido no navegador e ocupar pouco disco no cache.
/// </summary>
public static class ThumbnailEncoder
{
    /// <summary>
    /// Converte o bitmap em dados webp. A qualidade vai de 0 a 100
    /// (padrão 80, bom equilíbrio entre nitidez e tamanho do arquivo).
    /// </summary>
    public static SKData ParaWebp(SKBitmap bitmap, int qualidade = 80)
    {
        using var imagem = SKImage.FromBitmap(bitmap);
        return imagem.Encode(SKEncodedImageFormat.Webp, qualidade);
    }
}
