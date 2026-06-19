namespace MusicStore.Models;

public class TrackPageQuery
{
    public string? Locale { get; set; }
    public long Seed { get; set; }
    public int Page { get; set; } = 1;
    public decimal AverageLikes { get; set; }
    public bool InfiniteScroll { get; set; }
}
