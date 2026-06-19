using Microsoft.AspNetCore.Mvc;
using MusicStore.Services.Interfaces;

namespace MusicStore.Controllers;

public class MediaController(IAudioGenerator audioGenerator, IImageGenerator imageGenerator) : Controller
{
    [HttpGet("/media/audio")]
    public IActionResult Audio(int localSeed, int durationSeconds)
    {
        return File(audioGenerator.Generate(localSeed, durationSeconds), "audio/wav", true);
    }

    [HttpGet("/media/cover")]
    public async Task<IActionResult> Cover(int localSeed, string album, string song, string artist)
    {
        var bytes = await imageGenerator.GenerateAsync(localSeed, album, song, artist);
        return File(bytes, "image/jpeg", true);
    }
}
