namespace Anubis.Bots.Core.Models
{
    public class Post
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public List<string> Hashtags { get; set; } = new();
        public string Url { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
    }
} 