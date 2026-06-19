namespace MusicStore.Services.Interfaces;

public interface IImageGenerator
{
    Task<byte[]> GenerateAsync(int localSeed, string albumName, string songName, string artist,
        CancellationToken ct = default);
}
