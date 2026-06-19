namespace MusicStore.Services.Interfaces;

public interface IAudioGenerator
{
    byte[] Generate(int localSeed, int durationSeconds);
}
