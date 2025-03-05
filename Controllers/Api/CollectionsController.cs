using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VideoPlayer_EasierCS.Models;


namespace VideoPlayer_EasierCS.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class CollectionsController : ControllerBase
{
    private List<VideoCollection> _collections;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _thumbnailFolder = "thumbnails";

    public CollectionsController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _collections = LoadCollections();

        if (!_collections.Any())
        {
            _collections = new List<VideoCollection>
            {
                new VideoCollection
                {
                    Id = 1,
                    Name = "Dexter",
                    ThumbnailPath = "/thumbnails/dexter.jpg",
                    CreatedAt = DateTime.Now,
                }
            };

            SaveCollections();
        }
    }


    [HttpGet]
    public ActionResult<IEnumerable<VideoCollection>> GetCollections()
    { 
        return Ok(_collections);
    }

    [HttpGet("{id}")]
    public ActionResult<VideoCollection> GetCollection(int id)
    {
        var collection = _collections.FirstOrDefault(c => c.Id == id);
        if (collection == null)
        {
            return NotFound();
        }

        return Ok(collection);
    }

    [HttpPost]
    public async Task<ActionResult<VideoCollection>> CreateCollection([FromForm] string name, IFormFile thumbnailImage)
    {
        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Collection name is required");
        }

        try
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, _thumbnailFolder);
            Directory.CreateDirectory(uploadsFolder);

            string thumbnailPath = "/thumbnails/dexter.jpg";

            if (thumbnailImage != null && thumbnailImage.Length > 0)
            {
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await thumbnailImage.CopyToAsync(stream);
                }

                thumbnailPath = $"/{_thumbnailFolder}/{uniqueFileName}";
            }

            var collection = new VideoCollection
            {
                Id = _collections.Count > 0 ? _collections.Max(c => c.Id) + 1 : 1,
                Name = name,
                ThumbnailPath = thumbnailPath,
                CreatedAt = DateTime.Now
            };

            _collections.Add(collection);
            SaveCollections();

            return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating collection: {ex.Message}");
        }
    }
    
    [HttpDelete("{id}")]
    

    private void SaveCollections()
    {
        var jsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "collections.json");
        var jsonContent = JsonSerializer.Serialize(_collections);
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
        System.IO.File.WriteAllText(jsonPath, jsonContent);
    }
    
    private List<VideoCollection> LoadCollections()
    {
        var jsonPath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "collections.json");
        if (System.IO.File.Exists(jsonPath))
        {
            var jsonContent = System.IO.File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<VideoCollection>>(jsonContent) ?? new List<VideoCollection>();
        }

        return new List<VideoCollection>();
    }
    
}
