namespace MusicStore.Models;

public class TrackListViewModel
{
    public TrackPageQuery Query { get; set; } = new();
    public List<Track> Tracks { get; set; } = [];
    public List<string> Locales { get; set; } = [];
}
