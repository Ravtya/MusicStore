using Bogus;
using MusicStore.Models;
using MusicStore.Services.Interfaces;

namespace MusicStore.Services;

public class TrackGenerator : ITrackGenerator
{
    private const int PageSize = 20;

    private static readonly Func<Faker, string>[] ArtistTemplates =
    [
        f => $"{f.Name.FirstName()} {f.Name.LastName()}",
        f => f.Name.FirstName(),
        f => f.Company.CompanyName(),
        f => $"{f.Name.FirstName()} & {f.Name.LastName()}",
        f => $"{f.PickRandom("MC", "DJ", "")} {f.Name.FirstName()}".Trim()
    ];

    private static readonly Func<Faker, string>[] AlbumTemplates =
    [
        f => $"{f.Commerce.Color()} {f.Hacker.Noun()}",
        f => f.Commerce.ProductName(),
        f => f.Company.CompanyName(),
        f => f.Name.JobArea(),
        _ => "Single",
        f => $"{f.Commerce.Department()} {f.Hacker.Noun()}"
    ];

    private static readonly Func<Faker, string>[] SongTemplates =
    [
        f => $"{f.Hacker.Verb()} {f.Hacker.Noun()}",
        f => $"{f.Hacker.Adjective()} {f.Music.Genre()}",
        f => $"{f.Name.FirstName()} {f.Random.Number(1, 99)}",
        f => $"{f.Commerce.Color()} {f.Hacker.Adjective()}",
        f => f.Address.StreetName().Split(' ').Last(),
        f => $"{f.Hacker.Noun()} {f.Hacker.Noun()}",
        f => $"{f.Commerce.ProductMaterial()} {f.Commerce.Color()}"
    ];

    public List<Track> GeneratePage(string locale, long seed, int page, decimal averageLikes)
    {
        var result = new List<Track>(PageSize);
        var startIndex = (page - 1L) * PageSize;

        for (var i = 0; i < PageSize; i++)
            result.Add(GenerateSingle(locale, seed, startIndex + i, averageLikes));

        return result;
    }

    private static Track GenerateSingle(string locale, long seed, long index, decimal averageLikes)
    {
        var localSeed = HashCode.Combine(seed, index);

        var track = new Faker<Track>(locale)
            .UseSeed(localSeed)
            .RuleFor(t => t.LocalSeed, localSeed)
            .RuleFor(t => t.Id, index + 1)
            .RuleFor(t => t.Song, f => PickTemplate(f, SongTemplates, capitalize: true))
            .RuleFor(t => t.Artist, f => PickTemplate(f, ArtistTemplates))
            .RuleFor(t => t.Album, f => PickTemplate(f, AlbumTemplates, capitalize: true))
            .RuleFor(t => t.Genre, f => f.Music.Genre())
            .RuleFor(t => t.Review, f => f.Commerce.ProductDescription())
            .RuleFor(t => t.DurationSeconds, f => f.Random.Int(0, 1) * 60 + f.Random.Int(0, 59))
            .RuleFor(t => t.Year, f => f.Random.Int(1960, 2026))
            .Generate();

        track.Likes = ComputeLikes(localSeed, averageLikes);

        return track;
    }

    private static int ComputeLikes(int localSeed, decimal averageLikes)
    {
        var wholePart = (int)averageLikes;
        var fractionalPart = averageLikes - wholePart;
        var random = new Random(localSeed);

        return random.NextDouble() < (double)fractionalPart ? wholePart + 1 : wholePart;
    }

    private static string PickTemplate(Faker f, Func<Faker, string>[] templates, bool capitalize = false)
    {
        var result = f.PickRandom(templates)(f);
        return capitalize ? CapitalizeFirst(result) : result;
    }

    private static string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var first = input.FirstOrDefault(char.IsLetter);
        if (first == 0) return input;

        var i = input.IndexOf(first);
        return string.Concat(input[..i], char.ToUpper(first), input[(i + 1)..]);
    }
}
