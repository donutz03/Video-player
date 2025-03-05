using System.Text.Json.Serialization;

namespace VideoPlayer_EasierCS.Models;

public class VideoCollection
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ThumbnailPath { get; set; }
    public DateTime CreatedAt { get; set; }

    [JsonIgnore] public List<Video> Videos { get; set; } = new List<Video>();
}