using Anubis.Bots.Core.Factories;
using Anubis.Bots.TikTok;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Anubis.Bots.TikTok
{
    public class ChromiumCaptchaTest
    {
        public static async Task TestChromiumWithCaptchaExtension()
        {
            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<ChromiumCaptchaTest>();

            IBrowser? browser = null;
            IPage? page = null;

            try
            {
                logger.LogInformation("=== Testing Chromium with Captcha Extension ===");

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
                        logger.LogInformation("✓ Captcha solver extension found and will be loaded");
                    }
                }

                // Create browser with Chromium and captcha extension
                logger.LogInformation("Creating Chromium browser with captcha extension...");
                browser = await browserFactory.CreateBrowserAsync(
                    headless: false, 
                    extensionPath: extensionPath, // Use the captcha extension
                    browserType: "chromium"
                );
                
                if (browser == null)
                {
                    throw new InvalidOperationException("Failed to create browser");
                }

                // Wait a moment for browser to fully initialize
                await Task.Delay(2000);

                // Create page
                logger.LogInformation("Creating page...");
                page = await browserFactory.CreatePageAsync(browser);
                
                if (page == null)
                {
                    throw new InvalidOperationException("Failed to create page");
                }

                // Navigate to TikTok to test captcha detection
                logger.LogInformation("Navigating to TikTok...");
                await page.GotoAsync("https://www.tiktok.com/");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await Task.Delay(3000);

                logger.LogInformation($"Current URL: {page.Url}");
                logger.LogInformation($"Page title: {await page.TitleAsync()}");

                // Create TikTok driver to test captcha detection
                var tiktokDriver = new TikTokDriver(browser, page, logger);

                // Test captcha detection
                logger.LogInformation("Testing captcha detection...");
                var hasCaptcha = await tiktokDriver.IsCaptchaPresentAsync();
                logger.LogInformation($"Captcha detected: {hasCaptcha}");

                if (hasCaptcha)
                {
                    logger.LogInformation("Captcha found! Testing captcha solution waiting...");
                    var captchaSolved = await tiktokDriver.WaitForCaptchaSolutionAsync(30);
                    logger.LogInformation($"Captcha solution result: {captchaSolved}");
                }

                logger.LogInformation("✓ Chromium with captcha extension test completed successfully!");
                
                // Keep browser open for manual inspection
                logger.LogInformation("Browser will remain open for 30 seconds for manual inspection...");
                await Task.Delay(30000);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Chromium captcha test");
                throw;
            }
            finally
            {
                // Cleanup
                if (page != null)
                {
                    await page.CloseAsync();
                }
                if (browser != null)
                {
                    await browser.CloseAsync();
                }
            }
        }
    }
} 