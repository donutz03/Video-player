using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VideoPlayer_EasierCS.Helpers;
using VideoPlayer_EasierCS.Models;

namespace VideoPlayer_EasierCS.Controllers.Api;



[Route("api/[controller]")]
[ApiController]
public class VideosController : Controller
{
    private List<Video> _videos;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _mediaFolder = "media";
    private readonly string _thumbnailFolder = "thumbnails";


    public VideosController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _videos = LoadVideos();
    }

    [HttpGet]
    public ActionResult<IEnumerable<Video>> GetVideos(int? collectionId = null)
    {
        if (collectionId.HasValue)
        {
            return Ok(_videos.Where(v => v.CollectionId == collectionId.Value).OrderBy(v => v.OrderIndex).ToList());
        }

        return Ok(_videos.OrderBy(v => v.OrderIndex).ToList());
    }

    [HttpGet("{id")]
    public ActionResult<Video> GetVideo(int id)
    {
        var video = _videos.FirstOrDefault(v => v.Id == id);
        if (video == null)
        {
            return NotFound();
        }

        return Ok(video);
    }

    [HttpPost]
    public async Task<ActionResult<Video>> UploadVideo([FromForm] IFormFile file, [FromForm] int collectionid)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.ContentType.StartsWith("video/"))
        {
            return BadRequest("File must be a video");
        }

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

            string thumbnailPath = "/thumbnails/video-default.jpg";
            
            //TODO: here
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
            
        ViewBag.AvailableEffects = _effectService.GetAvailableEffects();
        
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
        try 
        {
            foreach (var update in updates)
            {
                var video = _videos.FirstOrDefault(v => v.Id == update.Id);
                if (video != null)
                {
                    video.OrderIndex = update.NewOrder;
                }
            }
        
            _videos = _videos.OrderBy(v => v.OrderIndex).ToList();
        
            SaveVideos();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    [HttpPost]
    public IActionResult DeleteVideo([FromBody] DeleteVideoRequest request)
    {
        var video = _videos.FirstOrDefault(v => v.Id == request.id);
        if (video != null)
        {
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