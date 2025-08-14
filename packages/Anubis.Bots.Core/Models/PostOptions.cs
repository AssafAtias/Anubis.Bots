namespace Anubis.Bots.Core.Models
{
    public class PostOptions
    {
        public List<string> Hashtags { get; set; } = new();
        public List<string> Mentions { get; set; } = new();
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }
        public bool IsPublic { get; set; } = true;
        public TimeSpan? DelayBeforePost { get; set; }
        public TimeSpan? DelayAfterPost { get; set; }
    }
} 