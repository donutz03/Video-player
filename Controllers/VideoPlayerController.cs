using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VideoPlayer_EasierCS.Helpers;
using VideoPlayer_EasierCS.Models;

namespace VideoPlayer_EasierCS.Controllers;

public class VideoPlayerController : Controller
{
    private readonly List<Video> _videos;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _mediaFolder = "media";

    public VideoPlayerController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        
        _videos = LoadVideos();
        
        if (!_videos.Any())
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
            
            SaveVideos();
        }
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
    
    private List<Video> LoadVideos()
    {
        var jsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "videos.json");
        if (System.IO.File.Exists(jsonPath))
        {
            var jsonContent = System.IO.File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<Video>>(jsonContent) ?? new List<Video>();
        }
        return new List<Video>();
    }
    
    private void SaveVideos()
    {
        var jsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "videos.json");
        var jsonContent = JsonSerializer.Serialize(_videos);
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
        System.IO.File.WriteAllText(jsonPath, jsonContent);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (!file.ContentType.StartsWith("video/"))
            return BadRequest("File must be a video");

        try
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, _mediaFolder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var video = new Video
            {
                Id = _videos.Count > 0 ? _videos.Max(v => v.Id) + 1 : 1,
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                FilePath = $"/{_mediaFolder}/{uniqueFileName}",
                OrderIndex = _videos.Count
            };

            _videos.Add(video);
            SaveVideos();

            return Json(new { success = true, video });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error uploading file: {ex.Message}");
        }
    }
    
    [HttpPost]
    public IActionResult UpdateOrder([FromBody] List<VideoOrderUpdate> updates)
    {
        foreach (var update in updates)
        {
            var video = _videos.FirstOrDefault(v => v.Id == update.Id);
            if (video != null)
            {
                video.OrderIndex = update.NewOrder;
            }
        }
        
        SaveVideos();
        return Json(new { success = true });
    }
    
    [HttpPost]
    public IActionResult DeleteVideo(int id)
    {
        var video = _videos.FirstOrDefault(v => v.Id == id);
        if (video != null)
        {
            // Delete the physical file
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, video.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _videos.Remove(video);
            var orderedVideos = _videos.OrderBy(v => v.OrderIndex).ToList();
            for (int i = 0; i < orderedVideos.Count; i++)
            {
                orderedVideos[i].OrderIndex = i;
            }
            
            SaveVideos();
            return Json(new { success = true });
        }
        
        return Json(new { success = false, message = "Video not found" });
    }
}