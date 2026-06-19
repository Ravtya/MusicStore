using MusicStore.Models;
using MusicStore.Services;
using MusicStore.Services.Interfaces;

namespace MusicStore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMusicStoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LocalizationSettings>(configuration.GetSection("Localization"));
        services.AddSingleton<IAudioGenerator, AudioGenerator>();
        services.AddSingleton<ITrackGenerator, TrackGenerator>();
        services.AddHttpClient<IImageGenerator, ImageGenerator>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MusicStore/1.0 (https://github.com/musicstore)");
        });
        return services;
    }
}
