using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Anubis.Bots.Core.Factories
{
    public class BrowserFactory
    {
        private readonly ILogger? _logger;

        public BrowserFactory(ILogger? logger = null)
        {
            _logger = logger;
        }

        public async Task<IBrowser> CreateBrowserAsync(bool headless = false, string? proxy = null, string? extensionPath = null, string browserType = "chromium")
        {
            try
            {
                switch (browserType.ToLower())
                {
                    case "chromium":
                        return await CreateChromiumBrowserAsync(headless, proxy, extensionPath);
                    case "firefox":
                        return await CreateFirefoxBrowserAsync(headless, proxy, extensionPath);
                    case "webkit":
                        return await CreateWebkitBrowserAsync(headless, proxy, extensionPath);
                    default:
                        // Default to Chromium for better extension support
                        return await CreateChromiumBrowserAsync(headless, proxy, extensionPath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Primary browser failed: {ex.Message}");
                
                // Fallback to Chromium if primary browser fails
                if (browserType.ToLower() != "chromium")
                {
                    _logger?.LogInformation("Trying Chromium as fallback...");
                    return await CreateChromiumBrowserAsync(headless, proxy, extensionPath);
                }
                
                throw;
            }
        }

        private async Task<IBrowser> CreateFirefoxBrowserAsync(bool headless = false, string? proxy = null, string? extensionPath = null)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = playwright.Firefox;

            var options = new BrowserTypeLaunchOptions
            {
                Headless = headless
            };

            if (!string.IsNullOrEmpty(proxy))
            {
                options.Proxy = new Proxy
                {
                    Server = proxy
                };
            }

            // Note: Firefox has limited extension support compared to Chromium
            if (!string.IsNullOrEmpty(extensionPath))
            {
                _logger?.LogWarning("Extension loading is not supported with Firefox, continuing without extension...");
            }

            _logger?.LogInformation("Launching Firefox browser...");
            var browserInstance = await browser.LaunchAsync(options);
            
            if (browserInstance == null)
            {
                throw new InvalidOperationException("Failed to create Firefox browser instance");
            }
            
            _logger?.LogInformation("Firefox browser launched successfully");
            return browserInstance;
        }

        private async Task<IBrowser> CreateChromiumBrowserAsync(bool headless = false, string? proxy = null, string? extensionPath = null)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = playwright.Chromium;

            var options = new BrowserTypeLaunchOptions
            {
                Headless = headless
            };

            // Try to use system Chrome if available
            var systemChromePaths = new[]
            {
                "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                "/Applications/Chromium.app/Contents/MacOS/Chromium",
                "/usr/bin/google-chrome",
                "/usr/bin/chromium-browser"
            };

            var chromePath = systemChromePaths.FirstOrDefault(path => File.Exists(path));
            if (!string.IsNullOrEmpty(chromePath))
            {
                options.ExecutablePath = chromePath;
                _logger?.LogInformation($"Using system Chrome: {chromePath}");
            }
            else
            {
                _logger?.LogInformation("Using Playwright's bundled Chromium");
            }

            // Enhanced arguments for better browser configuration
            var args = new List<string>
            {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-gpu",
                "--disable-webgl",
                "--start-maximized",
                "--disable-infobars",
                "--disable-dev-shm-usage",
                "--disable-browser-side-navigation",
                "--disable-features=VizDisplayCompositor",
                "--disable-site-isolation-trials",
                "--disable-notifications"
            };

            if (!string.IsNullOrEmpty(proxy))
            {
                options.Proxy = new Proxy
                {
                    Server = proxy
                };
            }

            // Load extension if provided - try with more conservative approach
            if (!string.IsNullOrEmpty(extensionPath))
            {
                if (!Directory.Exists(extensionPath))
                {
                    _logger?.LogWarning($"Extension path does not exist: {extensionPath}");
                }
                else
                {
                    var manifestPath = Path.Combine(extensionPath, "manifest.json");
                    if (!File.Exists(manifestPath))
                    {
                        _logger?.LogWarning($"manifest.json not found in extension path: {extensionPath}");
                    }
                    else
                    {
                        try
                        {
                            args.Add($"--load-extension={extensionPath}");
                            _logger?.LogInformation($"Loading extension from: {extensionPath}");
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning($"Failed to add extension argument: {ex.Message}");
                        }
                    }
                }
            }

            options.Args = args.ToArray();

            _logger?.LogInformation($"Launching Chromium browser with {args.Count} arguments");
            
            try
            {
                var browserInstance = await browser.LaunchAsync(options);
                
                if (browserInstance == null)
                {
                    throw new InvalidOperationException("Failed to create Chromium browser instance");
                }
                
                _logger?.LogInformation("Chromium browser launched successfully");
                return browserInstance;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to launch Chromium with extension: {ex.Message}");
                
                // Try without extension as fallback
                if (!string.IsNullOrEmpty(extensionPath))
                {
                    _logger?.LogInformation("Retrying without extension...");
                    options.Args = new[] { 
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-gpu",
                        "--disable-webgl",
                        "--start-maximized",
                        "--disable-infobars",
                        "--disable-dev-shm-usage",
                        "--disable-browser-side-navigation",
                        "--disable-features=VizDisplayCompositor",
                        "--disable-site-isolation-trials",
                        "--disable-notifications"
                    };
                    var browserInstance = await browser.LaunchAsync(options);
                    
                    if (browserInstance == null)
                    {
                        throw new InvalidOperationException("Failed to create Chromium browser instance even without extension");
                    }
                    
                    _logger?.LogInformation("Chromium browser launched successfully without extension");
                    return browserInstance;
                }
                
                throw;
            }
        }

        private async Task<IBrowser> CreateWebkitBrowserAsync(bool headless = false, string? proxy = null, string? extensionPath = null)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = playwright.Webkit;

            var options = new BrowserTypeLaunchOptions
            {
                Headless = headless
            };

            if (!string.IsNullOrEmpty(proxy))
            {
                options.Proxy = new Proxy
                {
                    Server = proxy
                };
            }

            // Note: Webkit has very limited extension support
            if (!string.IsNullOrEmpty(extensionPath))
            {
                _logger?.LogWarning("Extension loading is not supported with Webkit, continuing without extension...");
            }

            _logger?.LogInformation("Launching Webkit browser...");
            var browserInstance = await browser.LaunchAsync(options);
            
            if (browserInstance == null)
            {
                throw new InvalidOperationException("Failed to create Webkit browser instance");
            }
            
            _logger?.LogInformation("Webkit browser launched successfully");
            return browserInstance;
        }

        public async Task<IPage> CreatePageAsync(IBrowser browser, string? userAgent = null)
        {
            try
            {
                // Check if browser is valid
                if (browser == null)
                {
                    throw new ArgumentNullException(nameof(browser), "Browser cannot be null");
                }

                _logger?.LogInformation("Creating new page...");
                var page = await browser.NewPageAsync();
                
                // Verify page was created successfully
                if (page == null)
                {
                    throw new InvalidOperationException("Failed to create page");
                }
                
                _logger?.LogInformation("Page created, setting up...");
                
                // Set user agent if provided
                if (!string.IsNullOrEmpty(userAgent))
                {
                    await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
                    {
                        ["User-Agent"] = userAgent
                    });
                }
                else
                {
                    // Use random user agent
                    await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
                    {
                        ["User-Agent"] = GetRandomUserAgent()
                    });
                }

                // Inject stealth script
                await page.AddInitScriptAsync(GetStealthScript());
                
                _logger?.LogInformation("Page created successfully");
                return page;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating page");
                throw;
            }
        }

        private string[] GetAntiDetectionArgs()
        {
            return new[]
            {
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--no-first-run",
                "--disable-blink-features=AutomationControlled",
                "--disable-web-security",
                "--disable-features=VizDisplayCompositor",
                "--disable-plugins-discovery",
                "--disable-default-apps"
            };
        }

        private string GetRandomUserAgent()
        {
            var userAgents = new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15",
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            };

            return userAgents[new Random().Next(userAgents.Length)];
        }

        private string GetStealthScript()
        {
            return @"
                // Remove webdriver property
                Object.defineProperty(navigator, 'webdriver', {
                    get: () => undefined,
                });

                // Override plugins
                Object.defineProperty(navigator, 'plugins', {
                    get: () => [1, 2, 3, 4, 5],
                });

                // Override languages
                Object.defineProperty(navigator, 'languages', {
                    get: () => ['en-US', 'en'],
                });

                // Override permissions
                const originalQuery = window.navigator.permissions.query;
                window.navigator.permissions.query = (parameters) => (
                    parameters.name === 'notifications' ?
                        Promise.resolve({ state: Notification.permission }) :
                        originalQuery(parameters)
                );

                // Override chrome
                Object.defineProperty(window, 'chrome', {
                    writable: true,
                    enumerable: true,
                    configurable: true,
                    value: {
                        runtime: {},
                    },
                });

                // Override permissions
                const originalQuery = window.navigator.permissions.query;
                window.navigator.permissions.query = (parameters) => (
                    parameters.name === 'notifications' ?
                        Promise.resolve({ state: Notification.permission }) :
                        originalQuery(parameters)
                );
            ";
        }
    }
} 