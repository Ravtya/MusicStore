namespace MusicStore.Models;

public class Track
{
    public int LocalSeed { get; set; }
    public long Id { get; set; }
    public string Song { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;

    public int Likes { get; set; }
    public int DurationSeconds { get; set; }
    public string DurationDisplay => $"{DurationSeconds / 60}:{DurationSeconds % 60:D2}";
    public int Year { get; set; }
    public string Review { get; set; } = string.Empty;
}
