using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicStore.Models;
using MusicStore.Services.Interfaces;

namespace MusicStore.Controllers;

public class HomeController(ITrackGenerator trackGenerator, IOptions<LocalizationSettings> localizationOptions)
    : Controller
{
    private readonly LocalizationSettings _settings = localizationOptions.Value;

    public IActionResult Index(TrackPageQuery query) =>
        View(new TrackListViewModel
        {
            Query = ResolveLocale(query),
            Tracks = GenerateTracks(query),
            Locales = _settings.AvailableLocales
        });

    public IActionResult LoadMore(TrackPageQuery query) =>
        PartialView("_TrackRows", GenerateTracks(query));

    public IActionResult Randomize(TrackPageQuery query)
    {
        query = ResolveLocale(query);
        query.Seed = Random.Shared.NextInt64(long.MinValue, long.MaxValue);
        query.Page = 1;
        return RedirectToAction(nameof(Index), query);
    }

    private List<Track> GenerateTracks(TrackPageQuery query)
    {
        var locale = ResolveLocale(query).Locale!;
        return trackGenerator.GeneratePage(locale, query.Seed, query.Page, query.AverageLikes);
    }

    private TrackPageQuery ResolveLocale(TrackPageQuery query)
    {
        query.Locale ??= _settings.DefaultLocale;
        return query;
    }
}
