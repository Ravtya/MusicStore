using MusicStore.Models;

namespace MusicStore.Services.Interfaces;

public interface ITrackGenerator
{
    List<Track> GeneratePage(string locale, long seed, int page, decimal averageLikes);
}
