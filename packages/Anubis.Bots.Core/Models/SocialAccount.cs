namespace Anubis.Bots.Core.Models
{
    public class SocialAccount
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string? Proxy { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, string> Cookies { get; set; } = new();
        public DateTime LastUsed { get; set; }
        public bool IsActive { get; set; } = true;
        public int DailyActionCount { get; set; }
        public DateTime LastActionDate { get; set; }
    }
} 