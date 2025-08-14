using Anubis.Bots.Core.Base;
using Anubis.Bots.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Anubis.Bots.LinkedIn
{
    public class LinkedInDriver : BaseSocialNetworkDriver
    {
        public override string Platform => "LinkedIn";

        public LinkedInDriver(IBrowser browser, IPage page, ILogger? logger = null) 
            : base(browser, page, logger)
        {
        }

        protected override async Task<bool> PerformLoginAsync(string username, string password)
        {
            try
            {
                await _page.GotoAsync("https://www.linkedin.com/login");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Wait for username field to be visible
                await _page.WaitForSelectorAsync("#username", new PageWaitForSelectorOptions { Timeout = 10000 });
                
                // Fill username
                await _page.FillAsync("#username", username);
                await Task.Delay(1000);

                // Fill password
                await _page.FillAsync("#password", password);
                await Task.Delay(1000);

                // Click sign in button
                await _page.ClickAsync("button[type='submit']");
                
                // Wait for either successful login or error
                try
                {
                    await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 15000 });
                }
                catch (TimeoutException)
                {
                    // Continue anyway, might be slow network
                }

                // Check if login was successful
                var isLoggedIn = await IsUserLoggedInAsync();
                return isLoggedIn;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during LinkedIn login");
                return false;
            }
        }

        protected override async Task<bool> PerformLogoutAsync()
        {
            try
            {
                // Click on profile menu
                await _page.ClickAsync("button[aria-label='Open menu']");
                await Task.Delay(1000);

                // Click sign out
                await _page.ClickAsync("a[href*='logout']");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during LinkedIn logout");
                return false;
            }
        }

        protected override async Task<bool> PerformPostAsync(string content, PostOptions? options)
        {
            try
            {
                // Navigate to home page
                await _page.GotoAsync("https://www.linkedin.com/feed/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Click on "Start a post" button
                await _page.ClickAsync("button[aria-label='Start a post']");
                await Task.Delay(2000);

                // Fill the post content
                await _page.FillAsync("div[role='textbox']", content);
                await Task.Delay(1000);

                // Add hashtags if provided
                if (options?.Hashtags?.Any() == true)
                {
                    foreach (var hashtag in options.Hashtags)
                    {
                        await _page.FillAsync("div[role='textbox']", $" #{hashtag}");
                        await Task.Delay(500);
                    }
                }

                // Click post button
                await _page.ClickAsync("button[aria-label='Post']");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting to LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformLikeAsync(string postId)
        {
            try
            {
                // Find the like button for the specific post
                var likeButton = await _page.QuerySelectorAsync($"article[data-urn='{postId}'] button[aria-label*='Like']");
                if (likeButton != null)
                {
                    await likeButton.ClickAsync();
                    await Task.Delay(1000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error liking post on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformUnlikeAsync(string postId)
        {
            try
            {
                // Find the unlike button for the specific post
                var unlikeButton = await _page.QuerySelectorAsync($"article[data-urn='{postId}'] button[aria-label*='Unlike']");
                if (unlikeButton != null)
                {
                    await unlikeButton.ClickAsync();
                    await Task.Delay(1000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unliking post on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformFollowAsync(string userId)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.linkedin.com/in/{userId}/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Click follow button
                var followButton = await _page.QuerySelectorAsync("button[aria-label*='Follow']");
                if (followButton != null)
                {
                    await followButton.ClickAsync();
                    await Task.Delay(1000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error following user on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformUnfollowAsync(string userId)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.linkedin.com/in/{userId}/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Click unfollow button
                var unfollowButton = await _page.QuerySelectorAsync("button[aria-label*='Unfollow']");
                if (unfollowButton != null)
                {
                    await unfollowButton.ClickAsync();
                    await Task.Delay(1000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unfollowing user on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformSendMessageAsync(string userId, string message)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.linkedin.com/in/{userId}/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Click message button
                var messageButton = await _page.QuerySelectorAsync("button[aria-label*='Message']");
                if (messageButton != null)
                {
                    await messageButton.ClickAsync();
                    await Task.Delay(2000);

                    // Fill message
                    await _page.FillAsync("div[role='textbox']", message);
                    await Task.Delay(1000);

                    // Send message
                    await _page.ClickAsync("button[aria-label='Send now']");
                    await Task.Delay(1000);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending message on LinkedIn");
                return false;
            }
        }

        protected override async Task<List<Post>> PerformCollectPostsAsync(string hashtag, int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to hashtag search
                await _page.GotoAsync($"https://www.linkedin.com/search/results/content/?keywords=%23{hashtag}");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("article");
                foreach (var element in postElements.Take(limit))
                {
                    try
                    {
                        var post = await ExtractPostFromElement(element);
                        if (post != null)
                        {
                            posts.Add(post);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error extracting post");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from LinkedIn");
            }

            return posts;
        }

        protected override async Task<List<Post>> PerformCollectPostsFromUserAsync(string userId, int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.linkedin.com/in/{userId}/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("article");
                foreach (var element in postElements.Take(limit))
                {
                    try
                    {
                        var post = await ExtractPostFromElement(element);
                        if (post != null)
                        {
                            posts.Add(post);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error extracting post");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from user on LinkedIn");
            }

            return posts;
        }

        protected override async Task<List<Post>> PerformCollectPostsFromFeedAsync(int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to feed
                await _page.GotoAsync("https://www.linkedin.com/feed/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("article");
                foreach (var element in postElements.Take(limit))
                {
                    try
                    {
                        var post = await ExtractPostFromElement(element);
                        if (post != null)
                        {
                            posts.Add(post);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error extracting post");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error collecting posts from feed on LinkedIn");
            }

            return posts;
        }

        protected override async Task<bool> PerformBrowseHashtagAsync(string hashtag, TimeSpan duration)
        {
            try
            {
                await _page.GotoAsync($"https://www.linkedin.com/search/results/content/?keywords=%23{hashtag}");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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
                _logger?.LogError(ex, "Error browsing hashtag on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformBrowseUserProfileAsync(string userId, TimeSpan duration)
        {
            try
            {
                await _page.GotoAsync($"https://www.linkedin.com/in/{userId}/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

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
                _logger?.LogError(ex, "Error browsing user profile on LinkedIn");
                return false;
            }
        }

        protected override async Task<bool> PerformIsUserLoggedInAsync()
        {
            try
            {
                await _page.GotoAsync("https://www.linkedin.com/feed/");
                
                try
                {
                    await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 10000 });
                }
                catch (TimeoutException)
                {
                    // Continue anyway, might be slow network
                }

                // Check if we're redirected to login page
                if (_page.Url.Contains("/login"))
                {
                    return false;
                }

                // Check for feed elements
                var feedElement = await _page.QuerySelectorAsync("main[role='main']");
                return feedElement != null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking login status on LinkedIn");
                return false;
            }
        }

        protected override async Task<string> PerformGetCurrentUserIdAsync()
        {
            try
            {
                // Navigate to profile
                await _page.GotoAsync("https://www.linkedin.com/in/me/");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Extract user ID from URL
                var url = _page.Url;
                var match = Regex.Match(url, @"/in/([^/]+)/");
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting current user ID on LinkedIn");
                return string.Empty;
            }
        }

        private async Task<Post?> ExtractPostFromElement(IElementHandle element)
        {
            try
            {
                var post = new Post
                {
                    Platform = Platform,
                    CreatedAt = DateTime.UtcNow
                };

                // Extract post ID
                var dataUrn = await element.GetAttributeAsync("data-urn");
                post.Id = dataUrn ?? string.Empty;

                // Extract content
                var contentElement = await element.QuerySelectorAsync("span[dir='ltr']");
                if (contentElement != null)
                {
                    post.Content = await contentElement.TextContentAsync() ?? string.Empty;
                }

                // Extract author
                var authorElement = await element.QuerySelectorAsync("a[data-test-id='post-meta__author']");
                if (authorElement != null)
                {
                    post.AuthorName = await authorElement.TextContentAsync() ?? string.Empty;
                    var href = await authorElement.GetAttributeAsync("href");
                    if (!string.IsNullOrEmpty(href))
                    {
                        var match = Regex.Match(href, @"/in/([^/]+)/");
                        if (match.Success)
                        {
                            post.AuthorId = match.Groups[1].Value;
                        }
                    }
                }

                // Extract engagement metrics
                var likeElement = await element.QuerySelectorAsync("button[aria-label*='Like'] span");
                if (likeElement != null)
                {
                    var likeText = await likeElement.TextContentAsync();
                    if (int.TryParse(Regex.Match(likeText ?? "", @"\d+").Value, out var likes))
                    {
                        post.LikeCount = likes;
                    }
                }

                return post;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error extracting post data");
                return null;
            }
        }
    }
} 