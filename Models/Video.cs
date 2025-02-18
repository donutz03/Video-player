namespace VideoPlayer_EasierCS.Models;

public class Video
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string FilePath { get; set; }
    public string SubtitlesPath { get; set; }
    public int OrderIndex { get; set; }
}