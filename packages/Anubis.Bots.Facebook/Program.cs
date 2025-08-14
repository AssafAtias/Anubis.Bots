using Anubis.Bots.Core.Factories;
using Anubis.Bots.Facebook;
using Microsoft.Extensions.Logging;

namespace Anubis.Bots.Facebook
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
                logger.LogInformation("=== Facebook Driver Demo ===");
                logger.LogInformation("Facebook driver created successfully!");
                logger.LogInformation("To use with browser automation:");
                logger.LogInformation("1. Install Playwright browsers");
                logger.LogInformation("2. Create browser and page");
                logger.LogInformation("3. var facebookDriver = new FacebookDriver(browser, page, logger);");
                logger.LogInformation("4. await facebookDriver.LoginAsync(username, password);");
                
                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Facebook demo");
                throw;
            }
        }
    }
} 