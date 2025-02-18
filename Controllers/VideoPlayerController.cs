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
            new Video 
            { 
                Id = 2, 
                Title = "Video 2", 
                FilePath = "/media/video2.mp4",
                SubtitlesPath = "/media/subtitles2.json",
                OrderIndex = 1
            },
            new Video 
            { 
                Id = 3, 
                Title = "Video 3", 
                FilePath = "/media/video3.mp4",
                SubtitlesPath = "/media/subtitles3.json",
                OrderIndex = 2
            },
            new Video 
            { 
                Id = 4, 
                Title = "Video 4", 
                FilePath = "/media/video4.mp4",
                SubtitlesPath = "/media/subtitles4.json",
                OrderIndex = 3
            }
        };
    }
    
    public IActionResult Index(int? currentVideoId = null)
    {
        var orderedVideos = _videos.OrderBy(v => v.OrderIndex).ToList();
        ViewBag.CurrentVideo = currentVideoId.HasValue 
            ? orderedVideos.FirstOrDefault(v => v.Id == currentVideoId) 
            : orderedVideos.First();
        
        var currentIndex = orderedVideos.FindIndex(v => v.Id == ViewBag.CurrentVideo.Id);
        ViewBag.NextVideoId = currentIndex < orderedVideos.Count - 1 
            ? orderedVideos[currentIndex + 1].Id 
            : orderedVideos.First().Id;
            
        return View(orderedVideos);
    }

    [HttpPost]
    public IActionResult PlayNext(int currentVideoId)
    {
        var orderedVideos = _videos.OrderBy(v => v.OrderIndex).ToList();
        var currentIndex = orderedVideos.FindIndex(v => v.Id == currentVideoId);
        
        if (currentIndex < orderedVideos.Count - 1)
        {
            return RedirectToAction("Index", new { currentVideoId = orderedVideos[currentIndex + 1].Id });
        }
        
        return RedirectToAction("Index", new { currentVideoId = orderedVideos.First().Id });
    }

    [HttpPost]
    public IActionResult PlayPrevious(int currentVideoId)
    {
        var orderedVideos = _videos.OrderBy(v => v.OrderIndex).ToList();
        var currentIndex = orderedVideos.FindIndex(v => v.Id == currentVideoId);
        
        if (currentIndex > 0)
        {
            return RedirectToAction("Index", new { currentVideoId = orderedVideos[currentIndex - 1].Id });
        }
        
        return RedirectToAction("Index", new { currentVideoId = orderedVideos.Last().Id });
    }
}