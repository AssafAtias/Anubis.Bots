# Anubis Social Media Automation Framework

A scalable, multi-platform social media automation framework built with .NET 8 and Playwright. Supports LinkedIn, Facebook, Instagram, TikTok, and more.

## Features

- **Multi-Platform Support**: Unified interface for LinkedIn, Facebook, Instagram, TikTok
- **Anti-Detection**: Built-in stealth mode with human-like behavior simulation
- **Account Management**: Rotate between multiple accounts with rate limiting
- **Session Management**: Save/restore login sessions using cookies
- **Data Collection**: Extract structured data from posts and profiles
- **Rate Limiting**: Prevent detection with configurable delays and action limits
- **Proxy Support**: Route traffic through different IP addresses
- **Logging**: Comprehensive logging with Microsoft.Extensions.Logging

## Architecture

```
Anubis.Bots.Core/           # Core framework
├── Interfaces/             # Common interfaces
├── Models/                 # Data models
├── Base/                   # Base classes
├── Factories/              # Browser and driver factories
└── Managers/               # Account and session managers

Anubis.Bots.LinkedIn/       # LinkedIn-specific implementation
├── LinkedInDriver.cs       # LinkedIn automation driver
└── Program.cs             # Example usage

Anubis.Bots.Facebook/       # Facebook implementation
├── FacebookDriver.cs       # Facebook automation driver
└── Program.cs             # Example usage

Anubis.Bots.TikTok/         # TikTok implementation
├── TikTokDriver.cs         # TikTok automation driver
└── Program.cs             # Example usage

Anubis.Bots.Instagram/      # Instagram implementation (future)
```

## Quick Start

### 1. Install Dependencies

```bash
# Install Playwright browsers
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### 2. Basic Usage

```csharp
using Anubis.Bots.Core.Factories;
using Anubis.Bots.LinkedIn;
using Microsoft.Extensions.Logging;

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Create browser and driver
var browserFactory = new BrowserFactory(logger);
var browser = await browserFactory.CreateBrowserAsync(headless: false);
var page = await browserFactory.CreatePageAsync(browser);

var linkedInDriver = new LinkedInDriver(browser, page, logger);

// Login
var success = await linkedInDriver.LoginAsync("username", "password");

if (success)
{
    // Post content
    var postOptions = new PostOptions
    {
        Hashtags = new List<string> { "automation", "testing" }
    };
    await linkedInDriver.PostAsync("Hello from Anubis!", postOptions);
    
    // Collect posts
    var posts = await linkedInDriver.CollectPostsAsync("dotnet", 10);
    
    // Browse like a human
    await linkedInDriver.BrowseAsync(TimeSpan.FromMinutes(5));
}
```

### 3. Account Management

```csharp
var accountManager = new AccountManager(logger);

// Add accounts
var account = new SocialAccount
{
    Id = Guid.NewGuid().ToString(),
    Username = "your-email@example.com",
    Password = "your-password",
    Platform = "LinkedIn",
    IsActive = true
};

accountManager.AddAccount(account);

// Get available account
var availableAccount = accountManager.GetAvailableAccount("LinkedIn", maxDailyActions: 50);
```

### TikTok Example

```csharp
using Anubis.Bots.Core.Factories;
using Anubis.Bots.TikTok;
using Microsoft.Extensions.Logging;

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

// Create browser and driver
var browserFactory = new BrowserFactory(logger);
var browser = await browserFactory.CreateBrowserAsync(headless: false);
var page = await browserFactory.CreatePageAsync(browser);

var tiktokDriver = new TikTokDriver(browser, page, logger);

// Login
var success = await tiktokDriver.LoginAsync("username", "password");

if (success)
{
    // Post content
    var postOptions = new PostOptions
    {
        Hashtags = new List<string> { "automation", "testing", "tiktok" }
    };
    await tiktokDriver.PostAsync("Hello from Anubis TikTok bot!", postOptions);
    
    // Collect posts by hashtag
    var posts = await tiktokDriver.CollectPostsAsync("dotnet", 10);
    
    // Browse like a human
    await tiktokDriver.BrowseAsync(TimeSpan.FromMinutes(5));
}
```

## Supported Actions

### Core Actions
- **Login/Logout**: Secure authentication with cookie support
- **Post**: Create posts with hashtags, mentions, and media
- **Like/Unlike**: Interact with posts
- **Follow/Unfollow**: Manage connections
- **Send Message**: Direct messaging
- **Browse**: Human-like browsing behavior

### Data Collection
- **Collect Posts**: Extract posts by hashtag, user, or feed
- **Profile Information**: Gather user data
- **Engagement Metrics**: Like counts, comments, shares

## Anti-Detection Features

- **Stealth Mode**: Removes automation indicators
- **Human Behavior**: Random mouse movements, scrolling, delays
- **User Agent Rotation**: Multiple realistic user agents
- **Cookie Management**: Session persistence
- **Rate Limiting**: Configurable action delays
- **Proxy Support**: IP rotation

## Configuration

### Browser Options
```csharp
var browser = await browserFactory.CreateBrowserAsync(
    browserType: BrowserType.Chromium,
    headless: false,
    proxy: "http://proxy-server:port"
);
```

### Account Limits
```csharp
var account = accountManager.GetAvailableAccount(
    platform: "LinkedIn",
    maxDailyActions: 50,
    cooldownPeriod: TimeSpan.FromMinutes(30)
);
```

### Post Options
```csharp
var postOptions = new PostOptions
{
    Hashtags = new List<string> { "automation", "testing" },
    Mentions = new List<string> { "username" },
    ImagePath = "path/to/image.jpg",
    DelayBeforePost = TimeSpan.FromSeconds(2),
    DelayAfterPost = TimeSpan.FromSeconds(3)
};
```

## Extending for New Platforms

To add support for a new platform (e.g., Facebook):

1. **Create Platform Project**
```bash
mkdir packages/Anubis.Bots.Facebook
```

2. **Implement Driver**
```csharp
public class FacebookDriver : BaseSocialNetworkDriver
{
    public override string Platform => "Facebook";
    
    protected override async Task<bool> PerformLoginAsync(string username, string password)
    {
        // Facebook-specific login logic
    }
    
    // Implement other abstract methods...
}
```

3. **Add to Solution**
```xml
<ProjectReference Include="../Anubis.Bots.Core/Anubis.Bots.Core.csproj" />
```

## Best Practices

### Rate Limiting
- Limit daily actions per account (50-100 recommended)
- Add random delays between actions (2-10 seconds)
- Rotate accounts frequently

### Anti-Detection
- Use realistic user agents
- Enable stealth mode
- Add human-like behavior
- Use proxies for large-scale operations

### Session Management
- Save cookies after successful login
- Reuse sessions when possible
- Handle session expiration gracefully

### Error Handling
- Implement retry logic for transient failures
- Log all actions for debugging
- Handle captchas and blocks

## Security Considerations

- **Credential Storage**: Use secure configuration management
- **Proxy Security**: Use trusted proxy providers
- **Rate Limiting**: Respect platform terms of service
- **Data Privacy**: Handle collected data responsibly

## Troubleshooting

### Common Issues

1. **Login Failures**
   - Check credentials
   - Verify 2FA settings
   - Try cookie-based login

2. **Detection**
   - Reduce action frequency
   - Add more human-like behavior
   - Use different proxies

3. **Element Not Found**
   - Update selectors for platform changes
   - Add wait conditions
   - Check page load state

### Debug Mode
```csharp
var browser = await browserFactory.CreateBrowserAsync(headless: false);
// Set headless: false to see browser actions
```

## Contributing

1. Fork the repository
2. Create feature branch
3. Implement platform support
4. Add tests
5. Submit pull request

## License

This project is for educational purposes. Use responsibly and in compliance with platform terms of service.

## Roadmap

- [ ] Facebook driver implementation
- [ ] Instagram driver implementation  
- [ ] TikTok driver implementation
- [ ] Twitter/X driver implementation
- [ ] YouTube driver implementation
- [ ] Advanced analytics dashboard
- [ ] Cloud deployment support
- [ ] Mobile app automation 