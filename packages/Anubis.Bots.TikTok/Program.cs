using Anubis.Bots.Core.Factories;
using Anubis.Bots.Core.Managers;
using Anubis.Bots.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace Anubis.Bots.TikTok
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
                logger.LogInformation("=== Anubis TikTok Automation Framework ===");
                
                logger.LogInformation("Choose an option:");
                logger.LogInformation("1. Manual login test (recommended first)");
                logger.LogInformation("2. Test different login approaches");
                logger.LogInformation("3. Cookie-based login (if cookies exist)");
                logger.LogInformation("4. Automated login");
                logger.LogInformation("5. Run all tests");
                logger.LogInformation("6. Show troubleshooting tips");
                logger.LogInformation("7. Diagnose stuck login page");
                logger.LogInformation("8. Test alternative login methods");
                logger.LogInformation("9. Show stuck login page solutions");
                
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await TestTikTokAccess(logger);
                        break;
                    case "2":
                        await TestDifferentLoginApproaches(logger);
                        break;
                    case "3":
                        await TestCookieBasedLogin(logger);
                        break;
                    case "4":
                        await RunWithBrowser();
                        break;
                    case "5":
                        await TestTikTokAccess(logger);
                        await TestDifferentLoginApproaches(logger);
                        await TestCookieBasedLogin(logger);
                        logger.LogInformation("Would you like to try automated login? (y/n)");
                        var response = Console.ReadLine()?.ToLower();
                        if (response == "y" || response == "yes")
                        {
                            await RunWithBrowser();
                        }
                        break;
                    case "6":
                        ShowTroubleshootingTips(logger);
                        break;
                    case "7":
                        await DiagnoseStuckLoginPage(logger);
                        break;
                    case "8":
                        await TestAlternativeLoginMethods(logger);
                        break;
                    case "9":
                        ShowStuckLoginPageSolutions(logger);
                        break;
                    default:
                        logger.LogInformation("Invalid choice, running manual login test...");
                        await TestTikTokAccess(logger);
                        break;
                }
                
                logger.LogInformation("=== Automation completed successfully! ===");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during automation");
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
                    Username = "tiktokuser1@example.com",
                    Password = "password1",
                    Platform = "TikTok",
                    IsActive = true
                },
                new SocialAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "tiktokuser2@example.com",
                    Password = "password2",
                    Platform = "TikTok",
                    IsActive = true
                },
                new SocialAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "tiktokuser3@example.com",
                    Password = "password3",
                    Platform = "TikTok",
                    IsActive = true
                }
            };

            accountManager.AddAccounts(accounts);

            // Save accounts to file
            accountManager.SaveAccountsToFile("tiktok-accounts.json");
            logger.LogInformation("✓ Saved accounts to tiktok-accounts.json");

            // Get available account
            var availableAccount = accountManager.GetAvailableAccount("TikTok");
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
                Hashtags = new List<string> { "automation", "testing", "dotnet", "tiktok" },
                Mentions = new List<string> { "microsoft" },
                DelayBeforePost = TimeSpan.FromSeconds(2),
                DelayAfterPost = TimeSpan.FromSeconds(3)
            };
            logger.LogInformation("✓ PostOptions configured with hashtags and delays");

            // Create sample post
            var samplePost = new Post
            {
                Id = "sample-tiktok-post-123",
                Content = "Testing the Anubis TikTok automation framework! #automation #testing #tiktok",
                AuthorId = "sample-tiktok-user",
                AuthorName = "Sample TikTok User",
                CreatedAt = DateTime.UtcNow,
                LikeCount = 42,
                CommentCount = 5,
                ShareCount = 2,
                Hashtags = new List<string> { "automation", "testing", "tiktok" },
                Platform = "TikTok"
            };
            logger.LogInformation("✓ Sample TikTok post created with engagement metrics");

            logger.LogInformation("Framework components:");
            logger.LogInformation("  - ISocialNetworkDriver: Unified interface for all platforms");
            logger.LogInformation("  - BaseSocialNetworkDriver: Common functionality and anti-detection");
            logger.LogInformation("  - TikTokDriver: Platform-specific implementation");
            logger.LogInformation("  - AccountManager: Multi-account rotation and rate limiting");
            logger.LogInformation("  - BrowserFactory: Anti-detection browser setup");

            await Task.Delay(100);
        }

        static async Task DemoTikTokDriver(ILogger logger)
        {
            logger.LogInformation("=== Demo 3: TikTok Driver Structure ===");

            // Show the TikTok driver capabilities
            logger.LogInformation("TikTokDriver supports:");
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
            logger.LogInformation("  var driver = new TikTokDriver(browser, page, logger);");
            logger.LogInformation("  await driver.LoginAsync(username, password);");
            logger.LogInformation("  await driver.PostAsync(content, postOptions);");
            logger.LogInformation("  var posts = await driver.CollectPostsAsync(\"dotnet\", 10);");

            await Task.Delay(100);
        }

        private static async Task RunWithBrowser()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            IBrowser? browser = null;
            IPage? page = null;

            try
            {
                logger.LogInformation("Starting TikTok automation with Playwright");

                // Create browser factory
                var browserFactory = new BrowserFactory(logger);

                // Get the path to the captcha solver extension
                var currentDirectory = Directory.GetCurrentDirectory();
                var extensionPath = Path.Combine(currentDirectory, "captcha-solver");
                
                logger.LogInformation($"Looking for extension at: {extensionPath}");
                
                if (!Directory.Exists(extensionPath))
                {
                    logger.LogWarning($"Extension directory not found at: {extensionPath}");
                    logger.LogInformation("Continuing without captcha solver extension...");
                    extensionPath = null;
                }
                else
                {
                    var manifestPath = Path.Combine(extensionPath, "manifest.json");
                    if (!File.Exists(manifestPath))
                    {
                        logger.LogWarning($"manifest.json not found in extension directory");
                        logger.LogInformation("Continuing without captcha solver extension...");
                        extensionPath = null;
                    }
                    else
                    {
                        logger.LogInformation("Captcha solver extension found and will be loaded");
                    }
                }

                // Create browser with extension
                logger.LogInformation("Creating browser...");
                
                // Use Chromium explicitly for better captcha extension support
                browser = await browserFactory.CreateBrowserAsync(
                    headless: false, 
                    extensionPath: extensionPath,
                    browserType: "chromium"  // Explicitly use Chromium for captcha solving
                );
                
                if (browser == null)
                {
                    throw new InvalidOperationException("Failed to create browser");
                }

                // Create page with additional anti-detection measures
                logger.LogInformation("Creating page...");
                page = await browserFactory.CreatePageAsync(browser);
                
                if (page == null)
                {
                    throw new InvalidOperationException("Failed to create page");
                }
                
                // Add additional anti-detection measures
                await page.AddInitScriptAsync(@"
                    // Remove webdriver property
                    Object.defineProperty(navigator, 'webdriver', {
                        get: () => undefined,
                    });
                    
                    // Override permissions
                    const originalQuery = window.navigator.permissions.query;
                    window.navigator.permissions.query = (parameters) => (
                        parameters.name === 'notifications' ?
                            Promise.resolve({ state: Notification.permission }) :
                            originalQuery(parameters)
                    );
                    
                    // Override plugins
                    Object.defineProperty(navigator, 'plugins', {
                        get: () => [1, 2, 3, 4, 5],
                    });
                    
                    // Override languages
                    Object.defineProperty(navigator, 'languages', {
                        get: () => ['en-US', 'en'],
                    });
                ");
                
                // Set realistic viewport
                await page.SetViewportSizeAsync(1920, 1080);
                
                // Set realistic user agent
                await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
                {
                    ["Accept-Language"] = "en-US,en;q=0.9",
                    ["Accept-Encoding"] = "gzip, deflate, br",
                    ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    ["DNT"] = "1",
                    ["Upgrade-Insecure-Requests"] = "1"
                });
                
                // Create TikTok driver
                var tiktokDriver = new TikTokDriver(browser, page, logger);

                // Test automated login
                var username = "JacksonGarcia.cothb@outlook.com";
                var password = "@o9x$o#htnr1";

                logger.LogInformation("Attempting automated login to TikTok");
                var loginSuccess = await tiktokDriver.LoginAsync(username, password);

                if (loginSuccess)
                {
                    logger.LogInformation("✓ Successfully logged in to TikTok!");

                    // Test posting
                    var postOptions = new PostOptions
                    {
                        Hashtags = new List<string> { "automation", "testing", "tiktok" },
                        DelayBeforePost = TimeSpan.FromSeconds(2),
                        DelayAfterPost = TimeSpan.FromSeconds(3)
                    };

                    logger.LogInformation("Testing TikTok automation features...");

                    // Test navigating to upload page
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/upload");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to upload page");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to upload page: {Error}", ex.Message);
                    }

                    // Test navigating to hashtag
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/tag/dotnet");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to #dotnet hashtag");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to hashtag: {Error}", ex.Message);
                    }

                    // Test navigating to profile
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/@me");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to profile page");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to profile: {Error}", ex.Message);
                    }

                    // Export cookies for future use
                    var cookies = await tiktokDriver.ExportCookiesAsync();
                    logger.LogInformation("Exported {Count} cookies", cookies.Count);

                    logger.LogInformation("Press any key to close browser...");
                    Console.ReadKey();
                }
                else
                {
                    logger.LogError("Failed to login to TikTok automatically");
                    logger.LogInformation("You can try manual login if needed");
                    logger.LogInformation("Press any key to close browser...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during TikTok automation");
                
                if (browser != null)
                {
                    logger.LogInformation("Browser is still open. Press any key to close...");
                    Console.ReadKey();
                }
            }
            finally
            {
                // Cleanup
                try
                {
                    if (page != null)
                    {
                        await page.CloseAsync();
                        logger?.LogInformation("Page closed successfully");
                    }
                    
                    if (browser != null)
                    {
                        await browser.CloseAsync();
                        logger?.LogInformation("Browser closed successfully");
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Error during cleanup");
                }
            }
        }

        static async Task TestTikTokAccess(ILogger logger)
        {
            logger.LogInformation("Testing TikTok access...");
            
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var page = await browser.NewPageAsync();
                logger.LogInformation("✓ Browser and page ready");
                
                // Navigate to TikTok main page
                logger.LogInformation("Navigating to TikTok...");
                await page.GotoAsync("https://www.tiktok.com/");
                
                try
                {
                    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = 10000 });
                    logger.LogInformation("✓ TikTok page loaded");
                }
                catch (TimeoutException)
                {
                    logger.LogWarning("Page load timed out, but continuing...");
                }
                
                logger.LogInformation($"Page title: {await page.TitleAsync()}");
                logger.LogInformation($"Current URL: {page.Url}");
                
                // Instructions for manual login
                logger.LogInformation("=== TikTok Login Instructions ===");
                logger.LogInformation("1. Look for a 'Log in' or 'Sign in' button on the page");
                logger.LogInformation("2. Click it to open the login form");
                logger.LogInformation("3. Enter your credentials:");
                logger.LogInformation("   Email: JacksonGarcia.cothb@outlook.com");
                logger.LogInformation("   Password: @o9x$o#htnr1");
                logger.LogInformation("4. Complete any 2FA or verification if required");
                logger.LogInformation("5. Once logged in, press any key to continue...");
                
                Console.ReadKey();
                
                // Check if login was successful
                var currentUrl = page.Url;
                logger.LogInformation($"Current URL after login attempt: {currentUrl}");
                
                if (currentUrl.Contains("/foryou") || currentUrl.Contains("/feed"))
                {
                    logger.LogInformation("✓ Successfully logged in to TikTok!");
                    
                    // Export cookies for future use
                    var cookies = await page.Context.CookiesAsync();
                    var cookiesJson = JsonConvert.SerializeObject(cookies, Formatting.Indented);
                    await File.WriteAllTextAsync("tiktok-cookies.json", cookiesJson);
                    logger.LogInformation("✓ Cookies exported to tiktok-cookies.json");
                    
                    // Test some automation features
                    logger.LogInformation("Testing TikTok automation features...");
                    
                    // Try to navigate to profile
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/@me");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to profile page");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to profile: {Error}", ex.Message);
                    }
                    
                    // Try to navigate to upload page
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/upload");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to upload page");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to upload page: {Error}", ex.Message);
                    }
                    
                    // Try to navigate to a hashtag
                    try
                    {
                        await page.GotoAsync("https://www.tiktok.com/tag/dotnet");
                        await Task.Delay(2000);
                        logger.LogInformation("✓ Successfully navigated to #dotnet hashtag");
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Could not navigate to hashtag: {Error}", ex.Message);
                    }
                }
                else if (currentUrl.Contains("/login"))
                {
                    logger.LogInformation("Still on login page - login may not have completed");
                }
                else
                {
                    logger.LogInformation($"On page: {currentUrl}");
                }
                
                logger.LogInformation("Press any key to close browser...");
                Console.ReadKey();
                
                await browser.CloseAsync();
                logger.LogInformation("✓ Browser closed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in TikTok test");
                throw;
            }
        }

        static async Task TestCookieBasedLogin(ILogger logger)
        {
            logger.LogInformation("Testing cookie-based login...");
            
            try
            {
                if (!File.Exists("tiktok-cookies.json"))
                {
                    logger.LogWarning("No cookies file found. Please run manual login first to generate cookies.");
                    return;
                }
                
                var cookiesJson = await File.ReadAllTextAsync("tiktok-cookies.json");
                var cookies = JsonConvert.DeserializeObject<Cookie[]>(cookiesJson);
                
                if (cookies == null || cookies.Length == 0)
                {
                    logger.LogWarning("No cookies found in file");
                    return;
                }
                
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();
                
                // Set cookies before navigating
                await context.AddCookiesAsync(cookies);
                logger.LogInformation($"✓ Loaded {cookies.Length} cookies");
                
                // Navigate to TikTok
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Page title: {await page.TitleAsync()}");
                logger.LogInformation($"Current URL: {page.Url}");
                
                // Check if we're logged in
                if (page.Url.Contains("/foryou") || page.Url.Contains("/feed"))
                {
                    logger.LogInformation("✓ Successfully logged in using cookies!");
                }
                else
                {
                    logger.LogWarning("Cookie-based login failed - cookies may be expired");
                }
                
                logger.LogInformation("Press any key to close browser...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in cookie-based login test");
            }
        }

        static async Task TestDifferentLoginApproaches(ILogger logger)
        {
            logger.LogInformation("=== Testing Different Login Approaches ===");
            
            try
            {
                // Test 1: Direct login URL
                logger.LogInformation("Test 1: Direct login URL approach");
                await TestDirectLoginUrl(logger);
                
                // Test 2: Different browser (Firefox)
                logger.LogInformation("Test 2: Firefox browser approach");
                await TestFirefoxLogin(logger);
                
                // Test 3: Mobile user agent
                logger.LogInformation("Test 3: Mobile user agent approach");
                await TestMobileUserAgent(logger);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing different login approaches");
            }
        }

        static async Task TestDirectLoginUrl(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var page = await browser.NewPageAsync();
                
                // Go directly to login page
                await page.GotoAsync("https://www.tiktok.com/login");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Direct login URL - Page title: {await page.TitleAsync()}");
                logger.LogInformation($"Direct login URL - Current URL: {page.Url}");
                
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-direct-login.png" });
                logger.LogInformation("✓ Screenshot saved: tiktok-direct-login.png");
                
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in direct login URL test");
            }
        }

        static async Task TestFirefoxLogin(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var page = await browser.NewPageAsync();
                
                // Navigate to TikTok
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Firefox - Page title: {await page.TitleAsync()}");
                logger.LogInformation($"Firefox - Current URL: {page.Url}");
                
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-firefox.png" });
                logger.LogInformation("✓ Screenshot saved: tiktok-firefox.png");
                
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Firefox login test");
            }
        }

        static async Task TestMobileUserAgent(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_7_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.2 Mobile/15E148 Safari/604.1",
                    ViewportSize = new ViewportSize { Width = 375, Height = 667 }
                });
                
                var page = await context.NewPageAsync();
                
                // Navigate to TikTok mobile
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Mobile - Page title: {await page.TitleAsync()}");
                logger.LogInformation($"Mobile - Current URL: {page.Url}");
                
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-mobile.png" });
                logger.LogInformation("✓ Screenshot saved: tiktok-mobile.png");
                
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in mobile user agent test");
            }
        }

        static async Task DiagnoseStuckLoginPage(ILogger logger)
        {
            logger.LogInformation("=== Diagnosing Stuck Login Page ===");
            
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var page = await browser.NewPageAsync();
                
                // Navigate to TikTok
                logger.LogInformation("Navigating to TikTok...");
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Initial page title: {await page.TitleAsync()}");
                logger.LogInformation($"Initial URL: {page.Url}");
                
                // Take screenshot of initial state
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-initial.png" });
                logger.LogInformation("✓ Initial screenshot saved: tiktok-initial.png");
                
                // Look for login button
                logger.LogInformation("Looking for login button...");
                var loginSelectors = new[]
                {
                    "button#top-right-action-bar-login-button",
                    "a[href*='login']",
                    "button:has-text('Log in')",
                    "button:has-text('Sign in')",
                    "[data-e2e='login-button']",
                    ".login-button",
                    ".signin-button"
                };
                
                IElementHandle? loginButton = null;
                foreach (var selector in loginSelectors)
                {
                    try
                    {
                        var elements = await page.QuerySelectorAllAsync(selector);
                        logger.LogInformation($"Selector '{selector}' found {elements.Count} elements");
                        
                        if (elements.Count > 0)
                        {
                            loginButton = elements[0];
                            logger.LogInformation($"✓ Found login button with selector: {selector}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Error with selector '{selector}': {ex.Message}");
                    }
                }
                
                if (loginButton != null)
                {
                    // Click login button
                    logger.LogInformation("Clicking login button...");
                    await loginButton.ClickAsync();
                    await Task.Delay(3000);
                    
                    logger.LogInformation($"After login click - URL: {page.Url}");
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-after-login-click.png" });
                    logger.LogInformation("✓ Screenshot saved: tiktok-after-login-click.png");
                    
                    // Check for various states
                    var pageContent = await page.ContentAsync();
                    
                    if (pageContent.Contains("Something went wrong"))
                    {
                        logger.LogWarning("❌ 'Something went wrong' error detected");
                        logger.LogInformation("This usually means:");
                        logger.LogInformation("- IP address is flagged");
                        logger.LogInformation("- Account is temporarily blocked");
                        logger.LogInformation("- Too many login attempts");
                    }
                    else if (pageContent.Contains("captcha") || pageContent.Contains("Captcha"))
                    {
                        logger.LogWarning("❌ Captcha detected");
                        logger.LogInformation("TikTok is requiring human verification");
                    }
                    else if (page.Url.Contains("/login"))
                    {
                        logger.LogInformation("✓ Login page loaded successfully");
                        
                        // Look for login form elements
                        var formElements = await page.QuerySelectorAllAsync("input[type='email'], input[type='text'], input[type='password']");
                        logger.LogInformation($"Found {formElements.Count} form input elements");
                        
                        if (formElements.Count > 0)
                        {
                            logger.LogInformation("✓ Login form is present and ready");
                            logger.LogInformation("The page is not stuck - it's waiting for input");
                        }
                        else
                        {
                            logger.LogWarning("❌ No login form elements found");
                        }
                    }
                    else
                    {
                        logger.LogInformation($"Current state: {page.Url}");
                    }
                }
                else
                {
                    logger.LogWarning("❌ No login button found");
                    logger.LogInformation("This could mean:");
                    logger.LogInformation("- You're already logged in");
                    logger.LogInformation("- TikTok changed their interface");
                    logger.LogInformation("- Page didn't load properly");
                    
                    // Check if already logged in
                    if (page.Url.Contains("/foryou") || page.Url.Contains("/feed"))
                    {
                        logger.LogInformation("✓ Already logged in!");
                    }
                }
                
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error diagnosing stuck login page");
            }
        }

        static async Task TestAlternativeLoginMethods(ILogger logger)
        {
            logger.LogInformation("=== Testing Alternative Login Methods ===");
            
            try
            {
                // Method 1: Try different TikTok domains
                logger.LogInformation("Method 1: Testing different TikTok domains");
                await TestDifferentDomains(logger);
                
                // Method 2: Try with different user agent
                logger.LogInformation("Method 2: Testing with different user agent");
                await TestWithDifferentUserAgent(logger);
                
                // Method 3: Try with cleared cookies
                logger.LogInformation("Method 3: Testing with cleared cookies");
                await TestWithClearedCookies(logger);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing alternative login methods");
            }
        }

        static async Task TestDifferentDomains(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var domains = new[]
                {
                    "https://www.tiktok.com/",
                    "https://tiktok.com/",
                    "https://www.tiktok.com/login",
                    "https://www.tiktok.com/login/phone-or-email/email"
                };
                
                foreach (var domain in domains)
                {
                    try
                    {
                        var page = await browser.NewPageAsync();
                        logger.LogInformation($"Testing domain: {domain}");
                        
                        await page.GotoAsync(domain);
                        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                        
                        logger.LogInformation($"  Title: {await page.TitleAsync()}");
                        logger.LogInformation($"  URL: {page.Url}");
                        
                        await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"tiktok-{domain.Replace("https://", "").Replace("/", "-")}.png" });
                        
                        await page.CloseAsync();
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Error testing {domain}: {ex.Message}");
                    }
                }
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing different domains");
            }
        }

        static async Task TestWithDifferentUserAgent(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var userAgents = new[]
                {
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1.2 Mobile/15E148 Safari/604.1"
                };
                
                foreach (var userAgent in userAgents)
                {
                    try
                    {
                        var context = await browser.NewContextAsync(new BrowserNewContextOptions
                        {
                            UserAgent = userAgent
                        });
                        
                        var page = await context.NewPageAsync();
                        logger.LogInformation($"Testing user agent: {userAgent.Substring(0, 50)}...");
                        
                        await page.GotoAsync("https://www.tiktok.com/");
                        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                        
                        logger.LogInformation($"  Title: {await page.TitleAsync()}");
                        logger.LogInformation($"  URL: {page.Url}");
                        
                        await page.ScreenshotAsync(new PageScreenshotOptions { Path = $"tiktok-ua-{userAgent.GetHashCode()}.png" });
                        
                        await context.CloseAsync();
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Error testing user agent: {ex.Message}");
                    }
                }
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing with different user agent");
            }
        }

        static async Task TestWithClearedCookies(ILogger logger)
        {
            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                });
                
                var context = await browser.NewContextAsync();
                var page = await context.NewPageAsync();
                
                logger.LogInformation("Testing with fresh browser session (no cookies)");
                
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                
                logger.LogInformation($"Title: {await page.TitleAsync()}");
                logger.LogInformation($"URL: {page.Url}");
                
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = "tiktok-fresh-session.png" });
                
                logger.LogInformation("Press any key to continue...");
                Console.ReadKey();
                
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing with cleared cookies");
            }
        }

        static void ShowTroubleshootingTips(ILogger logger)
        {
            logger.LogInformation("=== TikTok Login Troubleshooting Tips ===");
            logger.LogInformation("");
            logger.LogInformation("Common Issues and Solutions:");
            logger.LogInformation("");
            logger.LogInformation("1. 'Something went wrong' error:");
            logger.LogInformation("   - Your IP address may be flagged");
            logger.LogInformation("   - Try using a VPN or different network");
            logger.LogInformation("   - Wait 24-48 hours before trying again");
            logger.LogInformation("");
            logger.LogInformation("2. Account temporarily blocked:");
            logger.LogInformation("   - Too many failed login attempts");
            logger.LogInformation("   - Try a different account");
            logger.LogInformation("   - Wait 24-48 hours");
            logger.LogInformation("");
            logger.LogInformation("3. Captcha keeps appearing:");
            logger.LogInformation("   - TikTok detected automated behavior");
            logger.LogInformation("   - Try manual login first");
            logger.LogInformation("   - Use a different browser (Firefox)");
            logger.LogInformation("");
            logger.LogInformation("4. Login form not found:");
            logger.LogInformation("   - TikTok may have changed their interface");
            logger.LogInformation("   - Try direct URL: https://www.tiktok.com/login");
            logger.LogInformation("   - Try mobile user agent");
            logger.LogInformation("");
            logger.LogInformation("5. 2FA/Verification required:");
            logger.LogInformation("   - Complete verification manually first");
            logger.LogInformation("   - Save cookies after successful login");
            logger.LogInformation("   - Use cookie-based login for automation");
            logger.LogInformation("");
            logger.LogInformation("6. Browser detection:");
            logger.LogInformation("   - Try Firefox instead of Chromium");
            logger.LogInformation("   - Use mobile user agent");
            logger.LogInformation("   - Disable browser automation flags");
            logger.LogInformation("");
            logger.LogInformation("Recommended approach:");
            logger.LogInformation("1. Try manual login first (option 1)");
            logger.LogInformation("2. If successful, save cookies");
            logger.LogInformation("3. Use cookie-based login for automation");
            logger.LogInformation("4. If manual login fails, try different approaches");
            logger.LogInformation("");
        }

        static void ShowStuckLoginPageSolutions(ILogger logger)
        {
            logger.LogInformation("=== Solutions for Stuck Login Page ===");
            logger.LogInformation("");
            logger.LogInformation("If the page is stuck on login, try these solutions:");
            logger.LogInformation("");
            logger.LogInformation("1. IMMEDIATE SOLUTIONS:");
            logger.LogInformation("   - Clear browser cache and cookies");
            logger.LogInformation("   - Try a different browser (Firefox, Safari)");
            logger.LogInformation("   - Use incognito/private browsing mode");
            logger.LogInformation("   - Try from a different network (mobile hotspot)");
            logger.LogInformation("");
            logger.LogInformation("2. WAIT AND RETRY:");
            logger.LogInformation("   - Wait 24-48 hours before trying again");
            logger.LogInformation("   - TikTok may have temporarily blocked your IP");
            logger.LogInformation("   - Try at different times of day");
            logger.LogInformation("");
            logger.LogInformation("3. USE VPN/PROXY:");
            logger.LogInformation("   - Connect to a VPN service");
            logger.LogInformation("   - Try different server locations");
            logger.LogInformation("   - Use a residential proxy service");
            logger.LogInformation("");
            logger.LogInformation("4. ALTERNATIVE APPROACHES:");
            logger.LogInformation("   - Try TikTok mobile app instead");
            logger.LogInformation("   - Use a different TikTok account");
            logger.LogInformation("   - Contact TikTok support if account is blocked");
            logger.LogInformation("");
            logger.LogInformation("5. TECHNICAL SOLUTIONS:");
            logger.LogInformation("   - Disable browser extensions");
            logger.LogInformation("   - Try mobile user agent");
            logger.LogInformation("   - Use a different device");
            logger.LogInformation("");
            logger.LogInformation("6. MANUAL WORKAROUND:");
            logger.LogInformation("   - Complete login manually in regular browser");
            logger.LogInformation("   - Export cookies after successful login");
            logger.LogInformation("   - Use cookie-based automation");
            logger.LogInformation("");
        }
    }
} 