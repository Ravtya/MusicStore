namespace MusicStore.Models;

public class LocalizationSettings
{
    public string DefaultLocale { get; set; } = "en_US";
    public List<string> AvailableLocales { get; set; } = [];
}