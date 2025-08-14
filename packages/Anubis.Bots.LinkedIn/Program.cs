using Anubis.Bots.Core.Factories;
using Anubis.Bots.Core.Managers;
using Anubis.Bots.Core.Models;
using Anubis.Bots.LinkedIn;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Anubis.Bots.LinkedIn
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("=== Anubis Social Media Automation Framework ===");
                
                // Simple LinkedIn test
                await TestLinkedInAccess(logger);
                
                logger.LogInformation("=== Test completed! ===");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during test");
                throw;
            }
        }

        static async Task TestPlaywrightBasic(ILogger logger)
        {
            logger.LogInformation("Testing basic Playwright functionality...");
            
            try
            {
                var playwright = await Playwright.CreateAsync();
                logger.LogInformation("✓ Playwright created successfully");
                
                var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                logger.LogInformation("✓ Firefox browser launched successfully");
                
                var page = await browser.NewPageAsync();
                logger.LogInformation("✓ Page created successfully");
                
                // Create a simple local HTML page
                await page.SetContentAsync(@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <title>Playwright Test</title>
                    </head>
                    <body>
                        <h1>Playwright is working!</h1>
                        <p>If you can see this, Playwright is successfully running.</p>
                        <p>Time: " + DateTime.Now.ToString() + @"</p>
                    </body>
                    </html>
                ");
                logger.LogInformation("✓ Set page content successfully");
                
                await Task.Delay(5000); // Wait 5 seconds to see the page
                
                await browser.CloseAsync();
                logger.LogInformation("✓ Browser closed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in basic Playwright test");
                throw;
            }
        }

        static async Task DemoAccountManager()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("=== Demo 1: Account Management ===");

            // Create account manager
            var accountManager = new AccountManager(logger);

            // Add some sample accounts
            var accounts = new List<SocialAccount>
            {
                new SocialAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "account1@example.com",
                    Password = "password1",
                    Platform = "LinkedIn",
                    IsActive = true
                },
                new SocialAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "account2@example.com",
                    Password = "password2",
                    Platform = "LinkedIn",
                    IsActive = true
                },
                new SocialAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "account3@example.com",
                    Password = "password3",
                    Platform = "Facebook",
                    IsActive = true
                }
            };

            accountManager.AddAccounts(accounts);

            // Save accounts to file
            accountManager.SaveAccountsToFile("accounts.json");
            logger.LogInformation("✓ Saved accounts to accounts.json");

            // Get available account
            var availableAccount = accountManager.GetAvailableAccount("LinkedIn");
            if (availableAccount != null)
            {
                logger.LogInformation("✓ Selected account: {Username}", availableAccount.Username);
            }

            // Get account stats
            var stats = accountManager.GetAccountStats();
            logger.LogInformation("✓ Account stats: {TotalAccounts} total, {ActiveAccounts} active", 
                stats["TotalAccounts"], stats["ActiveAccounts"]);

            await Task.Delay(100);
        }

        static async Task DemoFrameworkStructure()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("=== Demo 2: Framework Structure ===");

            // Create browser factory (without launching browser)
            var browserFactory = new BrowserFactory(logger);
            logger.LogInformation("✓ BrowserFactory created with anti-detection features");

            // Create sample post options
            var postOptions = new PostOptions
            {
                Hashtags = new List<string> { "automation", "testing", "dotnet" },
                Mentions = new List<string> { "microsoft" },
                DelayBeforePost = TimeSpan.FromSeconds(2),
                DelayAfterPost = TimeSpan.FromSeconds(3)
            };
            logger.LogInformation("✓ PostOptions configured with hashtags and delays");

            // Create sample post
            var samplePost = new Post
            {
                Id = "sample-post-123",
                Content = "Testing the Anubis automation framework! #automation #testing",
                AuthorId = "sample-user",
                AuthorName = "Sample User",
                CreatedAt = DateTime.UtcNow,
                LikeCount = 42,
                CommentCount = 5,
                ShareCount = 2,
                Hashtags = new List<string> { "automation", "testing" },
                Platform = "LinkedIn"
            };
            logger.LogInformation("✓ Sample post created with engagement metrics");

            logger.LogInformation("Framework components:");
            logger.LogInformation("  - ISocialNetworkDriver: Unified interface for all platforms");
            logger.LogInformation("  - BaseSocialNetworkDriver: Common functionality and anti-detection");
            logger.LogInformation("  - LinkedInDriver: Platform-specific implementation");
            logger.LogInformation("  - AccountManager: Multi-account rotation and rate limiting");
            logger.LogInformation("  - BrowserFactory: Anti-detection browser setup");

            await Task.Delay(100);
        }

        static async Task DemoLinkedInDriver(ILogger logger)
        {
            logger.LogInformation("=== Demo 3: LinkedIn Driver Structure ===");

            // Show the LinkedIn driver capabilities
            logger.LogInformation("LinkedInDriver supports:");
            logger.LogInformation("  ✓ Login/Logout with username/password or cookies");
            logger.LogInformation("  ✓ Post creation with hashtags and mentions");
            logger.LogInformation("  ✓ Like/Unlike posts");
            logger.LogInformation("  ✓ Follow/Unfollow users");
            logger.LogInformation("  ✓ Send direct messages");
            logger.LogInformation("  ✓ Collect posts by hashtag, user, or feed");
            logger.LogInformation("  ✓ Human-like browsing behavior");
            logger.LogInformation("  ✓ Cookie management for session persistence");
            logger.LogInformation("  ✓ Anti-detection with stealth mode");

            // Show example usage
            logger.LogInformation("Example usage:");
            logger.LogInformation("  var driver = new LinkedInDriver(browser, page, logger);");
            logger.LogInformation("  await driver.LoginAsync(username, password);");
            logger.LogInformation("  await driver.PostAsync(content, postOptions);");
            logger.LogInformation("  var posts = await driver.CollectPostsAsync(\"dotnet\", 10);");

            await Task.Delay(100);
        }

        static async Task RunWithBrowser()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("Starting LinkedIn automation with Playwright");

                // Create browser factory
                var browserFactory = new BrowserFactory(logger);

                // Create browser and page
                var browser = await browserFactory.CreateBrowserAsync(headless: false);
                var page = await browserFactory.CreatePageAsync(browser);

                // Create LinkedIn driver
                var linkedInDriver = new LinkedInDriver(browser, page, logger);

                // Test login
                var username = "assaf.atias@gmail.com"; // Replace with your credentials
                var password = "Assaf@3490";

                logger.LogInformation("Attempting to login to LinkedIn");
                var loginSuccess = await linkedInDriver.LoginAsync(username, password);

                if (loginSuccess)
                {
                    logger.LogInformation("Successfully logged in to LinkedIn");

                    // Test posting
                    var postOptions = new PostOptions
                    {
                        Hashtags = new List<string> { "automation", "testing" },
                        DelayBeforePost = TimeSpan.FromSeconds(2),
                        DelayAfterPost = TimeSpan.FromSeconds(3)
                    };

                    var postSuccess = await linkedInDriver.PostAsync("Testing the new Playwright-based automation framework! #automation #testing", postOptions);
                    
                    if (postSuccess)
                    {
                        logger.LogInformation("Successfully posted to LinkedIn");
                    }

                    // Test browsing
                    await linkedInDriver.BrowseAsync(TimeSpan.FromMinutes(2));

                    // Test collecting posts
                    var posts = await linkedInDriver.CollectPostsAsync("dotnet", 10);
                    logger.LogInformation("Collected {Count} posts with #dotnet hashtag", posts.Count);

                    // Export cookies for future use
                    var cookies = await linkedInDriver.ExportCookiesAsync();
                    logger.LogInformation("Exported {Count} cookies", cookies.Count);

                    // Test logout
                    await linkedInDriver.LogoutAsync();
                    logger.LogInformation("Successfully logged out from LinkedIn");
                }
                else
                {
                    logger.LogError("Failed to login to LinkedIn");
                }

                // Cleanup
                await browser.CloseAsync();
                logger.LogInformation("Browser closed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during LinkedIn automation");
                throw;
            }
        }

        static async Task TestLinkedInAccess(ILogger logger)
        {
            logger.LogInformation("Testing LinkedIn access...");
            
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var page = await browser.NewPageAsync();
                logger.LogInformation("✓ Browser and page ready");
                
                // Navigate to LinkedIn
                logger.LogInformation("Navigating to LinkedIn...");
                await page.GotoAsync("https://www.linkedin.com/login");
                
                try
                {
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 10000 });
                    logger.LogInformation("✓ LinkedIn login page loaded");
                }
                catch (TimeoutException)
                {
                    logger.LogWarning("Page load timed out, but continuing...");
                }
                
                // Wait for manual interaction
                logger.LogInformation("LinkedIn login page is open. You can manually login if needed.");
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                // Check current URL
                var currentUrl = page.Url;
                logger.LogInformation($"Current URL: {currentUrl}");
                
                if (currentUrl.Contains("/feed/"))
                {
                    logger.LogInformation("✓ Successfully logged in to LinkedIn!");
                }
                else if (currentUrl.Contains("/login"))
                {
                    logger.LogInformation("Still on login page");
                }
                else
                {
                    logger.LogInformation($"On page: {currentUrl}");
                }
                
                await Task.Delay(3000);
                await browser.CloseAsync();
                logger.LogInformation("✓ Browser closed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LinkedIn test");
                throw;
            }
        }
    }
} 