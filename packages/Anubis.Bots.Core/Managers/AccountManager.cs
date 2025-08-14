using Anubis.Bots.Core.Interfaces;
using Anubis.Bots.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Anubis.Bots.Core.Managers
{
    public class AccountManager
    {
        private readonly List<SocialAccount> _accounts;
        private readonly Dictionary<string, DateTime> _lastUsed;
        private readonly Dictionary<string, int> _dailyActionCount;
        private readonly ILogger? _logger;
        private readonly Random _random;
        private readonly object _lock = new object();

        public AccountManager(ILogger? logger = null)
        {
            _accounts = new List<SocialAccount>();
            _lastUsed = new Dictionary<string, DateTime>();
            _dailyActionCount = new Dictionary<string, int>();
            _logger = logger;
            _random = new Random();
        }

        public void AddAccount(SocialAccount account)
        {
            lock (_lock)
            {
                if (!_accounts.Any(a => a.Id == account.Id))
                {
                    _accounts.Add(account);
                    _logger?.LogInformation("Added account {Username} for {Platform}", account.Username, account.Platform);
                }
            }
        }

        public void AddAccounts(IEnumerable<SocialAccount> accounts)
        {
            foreach (var account in accounts)
            {
                AddAccount(account);
            }
        }

        public void LoadAccountsFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var accounts = JsonConvert.DeserializeObject<List<SocialAccount>>(json);
                    if (accounts != null)
                    {
                        AddAccounts(accounts);
                        _logger?.LogInformation("Loaded {Count} accounts from {FilePath}", accounts.Count, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading accounts from file {FilePath}", filePath);
            }
        }

        public void SaveAccountsToFile(string filePath)
        {
            try
            {
                lock (_lock)
                {
                    var json = JsonConvert.SerializeObject(_accounts, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    _logger?.LogInformation("Saved {Count} accounts to {FilePath}", _accounts.Count, filePath);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving accounts to file {FilePath}", filePath);
            }
        }

        public SocialAccount? GetAvailableAccount(string platform, int maxDailyActions = 50, TimeSpan? cooldownPeriod = null)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var cooldown = cooldownPeriod ?? TimeSpan.FromMinutes(30);

                // Reset daily counts if it's a new day
                var today = now.Date;
                foreach (var kvp in _dailyActionCount.ToList())
                {
                    if (_lastUsed.TryGetValue(kvp.Key, out var lastUsed) && lastUsed.Date < today)
                    {
                        _dailyActionCount[kvp.Key] = 0;
                    }
                }

                var availableAccounts = _accounts
                    .Where(a => a.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase))
                    .Where(a => a.IsActive)
                    .Where(a => !_lastUsed.ContainsKey(a.Id) || now - _lastUsed[a.Id] >= cooldown)
                    .Where(a => !_dailyActionCount.ContainsKey(a.Id) || _dailyActionCount[a.Id] < maxDailyActions)
                    .ToList();

                if (!availableAccounts.Any())
                {
                    _logger?.LogWarning("No available accounts for {Platform}", platform);
                    return null;
                }

                // Select account with least recent usage and least daily actions
                var selectedAccount = availableAccounts
                    .OrderBy(a => _lastUsed.GetValueOrDefault(a.Id, DateTime.MinValue))
                    .ThenBy(a => _dailyActionCount.GetValueOrDefault(a.Id, 0))
                    .First();

                // Update usage tracking
                _lastUsed[selectedAccount.Id] = now;
                _dailyActionCount[selectedAccount.Id] = _dailyActionCount.GetValueOrDefault(selectedAccount.Id, 0) + 1;

                _logger?.LogInformation("Selected account {Username} for {Platform} (daily actions: {DailyCount})", 
                    selectedAccount.Username, platform, _dailyActionCount[selectedAccount.Id]);

                return selectedAccount;
            }
        }

        public List<SocialAccount> GetAccountsByPlatform(string platform)
        {
            lock (_lock)
            {
                return _accounts
                    .Where(a => a.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        public void UpdateAccountStatus(string accountId, bool isActive)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account != null)
                {
                    account.IsActive = isActive;
                    _logger?.LogInformation("Updated account {Username} status to {IsActive}", account.Username, isActive);
                }
            }
        }

        public void UpdateAccountCookies(string accountId, Dictionary<string, string> cookies)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account != null)
                {
                    account.Cookies = cookies;
                    account.LastUsed = DateTime.UtcNow;
                    _logger?.LogInformation("Updated cookies for account {Username}", account.Username);
                }
            }
        }

        public void ResetDailyCounts()
        {
            lock (_lock)
            {
                _dailyActionCount.Clear();
                _logger?.LogInformation("Reset daily action counts for all accounts");
            }
        }

        public Dictionary<string, object> GetAccountStats()
        {
            lock (_lock)
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalAccounts"] = _accounts.Count,
                    ["ActiveAccounts"] = _accounts.Count(a => a.IsActive),
                    ["Platforms"] = _accounts.Select(a => a.Platform).Distinct().ToList(),
                    ["DailyActionCounts"] = _dailyActionCount.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    ["LastUsed"] = _lastUsed.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                };

                return stats;
            }
        }

        public void RemoveAccount(string accountId)
        {
            lock (_lock)
            {
                var account = _accounts.FirstOrDefault(a => a.Id == accountId);
                if (account != null)
                {
                    _accounts.Remove(account);
                    _lastUsed.Remove(accountId);
                    _dailyActionCount.Remove(accountId);
                    _logger?.LogInformation("Removed account {Username}", account.Username);
                }
            }
        }

        public void ClearAccounts()
        {
            lock (_lock)
            {
                _accounts.Clear();
                _lastUsed.Clear();
                _dailyActionCount.Clear();
                _logger?.LogInformation("Cleared all accounts");
            }
        }
    }
} 