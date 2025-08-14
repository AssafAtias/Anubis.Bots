using Anubis.Bots.Core.Base;
using Anubis.Bots.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Anubis.Bots.TikTok
{
    public class TikTokDriver : BaseSocialNetworkDriver
    {
        public override string Platform => "TikTok";

        // Cache for captcha detection to avoid repeated expensive operations
        private DateTime? _lastPageContentCheck;
        private string? _cachedPageContent;

        public TikTokDriver(IBrowser browser, IPage page, ILogger? logger = null) 
            : base(browser, page, logger)
        {
        }

        protected override async Task<bool> PerformLoginAsync(string username, string password)
        {
            try
            {
                _logger?.LogInformation("=== Starting TikTok Login Process ===");
                
                // Navigate to TikTok main page
                await _page.GotoAsync("https://www.tiktok.com/");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await Task.Delay(3000);

                _logger?.LogInformation($"Page title: {await _page.TitleAsync()}");
                _logger?.LogInformation($"Current URL: {_page.Url}");

                // Take a screenshot for debugging
                await _page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-main-page.png" });
                _logger?.LogInformation("✓ Screenshot saved: tiktok-main-page.png");

                // Check for captcha before proceeding
                if (await IsCaptchaPresentAsync())
                {
                    _logger?.LogInformation("Captcha detected on main page, waiting for solution...");
                    if (!await WaitForCaptchaSolutionAsync())
                    {
                        _logger?.LogWarning("Captcha solution failed or timed out");
                        return false;
                    }
                }

                // Step 1: Click on Log In button on main page
                _logger?.LogInformation("Looking for Log In button...");
                var logInSelectors = new[]
                {
                    "button#top-right-action-bar-login-button",
                };

                IElementHandle? logInButton = null;
                foreach (var selector in logInSelectors)
                {
                    try
                    {
                        // Debug: Check how many elements match this selector
                        var allMatches = await _page.QuerySelectorAllAsync(selector);
                        _logger?.LogInformation($"Selector '{selector}' found {allMatches.Count} elements");
                        
                        // Use QuerySelectorAsync instead of QuerySelectorAllAsync to get only the first match
                        logInButton = await _page.QuerySelectorAsync(selector);
                        if (logInButton != null)
                        {
                            _logger?.LogInformation($"Found Log In button with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                if (logInButton == null)
                {
                    _logger?.LogWarning("Could not find Log In button");
                    return false;
                }

                // Click Log In button
                try
                {
                    // Debug: Check button state
                    var isVisible = await logInButton.IsVisibleAsync();
                    var isEnabled = await logInButton.IsEnabledAsync();
                    _logger?.LogInformation($"Log In button - Visible: {isVisible}, Enabled: {isEnabled}");
                    
                    // Try to scroll the button into view first
                    await logInButton.ScrollIntoViewIfNeededAsync();
                    await Task.Delay(1000);
                    
                    // Check again after scrolling
                    isVisible = await logInButton.IsVisibleAsync();
                    isEnabled = await logInButton.IsEnabledAsync();
                    _logger?.LogInformation($"Log In button after scroll - Visible: {isVisible}, Enabled: {isEnabled}");
                    
                    // Try different click strategies
                    try
                    {
                        await logInButton.ClickAsync();
                    }
                    catch
                    {
                        // If direct click fails, try JavaScript click
                        await _page.EvaluateAsync("arguments[0].click();", logInButton);
                    }
                    
                    _logger?.LogInformation("Clicked Log In button");
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Could not click Log In button: {ex.Message}");
                    return false;
                }

                // Check for captcha after clicking login
                // if (await IsCaptchaPresentAsync())
                // {
                //     _logger?.LogInformation("Captcha detected after login click, waiting for solution...");
                //     if (!await WaitForCaptchaSolutionAsync())
                //     {
                //         _logger?.LogWarning("Captcha solution failed or timed out");
                //         return false;
                //     }
                // }

                // Step 2: Choose "Use phone / email / username" option
                _logger?.LogInformation("Looking for 'Use phone / email / username' option...");
                var phoneEmailSelectors = new[]
                {
                    "div[role='link']:has-text('Use phone / email / username')"
                };

                IElementHandle? phoneEmailButton = null;
                foreach (var selector in phoneEmailSelectors)
                {
                    try
                    {
                        phoneEmailButton = await _page.QuerySelectorAsync(selector);
                        if (phoneEmailButton != null)
                        {
                            _logger?.LogInformation($"Found 'Use phone / email / username' with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                if (phoneEmailButton != null)
                {
                    try
                    {
                        // Try to scroll the element into view first
                        await phoneEmailButton.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(1000);
                        
                        // Check if element is visible and enabled
                        var isVisible = await phoneEmailButton.IsVisibleAsync();
                        var isEnabled = await phoneEmailButton.IsEnabledAsync();
                        _logger?.LogInformation($"Phone/email button - Visible: {isVisible}, Enabled: {isEnabled}");
                        
                        // Try different click strategies
                        try
                        {
                            await phoneEmailButton.ClickAsync();
                        }
                        catch
                        {
                            // If direct click fails, try JavaScript click
                            await _page.EvaluateAsync("arguments[0].click();", phoneEmailButton);
                        }
                        
                        _logger?.LogInformation("Clicked 'Use phone / email / username' option");
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning($"Could not click 'Use phone / email / username' option: {ex.Message}");
                    }
                }
                else
                {
                    _logger?.LogInformation("'Use phone / email / username' option not found, continuing...");
                }

                // Check for captcha after phone/email selection
                // if (await IsCaptchaPresentAsync())
                // {
                //     _logger?.LogInformation("Captcha detected after phone/email selection, waiting for solution...");
                //     if (!await WaitForCaptchaSolutionAsync())
                //     {
                //         _logger?.LogWarning("Captcha solution failed or timed out");
                //         return false;
                //     }
                // }

                // Step 3: Click "Log in with email or username" link
                _logger?.LogInformation("Looking for 'Log in with email or username' link...");
                var emailUsernameSelectors = new[]
                {
                    "a[href='/login/phone-or-email/email']"
                };

                IElementHandle? emailUsernameButton = null;
                foreach (var selector in emailUsernameSelectors)
                {
                    try
                    {
                        emailUsernameButton = await _page.QuerySelectorAsync(selector);
                        if (emailUsernameButton != null)
                        {
                            _logger?.LogInformation($"Found 'Log in with email or username' with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                if (emailUsernameButton != null)
                {
                    try
                    {
                        // Try to scroll the element into view first
                        await emailUsernameButton.ScrollIntoViewIfNeededAsync();
                        await Task.Delay(1000);
                        
                        // Check if element is visible and enabled
                        var isVisible = await emailUsernameButton.IsVisibleAsync();
                        var isEnabled = await emailUsernameButton.IsEnabledAsync();
                        _logger?.LogInformation($"Email/username link - Visible: {isVisible}, Enabled: {isEnabled}");
                        
                        // Try different click strategies
                        try
                        {
                            await emailUsernameButton.ClickAsync();
                        }
                        catch
                        {
                            // If direct click fails, try JavaScript click
                            await _page.EvaluateAsync("arguments[0].click();", emailUsernameButton);
                        }
                        
                        _logger?.LogInformation("Clicked 'Log in with email or username' link");
                        await Task.Delay(3000); // Give more time for the page to load
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning($"Could not click 'Log in with email or username' link: {ex.Message}");
                    }
                }
                else
                {
                    _logger?.LogInformation("'Log in with email or username' link not found, continuing...");
                }

                // Check for captcha after email/username selection
                // if (await IsCaptchaPresentAsync())
                // {
                //     _logger?.LogInformation("Captcha detected after email/username selection, waiting for solution...");
                //     if (!await WaitForCaptchaSolutionAsync())
                //     {
                //         _logger?.LogWarning("Captcha solution failed or timed out");
                //         return false;
                //     }
                // }

                // Step 4: Enter email/username
                _logger?.LogInformation("Looking for email/username input field...");
                var usernameSelectors = new[]
                {
                    "input[name='email']",
                    "input[type='email']",
                    "input[placeholder*='Email']",
                    "input[placeholder*='email']",
                    "input[name='username']",
                    "input[type='text']",
                    "input[placeholder*='username']",
                    "input[placeholder*='Username']",
                    "input[placeholder*='phone']",
                    "input[placeholder*='Phone']",
                    "[data-e2e='login-username']",
                    "[data-e2e='login-email']",
                    "input[autocomplete='email']",
                    "input[autocomplete='username']"
                };

                IElementHandle? usernameInput = null;
                foreach (var selector in usernameSelectors)
                {
                    try
                    {
                        usernameInput = await _page.QuerySelectorAsync(selector);
                        if (usernameInput != null)
                        {
                            _logger?.LogInformation($"Found username input with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                if (usernameInput == null)
                {
                    _logger?.LogWarning("Could not find username input field");
                    return false;
                }

                // Fill username with human-like typing
                await HumanLikeTypingAsync(usernameInput, username);
                _logger?.LogInformation("Entered username/email");
                await Task.Delay(Random.Shared.Next(1000, 2000)); // Random delay

                // Step 5: Enter password
                _logger?.LogInformation("Looking for password input field...");
                var passwordSelectors = new[]
                {
                    "input[name='password']",
                    "input[type='password']",
                    "input[placeholder*='Password']",
                    "input[placeholder*='password']",
                    "[data-e2e='login-password']",
                    "input[autocomplete='current-password']"
                };

                IElementHandle? passwordInput = null;
                foreach (var selector in passwordSelectors)
                {
                    try
                    {
                        passwordInput = await _page.QuerySelectorAsync(selector);
                        if (passwordInput != null)
                        {
                            _logger?.LogInformation($"Found password input with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                if (passwordInput == null)
                {
                    _logger?.LogWarning("Could not find password input field");
                    return false;
                }

                // Fill password with human-like typing
                await HumanLikeTypingAsync(passwordInput, password);
                _logger?.LogInformation("Entered password");
                await Task.Delay(Random.Shared.Next(1500, 3000)); // Longer random delay before submit

                // Step 6: Click submit button
                _logger?.LogInformation("Looking for submit button...");
                var submitSelectors = new[]
                {
                    "button[type='submit'][data-e2e='login-button']"
                };

                IElementHandle? submitButton = null;
                foreach (var selector in submitSelectors)
                {
                    try
                    {
                        // Debug: Check how many elements match this selector
                        var allMatches = await _page.QuerySelectorAllAsync(selector);
                        _logger?.LogInformation($"Submit selector '{selector}' found {allMatches.Count} elements");
                        
                        submitButton = await _page.QuerySelectorAsync(selector);
                        if (submitButton != null)
                        {
                            _logger?.LogInformation($"Found submit button with selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning($"Error with submit selector '{selector}': {ex.Message}");
                        // Continue to next selector
                    }
                }

                if (submitButton == null)
                {
                    _logger?.LogWarning("Could not find submit button");
                    return false;
                }

                // Wait for button to be ready and clickable
                try
                {
                    await submitButton.WaitForElementStateAsync(ElementState.Visible, new ElementHandleWaitForElementStateOptions { Timeout = 5000 });
                    await submitButton.WaitForElementStateAsync(ElementState.Enabled, new ElementHandleWaitForElementStateOptions { Timeout = 5000 });
                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning("Submit button not ready, but attempting to click anyway...");
                }

                // Click submit button
                try
                {
                    // Try to scroll the button into view first
                    await submitButton.ScrollIntoViewIfNeededAsync();
                    await Task.Delay(1000);
                    
                    // Check button state
                    var isVisible = await submitButton.IsVisibleAsync();
                    var isEnabled = await submitButton.IsEnabledAsync();
                    _logger?.LogInformation($"Submit button - Visible: {isVisible}, Enabled: {isEnabled}");
                    
                    // Try different click strategies
                    try
                    {
                        await submitButton.ClickAsync();
                    }
                    catch
                    {
                        // If direct click fails, try JavaScript click
                        await _page.EvaluateAsync("arguments[0].click();", submitButton);
                    }
                    
                    _logger?.LogInformation("Clicked submit button");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Could not click submit button: {ex.Message}");
                    return false;
                }

                // Wait for login result and check for captcha
                try
                {
                    await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 15000 });
                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning("Login response timed out, but continuing...");
                }

                // Check for various error messages and captcha
                await Task.Delay(3000); // Wait for any error messages to appear
                
                var currentUrl = _page.Url;
                var pageContent = await _page.ContentAsync();
                
                // Check for "Something went wrong" error
                if (pageContent.Contains("Something went wrong") || pageContent.Contains("Sorry about that"))
                {
                    _logger?.LogWarning("TikTok returned 'Something went wrong' error - likely anti-bot protection");
                    
                    // Check if it's a captcha
                    if (await IsCaptchaPresentAsync())
                    {
                        _logger?.LogInformation("Captcha detected after login attempt, waiting for solution...");
                        if (!await WaitForCaptchaSolutionAsync())
                        {
                            _logger?.LogWarning("Captcha solution failed or timed out");
                            return false;
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("No captcha detected, but login failed. This might be due to:");
                        _logger?.LogWarning("1. Account temporarily blocked");
                        _logger?.LogWarning("2. IP address flagged");
                        _logger?.LogWarning("3. Browser fingerprinting detected");
                        _logger?.LogWarning("4. Too many login attempts");
                        
                        // Try to get more specific error information
                        var errorElements = await _page.QuerySelectorAllAsync("[class*='error'], [class*='Error'], [data-e2e*='error']");
                        foreach (var errorElement in errorElements)
                        {
                            var errorText = await errorElement.TextContentAsync();
                            if (!string.IsNullOrEmpty(errorText))
                            {
                                _logger?.LogWarning($"Error message: {errorText.Trim()}");
                            }
                        }
                        
                        return false;
                    }
                }

                // Check for captcha after login attempt
                if (await IsCaptchaPresentAsync())
                {
                    _logger?.LogInformation("Captcha detected after login attempt, waiting for solution...");
                    if (!await WaitForCaptchaSolutionAsync())
                    {
                        _logger?.LogWarning("Captcha solution failed or timed out");
                        return false;
                    }
                }

                // Check if login was successful
                var isLoggedIn = await IsUserLoggedInAsync();
                return isLoggedIn;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during TikTok login");
                return false;
            }
        }

        protected override async Task<bool> PerformLogoutAsync()
        {
            try
            {
                // Click on profile menu
                await _page.ClickAsync("button[data-e2e='profile-icon']");
                await Task.Delay(1000);

                // Click settings
                await _page.ClickAsync("a[href*='settings']");
                await Task.Delay(1000);

                // Click logout
                await _page.ClickAsync("button[data-e2e='logout-button']");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during TikTok logout");
                return false;
            }
        }

        protected override async Task<bool> PerformPostAsync(string content, PostOptions? options)
        {
            try
            {
                // Navigate to create post page
                await _page.GotoAsync("https://www.tiktok.com/upload");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Wait for text input area
                await _page.WaitForSelectorAsync("div[data-e2e='text-input']", new PageWaitForSelectorOptions { Timeout = 10000 });
                
                // Fill the post content
                await _page.FillAsync("div[data-e2e='text-input']", content);
                await Task.Delay(1000);

                // Add hashtags if provided
                if (options?.Hashtags?.Any() == true)
                {
                    foreach (var hashtag in options.Hashtags)
                    {
                        await _page.FillAsync("div[data-e2e='text-input']", $" #{hashtag}");
                        await Task.Delay(500);
                    }
                }

                // Click post button
                await _page.ClickAsync("button[data-e2e='post-button']");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error posting to TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformLikeAsync(string postId)
        {
            try
            {
                // Find the like button for the specific post
                var likeButton = await _page.QuerySelectorAsync($"div[data-e2e='like-button'][data-post-id='{postId}']");
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
                _logger?.LogError(ex, "Error liking post on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformUnlikeAsync(string postId)
        {
            try
            {
                // Find the unlike button for the specific post
                var unlikeButton = await _page.QuerySelectorAsync($"div[data-e2e='unlike-button'][data-post-id='{postId}']");
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
                _logger?.LogError(ex, "Error unliking post on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformFollowAsync(string userId)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.tiktok.com/@{userId}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Click follow button
                var followButton = await _page.QuerySelectorAsync("button[data-e2e='follow-button']");
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
                _logger?.LogError(ex, "Error following user on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformUnfollowAsync(string userId)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.tiktok.com/@{userId}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Click unfollow button
                var unfollowButton = await _page.QuerySelectorAsync("button[data-e2e='unfollow-button']");
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
                _logger?.LogError(ex, "Error unfollowing user on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformSendMessageAsync(string userId, string message)
        {
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.tiktok.com/@{userId}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Click message button
                var messageButton = await _page.QuerySelectorAsync("button[data-e2e='message-button']");
                if (messageButton != null)
                {
                    await messageButton.ClickAsync();
                    await Task.Delay(2000);

                    // Fill message
                    await _page.FillAsync("div[data-e2e='message-input']", message);
                    await Task.Delay(1000);

                    // Send message
                    await _page.ClickAsync("button[data-e2e='send-button']");
                    await Task.Delay(1000);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending message on TikTok");
                return false;
            }
        }

        protected override async Task<List<Post>> PerformCollectPostsAsync(string hashtag, int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to hashtag search
                await _page.GotoAsync($"https://www.tiktok.com/tag/{hashtag}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("div[data-e2e='feed-item']");
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
                _logger?.LogError(ex, "Error collecting posts from TikTok");
            }

            return posts;
        }

        protected override async Task<List<Post>> PerformCollectPostsFromUserAsync(string userId, int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to user profile
                await _page.GotoAsync($"https://www.tiktok.com/@{userId}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("div[data-e2e='user-post']");
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
                _logger?.LogError(ex, "Error collecting posts from user on TikTok");
            }

            return posts;
        }

        protected override async Task<List<Post>> PerformCollectPostsFromFeedAsync(int limit)
        {
            var posts = new List<Post>();
            try
            {
                // Navigate to feed
                await _page.GotoAsync("https://www.tiktok.com/foryou");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Scroll to load more posts
                for (int i = 0; i < limit / 10; i++)
                {
                    await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await Task.Delay(2000);
                }

                // Extract posts
                var postElements = await _page.QuerySelectorAllAsync("div[data-e2e='feed-item']");
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
                _logger?.LogError(ex, "Error collecting posts from feed on TikTok");
            }

            return posts;
        }

        protected override async Task<bool> PerformBrowseHashtagAsync(string hashtag, TimeSpan duration)
        {
            try
            {
                await _page.GotoAsync($"https://www.tiktok.com/tag/{hashtag}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

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
                _logger?.LogError(ex, "Error browsing hashtag on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformBrowseUserProfileAsync(string userId, TimeSpan duration)
        {
            try
            {
                await _page.GotoAsync($"https://www.tiktok.com/@{userId}");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

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
                _logger?.LogError(ex, "Error browsing user profile on TikTok");
                return false;
            }
        }

        protected override async Task<bool> PerformIsUserLoggedInAsync()
        {
            try
            {
                await _page.GotoAsync("https://www.tiktok.com/foryou");
                
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
                var feedElement = await _page.QuerySelectorAsync("div[data-e2e='feed-container']");
                return feedElement != null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking login status on TikTok");
                return false;
            }
        }

        protected override async Task<string> PerformGetCurrentUserIdAsync()
        {
            try
            {
                // Navigate to profile
                await _page.GotoAsync("https://www.tiktok.com/@me");
                await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                // Extract user ID from URL
                var url = _page.Url;
                var match = Regex.Match(url, @"/@([^/]+)");
                return match.Success ? match.Groups[1].Value : string.Empty;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting current user ID on TikTok");
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
                var dataId = await element.GetAttributeAsync("data-id");
                post.Id = dataId ?? string.Empty;

                // Extract content
                var contentElement = await element.QuerySelectorAsync("div[data-e2e='post-content']");
                if (contentElement != null)
                {
                    post.Content = await contentElement.TextContentAsync() ?? string.Empty;
                }

                // Extract author
                var authorElement = await element.QuerySelectorAsync("a[data-e2e='post-author']");
                if (authorElement != null)
                {
                    post.AuthorName = await authorElement.TextContentAsync() ?? string.Empty;
                    var href = await authorElement.GetAttributeAsync("href");
                    if (!string.IsNullOrEmpty(href))
                    {
                        var match = Regex.Match(href, @"/@([^/]+)");
                        if (match.Success)
                        {
                            post.AuthorId = match.Groups[1].Value;
                        }
                    }
                }

                // Extract engagement metrics
                var likeElement = await element.QuerySelectorAsync("div[data-e2e='like-count']");
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

        /// <summary>
        /// Detects if a captcha is present on the current page
        /// </summary>
        /// <returns>True if captcha is detected, false otherwise</returns>
        public async Task<bool> IsCaptchaPresentAsync()
        {
            try
            {
                // Quick check for TikTok-specific captcha types first (most common)
                var tiktokCaptchaSelectors = new[]
                {
                    "div:has-text('Drag the slider to fit the puzzle')",
                    "div[class*='captcha']",
                    "div[class*='verify']",
                    "div[class*='challenge']",
                    "div[class*='slider']",
                    "div[class*='puzzle']"
                };

                foreach (var selector in tiktokCaptchaSelectors)
                {
                    try
                    {
                        var element = await _page.QuerySelectorAsync(selector);
                        if (element != null && await element.IsVisibleAsync())
                        {
                            _logger?.LogInformation($"TikTok captcha detected with selector: {selector}");
                            return true;
                        }
                    }
                    catch
                    {
                        // Continue to next selector
                    }
                }

                // Quick check for reCAPTCHA iframe
                try
                {
                    var recaptchaFrame = await _page.QuerySelectorAsync("iframe[src*='recaptcha']");
                    if (recaptchaFrame != null && await recaptchaFrame.IsVisibleAsync())
                    {
                        _logger?.LogInformation("reCAPTCHA detected");
                        return true;
                    }
                }
                catch
                {
                    // Continue
                }

                // Only do expensive page content check if quick checks failed
                // and we haven't done it recently (cache for 5 seconds)
                var now = DateTime.UtcNow;
                if (_lastPageContentCheck == null || now - _lastPageContentCheck > TimeSpan.FromSeconds(5))
                {
                    _lastPageContentCheck = now;
                    _cachedPageContent = await _page.ContentAsync();
                }

                // Check cached page content for captcha-related text
                var pageContent = _cachedPageContent ?? "";
                var captchaKeywords = new[]
                {
                    "captcha",
                    "verify",
                    "challenge",
                    "robot",
                    "human verification",
                    "security check",
                    "drag the slider",
                    "fit the puzzle",
                    "puzzle piece"
                };

                foreach (var keyword in captchaKeywords)
                {
                    if (pageContent.ToLower().Contains(keyword.ToLower()))
                    {
                        _logger?.LogInformation($"Captcha-related keyword found: {keyword}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Error checking for captcha: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clears the page content cache to force a fresh check
        /// </summary>
        public void ClearPageContentCache()
        {
            _lastPageContentCheck = null;
            _cachedPageContent = null;
            _logger?.LogInformation("Page content cache cleared");
        }

        /// <summary>
        /// Waits for the captcha solver extension to solve the captcha
        /// </summary>
        /// <param name="timeoutSeconds">Maximum time to wait for solution (default: 60 seconds)</param>
        /// <returns>True if captcha was solved, false if timed out or failed</returns>
        public async Task<bool> WaitForCaptchaSolutionAsync(int timeoutSeconds = 60)
        {
            try
            {
                _logger?.LogInformation($"Waiting for captcha solution (timeout: {timeoutSeconds}s)...");
                
                var startTime = DateTime.UtcNow;
                var timeout = TimeSpan.FromSeconds(timeoutSeconds);
                var checkCount = 0;
                var maxChecks = timeoutSeconds / 5; // Maximum checks based on 5-second intervals
                var lastUrl = _page.Url;

                while (DateTime.UtcNow - startTime < timeout && checkCount < maxChecks)
                {
                    checkCount++;
                    var currentUrl = _page.Url;
                    
                    // Check if URL changed (indicates captcha was solved and page redirected)
                    if (currentUrl != lastUrl)
                    {
                        _logger?.LogInformation($"URL changed from {lastUrl} to {currentUrl} - captcha likely solved");
                        lastUrl = currentUrl;
                        
                        // If we're no longer on a captcha/verify page, consider it solved
                        if (!currentUrl.Contains("captcha") && !currentUrl.Contains("verify") && !currentUrl.Contains("challenge"))
                        {
                            _logger?.LogInformation("Captcha solution successful - redirected away from captcha page");
                            return true;
                        }
                    }
                    
                    // Check if captcha is still present
                    var captchaPresent = await IsCaptchaPresentAsync();
                    if (!captchaPresent)
                    {
                        _logger?.LogInformation($"Captcha no longer detected after {checkCount} checks!");
                        
                        // Wait a bit more to ensure the page has processed the solution
                        await Task.Delay(3000);
                        
                        // Double-check that captcha is really gone
                        if (!await IsCaptchaPresentAsync())
                        {
                            _logger?.LogInformation("Captcha confirmed to be solved!");
                            
                            // Check for TikTok-specific success indicators
                            var tiktokSuccessIndicators = new[]
                            {
                                // Login form elements
                                "input[name='password']",
                                "input[name='email']",
                                "input[name='username']",
                                "button[data-e2e='login-button']:not([disabled])",
                                "div[data-e2e='login-form']",
                                
                                // Main TikTok page elements (if we're past login)
                                "div[data-e2e='feed-container']",
                                "div[data-e2e='sidebar']",
                                "div[data-e2e='profile-icon']",
                                
                                // Upload page elements
                                "div[data-e2e='text-input']",
                                "button[data-e2e='post-button']"
                            };

                            foreach (var selector in tiktokSuccessIndicators)
                            {
                                try
                                {
                                    var element = await _page.QuerySelectorAsync(selector);
                                    if (element != null && await element.IsVisibleAsync())
                                    {
                                        _logger?.LogInformation($"TikTok success indicator found: {selector}");
                                        return true;
                                    }
                                }
                                catch
                                {
                                    // Continue to next selector
                                }
                            }

                            // If we're on a TikTok page that's not a captcha page, consider it successful
                            if (currentUrl.Contains("tiktok.com") && 
                                !currentUrl.Contains("captcha") && 
                                !currentUrl.Contains("verify") &&
                                !currentUrl.Contains("challenge"))
                            {
                                _logger?.LogInformation("Captcha solution successful - on TikTok page");
                                return true;
                            }
                            
                            // If we can't find specific indicators but captcha is gone, assume success
                            _logger?.LogInformation("Captcha is gone but no specific indicators found - assuming success");
                            return true;
                        }
                        else
                        {
                            _logger?.LogWarning("Captcha reappeared after initial detection of removal");
                        }
                    }

                    // Log progress every 10 checks
                    if (checkCount % 10 == 0)
                    {
                        var elapsed = DateTime.UtcNow - startTime;
                        _logger?.LogInformation($"Still waiting for captcha solution... ({checkCount}/{maxChecks} checks, {elapsed.TotalSeconds:F1}s elapsed, URL: {currentUrl})");
                    }

                    // Wait between checks
                    await Task.Delay(5000);
                }

                var reason = checkCount >= maxChecks ? "maximum check count reached" : "timeout";
                _logger?.LogWarning($"Captcha solution failed: {reason} after {checkCount} checks");
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error waiting for captcha solution");
                return false;
            }
        }

        /// <summary>
        /// Handles captcha during any operation by detecting and waiting for solution
        /// </summary>
        /// <param name="operation">Name of the operation being performed</param>
        /// <param name="timeoutSeconds">Maximum time to wait for captcha solution</param>
        /// <returns>True if operation can continue, false if captcha handling failed</returns>
        private async Task<bool> HandleCaptchaAsync(string operation, int timeoutSeconds = 60)
        {
            try
            {
                // if (await IsCaptchaPresentAsync())
                // {
                //     _logger?.LogInformation($"Captcha detected during {operation}, waiting for solution...");
                //     return await WaitForCaptchaSolutionAsync(timeoutSeconds);
                // }
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error handling captcha during {operation}");
                return false;
            }
        }

        /// <summary>
        /// Simulates human-like typing with random delays between characters
        /// </summary>
        /// <param name="element">The input element to type into</param>
        /// <param name="text">The text to type</param>
        private async Task HumanLikeTypingAsync(IElementHandle element, string text)
        {
            try
            {
                // Clear the field first
                await element.FillAsync("");
                await Task.Delay(Random.Shared.Next(200, 500));

                // Type each character with random delays
                foreach (char c in text)
                {
                    await element.TypeAsync(c.ToString());
                    
                    // Random delay between characters (50-150ms for normal typing, longer for mistakes)
                    var baseDelay = Random.Shared.Next(50, 150);
                    
                    // Occasionally add longer delays to simulate thinking or mistakes
                    if (Random.Shared.Next(1, 20) == 1) // 5% chance
                    {
                        baseDelay += Random.Shared.Next(200, 800);
                    }
                    
                    await Task.Delay(baseDelay);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Error during human-like typing: {ex.Message}");
                // Fallback to normal typing
                await element.FillAsync(text);
            }
        }
    }
} 