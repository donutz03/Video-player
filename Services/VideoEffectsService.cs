namespace VideoPlayer_EasierCS.Services

{
    public class VideoEffectService
    {
        public List<VideoEffect> GetAvailableEffects()
        {
            return new List<VideoEffect>
            {
                new() { Id = 1, Name = "Grayscale", Description = "Convert video to grayscale" },
                new() { Id = 2, Name = "Sepia", Description = "Apply sepia tone effect" },
                new() { Id = 3, Name = "Invert", Description = "Invert video colors" }
            };
        }
    }

    public class VideoEffect
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
