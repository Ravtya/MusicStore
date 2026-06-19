using System.Text.Json;
using MusicStore.Services.Interfaces;
using SkiaSharp;

namespace MusicStore.Services;

public class ImageGenerator(HttpClient httpClient, ILogger<ImageGenerator> logger) : IImageGenerator
{
    private const int CoverSize = 200;

    private static readonly string[] Queries =
    [
        "music", "concert", "vinyl", "guitar", "piano", "drums",
        "headphones", "microphone", "synthesizer", "record player",
        "festival", "stage lights", "album art", "cassette tape", "saxophone"
    ];

    public async Task<byte[]> GenerateAsync(int localSeed, string albumName, string songName, string artist,
        CancellationToken ct = default)
    {
        using var bitmap = await LoadBitmapAsync(localSeed, ct);

        using var canvas = new SKCanvas(bitmap);
        var title = albumName.Equals("Single", StringComparison.OrdinalIgnoreCase) ? songName : albumName;

        using (var shade = new SKPaint())
        {
            shade.Color = new SKColor(0, 0, 0, 160);
            canvas.DrawRect(0, CoverSize * 0.62f, CoverSize, CoverSize * 0.38f, shade);
        }

        DrawLabel(canvas, artist, CoverSize / 2f, CoverSize * 0.72f, CoverSize / 22f, CoverSize * 0.9f, false);
        DrawLabel(canvas, title, CoverSize / 2f, CoverSize * 0.84f, CoverSize / 16f, CoverSize * 0.92f, true);

        return EncodeJpeg(bitmap);
    }

    private async Task<SKBitmap> LoadBitmapAsync(int seed, CancellationToken ct)
    {
        try
        {
            var query = Queries[Math.Abs(seed) % Queries.Length];
            var searchUrl = $"https://api.openverse.org/v1/images/?q={Uri.EscapeDataString(query)}&page_size=20";
            var json = await httpClient.GetStringAsync(searchUrl, ct);
            using var doc = JsonDocument.Parse(json);
            var results = doc.RootElement.GetProperty("results");

            if (results.GetArrayLength() == 0)
                return CreateFallback(seed);

            var imageUrl = results[Math.Abs(seed) % results.GetArrayLength()].GetProperty("url").GetString();
            if (string.IsNullOrEmpty(imageUrl))
                return CreateFallback(seed);

            using var source = SKBitmap.Decode(await httpClient.GetByteArrayAsync(imageUrl, ct));
            if (source is null) return CreateFallback(seed);

            using var square = ToSquare(source);
            return Resize(square);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch cover image, using fallback");
            return CreateFallback(seed);
        }
    }

    private static SKBitmap CreateFallback(int seed)
    {
        var bmp = new SKBitmap(CoverSize, CoverSize);
        using var canvas = new SKCanvas(bmp);
        canvas.Clear(SKColor.FromHsl((uint)Math.Abs(seed) % 360, 40, 35));
        return bmp;
    }

    private static SKBitmap Resize(SKBitmap src)
    {
        var dst = new SKBitmap(CoverSize, CoverSize);
        using var canvas = new SKCanvas(dst);
        canvas.DrawBitmap(src, new SKRect(0, 0, src.Width, src.Height), new SKRect(0, 0, CoverSize, CoverSize));
        return dst;
    }

    private static SKBitmap ToSquare(SKBitmap src)
    {
        var side = Math.Min(src.Width, src.Height);
        var dst = new SKBitmap(side, side);

        using var canvas = new SKCanvas(dst);
        var srcRect = new SKRect((src.Width - side) / 2f, (src.Height - side) / 2f, (src.Width + side) / 2f,
            (src.Height + side) / 2f);
        canvas.DrawBitmap(src, srcRect, new SKRect(0, 0, side, side));

        return dst;
    }

    private static void DrawLabel(SKCanvas canvas, string text, float cx, float y, float fontSize, float maxWidth,
        bool bold)
    {
        using var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, fontSize);
        font.Embolden = bold;
        using var paint = new SKPaint();
        paint.IsAntialias = true;
        paint.Color = bold ? SKColors.White : new SKColor(220, 220, 220);

        if (font.MeasureText(text) > maxWidth)
            font.Size = fontSize * maxWidth / font.MeasureText(text);

        canvas.DrawText(text, cx - font.MeasureText(text) / 2f, y, font, paint);
    }

    private static byte[] EncodeJpeg(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }
}
