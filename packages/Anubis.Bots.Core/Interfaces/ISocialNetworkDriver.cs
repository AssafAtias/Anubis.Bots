using Anubis.Bots.Core.Models;

namespace Anubis.Bots.Core.Interfaces
{
    public interface ISocialNetworkDriver : IDisposable
    {
        string Platform { get; }
        bool IsLoggedIn { get; }
        
        Task<bool> LoginAsync(string username, string password);
        Task<bool> LoginWithCookiesAsync(Dictionary<string, string> cookies);
        Task<bool> LogoutAsync();
        
        Task<bool> PostAsync(string content, PostOptions? options = null);
        Task<bool> LikeAsync(string postId);
        Task<bool> UnlikeAsync(string postId);
        Task<bool> FollowAsync(string userId);
        Task<bool> UnfollowAsync(string userId);
        Task<bool> SendMessageAsync(string userId, string message);
        
        Task<List<Post>> CollectPostsAsync(string hashtag, int limit = 50);
        Task<List<Post>> CollectPostsFromUserAsync(string userId, int limit = 50);
        Task<List<Post>> CollectPostsFromFeedAsync(int limit = 50);
        
        Task<bool> BrowseAsync(TimeSpan duration);
        Task<bool> BrowseHashtagAsync(string hashtag, TimeSpan duration);
        Task<bool> BrowseUserProfileAsync(string userId, TimeSpan duration);
        
        Task<Dictionary<string, string>> ExportCookiesAsync();
        Task<bool> ImportCookiesAsync(Dictionary<string, string> cookies);
        
        Task<bool> IsUserLoggedInAsync();
        Task<string> GetCurrentUserIdAsync();
    }
} 