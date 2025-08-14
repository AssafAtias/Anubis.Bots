using Anubis.Bots.Core.Interfaces;
using Anubis.Bots.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Polly;
using System.Text.Json;

namespace Anubis.Bots.Core.Base
{
    public abstract class BaseSocialNetworkDriver : ISocialNetworkDriver
    {
        protected readonly IPlaywright _playwright;
        protected readonly IBrowser _browser;
        protected readonly IPage _page;
        protected readonly ILogger? _logger;
        protected readonly Random _random;
        
        public abstract string Platform { get; }
        public bool IsLoggedIn { get; protected set; }

        protected BaseSocialNetworkDriver(IBrowser browser, IPage page, ILogger? logger = null)
        {
            _browser = browser;
            _page = page;
            _logger = logger;
            _random = new Random();
            _playwright = Playwright.CreateAsync().Result;
        }

        public virtual async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logger?.LogInformation("Logging in to {Platform} with username: {Username}", Platform, username);
                
                var result = await PerformLoginAsync(username, password);
                IsLoggedIn = result;
                
                if (result)
                {
                    _logger?.LogInformation("Successfully logged in to {Platform}", Platform);
                    await AddHumanBehaviorAsync();
                }
                else
                {
                    _logger?.LogWarning("Failed to log in to {Platform}", Platform);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during login to {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> LoginWithCookiesAsync(Dictionary<string, string> cookies)
        {
            try
            {
                _logger?.LogInformation("Logging in to {Platform} with cookies", Platform);
                
                await ImportCookiesAsync(cookies);
                var result = await IsUserLoggedInAsync();
                IsLoggedIn = result;
                
                if (result)
                {
                    _logger?.LogInformation("Successfully logged in to {Platform} with cookies", Platform);
                }
                else
                {
                    _logger?.LogWarning("Failed to log in to {Platform} with cookies", Platform);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during cookie login to {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> LogoutAsync()
        {
            try
            {
                _logger?.LogInformation("Logging out from {Platform}", Platform);
                
                var result = await PerformLogoutAsync();
                IsLoggedIn = !result;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during logout from {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> PostAsync(string content, PostOptions? options = null)
        {
            try
            {
                _logger?.LogInformation("Posting content to {Platform}", Platform);
                
                if (options?.DelayBeforePost.HasValue == true)
                {
                    await Task.Delay(options.DelayBeforePost.Value);
                }
                
                var result = await PerformPostAsync(content, options);
                
                if (options?.DelayAfterPost.HasValue == true)
                {
                    await Task.Delay(options.DelayAfterPost.Value);
                }
                
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting to {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> LikeAsync(string postId)
        {
            try
            {
                _logger?.LogInformation("Liking post {PostId} on {Platform}", postId, Platform);
                
                var result = await PerformLikeAsync(postId);
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error liking post on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> UnlikeAsync(string postId)
        {
            try
            {
                _logger?.LogInformation("Unliking post {PostId} on {Platform}", postId, Platform);
                
                var result = await PerformUnlikeAsync(postId);
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unliking post on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> FollowAsync(string userId)
        {
            try
            {
                _logger?.LogInformation("Following user {UserId} on {Platform}", userId, Platform);
                
                var result = await PerformFollowAsync(userId);
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error following user on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> UnfollowAsync(string userId)
        {
            try
            {
                _logger?.LogInformation("Unfollowing user {UserId} on {Platform}", userId, Platform);
                
                var result = await PerformUnfollowAsync(userId);
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unfollowing user on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> SendMessageAsync(string userId, string message)
        {
            try
            {
                _logger?.LogInformation("Sending message to user {UserId} on {Platform}", userId, Platform);
                
                var result = await PerformSendMessageAsync(userId, message);
                await AddHumanBehaviorAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending message on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<List<Post>> CollectPostsAsync(string hashtag, int limit = 50)
        {
            try
            {
                _logger?.LogInformation("Collecting posts with hashtag {Hashtag} from {Platform}", hashtag, Platform);
                
                var posts = await PerformCollectPostsAsync(hashtag, limit);
                await AddHumanBehaviorAsync();
                return posts;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from {Platform}", Platform);
                return new List<Post>();
            }
        }

        public virtual async Task<List<Post>> CollectPostsFromUserAsync(string userId, int limit = 50)
        {
            try
            {
                _logger?.LogInformation("Collecting posts from user {UserId} on {Platform}", userId, Platform);
                
                var posts = await PerformCollectPostsFromUserAsync(userId, limit);
                await AddHumanBehaviorAsync();
                return posts;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from user on {Platform}", Platform);
                return new List<Post>();
            }
        }

        public virtual async Task<List<Post>> CollectPostsFromFeedAsync(int limit = 50)
        {
            try
            {
                _logger?.LogInformation("Collecting posts from feed on {Platform}", Platform);
                
                var posts = await PerformCollectPostsFromFeedAsync(limit);
                await AddHumanBehaviorAsync();
                return posts;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from feed on {Platform}", Platform);
                return new List<Post>();
            }
        }

        public virtual async Task<bool> BrowseAsync(TimeSpan duration)
        {
            try
            {
                _logger?.LogInformation("Browsing {Platform} for {Duration}", Platform, duration);
                
                var startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < duration)
                {
                    await AddHumanBehaviorAsync();
                    await Task.Delay(TimeSpan.FromSeconds(_random.Next(5, 15)));
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error browsing {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> BrowseHashtagAsync(string hashtag, TimeSpan duration)
        {
            try
            {
                _logger?.LogInformation("Browsing hashtag {Hashtag} on {Platform} for {Duration}", hashtag, Platform, duration);
                
                var result = await PerformBrowseHashtagAsync(hashtag, duration);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error browsing hashtag on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> BrowseUserProfileAsync(string userId, TimeSpan duration)
        {
            try
            {
                _logger?.LogInformation("Browsing user profile {UserId} on {Platform} for {Duration}", userId, Platform, duration);
                
                var result = await PerformBrowseUserProfileAsync(userId, duration);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error browsing user profile on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<Dictionary<string, string>> ExportCookiesAsync()
        {
            try
            {
                var cookies = await _page.Context.CookiesAsync();
                return cookies.GroupBy(c => c.Name)
                             .ToDictionary(g => g.Key, g => g.Last().Value);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting cookies from {Platform}", Platform);
                return new Dictionary<string, string>();
            }
        }

        public virtual async Task<bool> ImportCookiesAsync(Dictionary<string, string> cookies)
        {
            try
            {
                var cookieList = cookies.Select(kvp => new Cookie
                {
                    Name = kvp.Key,
                    Value = kvp.Value,
                    Domain = GetCookieDomain(),
                    Path = "/"
                }).ToArray();
                
                await _page.Context.AddCookiesAsync(cookieList);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error importing cookies to {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<bool> IsUserLoggedInAsync()
        {
            try
            {
                return await PerformIsUserLoggedInAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking login status on {Platform}", Platform);
                return false;
            }
        }

        public virtual async Task<string> GetCurrentUserIdAsync()
        {
            try
            {
                return await PerformGetCurrentUserIdAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting current user ID on {Platform}", Platform);
                return string.Empty;
            }
        }

        protected virtual async Task AddHumanBehaviorAsync()
        {
            // Random mouse movements
            await _page.Mouse.MoveAsync(_random.Next(100, 800), _random.Next(100, 600));
            
            // Random scroll
            await _page.Mouse.WheelAsync(0, _random.Next(-300, 300));
            
            // Random delay
            await Task.Delay(_random.Next(1000, 3000));
        }

        protected virtual string GetCookieDomain()
        {
            return _page.Url.Contains("linkedin.com") ? ".linkedin.com" :
                   _page.Url.Contains("facebook.com") ? ".facebook.com" :
                   _page.Url.Contains("instagram.com") ? ".instagram.com" :
                   _page.Url.Contains("tiktok.com") ? ".tiktok.com" : "";
        }

        // Abstract methods that must be implemented by platform-specific drivers
        protected abstract Task<bool> PerformLoginAsync(string username, string password);
        protected abstract Task<bool> PerformLogoutAsync();
        protected abstract Task<bool> PerformPostAsync(string content, PostOptions? options);
        protected abstract Task<bool> PerformLikeAsync(string postId);
        protected abstract Task<bool> PerformUnlikeAsync(string postId);
        protected abstract Task<bool> PerformFollowAsync(string userId);
        protected abstract Task<bool> PerformUnfollowAsync(string userId);
        protected abstract Task<bool> PerformSendMessageAsync(string userId, string message);
        protected abstract Task<List<Post>> PerformCollectPostsAsync(string hashtag, int limit);
        protected abstract Task<List<Post>> PerformCollectPostsFromUserAsync(string userId, int limit);
        protected abstract Task<List<Post>> PerformCollectPostsFromFeedAsync(int limit);
        protected abstract Task<bool> PerformBrowseHashtagAsync(string hashtag, TimeSpan duration);
        protected abstract Task<bool> PerformBrowseUserProfileAsync(string userId, TimeSpan duration);
        protected abstract Task<bool> PerformIsUserLoggedInAsync();
        protected abstract Task<string> PerformGetCurrentUserIdAsync();

        public virtual async ValueTask DisposeAsync()
        {
            try
            {
                if (_page != null)
                {
                    await _page.CloseAsync();
                }
                
                if (_browser != null)
                {
                    await _browser.CloseAsync();
                }
                
                if (_playwright != null)
                {
                    _playwright.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disposing resources");
            }
        }

        public virtual void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }
    }
} 