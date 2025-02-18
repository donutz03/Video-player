using Microsoft.AspNetCore.Mvc;
using VideoPlayer_EasierCS.Models;

namespace VideoPlayer_EasierCS.Controllers;

public class VideoPlayerController : Controller
{
    private readonly List<Video> _videos;
    
    public VideoPlayerController()
    {
        _videos = new List<Video>
        {
            new Video 
            { 
                Id = 1, 
                Title = "Video 1", 
                FilePath = "/media/video1.mp4",
                SubtitlesPath = "/media/subtitles1.json",
                OrderIndex = 0
            },
        };
    }
    
    public IActionResult Index()
    {
        return View(_videos.OrderBy(v => v.OrderIndex).ToList());
    }
}