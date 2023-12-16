using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Polly;

namespace Anubis.Bots.Linkedin
{
    public class WebBrowserDriver : IDisposable
    {
        private readonly IWebDriver _driver;

        public WebBrowserDriver(IWebDriver driver)
        {
            _driver = driver;
        }

        public string CurrentPageUrl => _driver.Url;

        public IWebElement FindElement(By by, uint retrySeconds, bool throwExceptionIfNotFound = true)
        {
            var retryPolicy = Policy
                .Handle<NoSuchElementException>()
                .Or<ElementNotVisibleException>()
                .Or<ElementNotSelectableException>()
                .WaitAndRetry(
                    retryCount: (int)(retrySeconds / 0.5), // Number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500), // Wait 500ms between each try
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        // This is your retry logic, you could log the exception or do something else.
                        Console.WriteLine($"Retrying {retryCount} time...");
                    });

            IWebElement element = null;
    
            retryPolicy.Execute(() =>
            {
                // Attempt to find the element
                element = FindElement(by) 
                    ?? throw new ElementNotSelectableException("Element not found");
            });

            return element ?? (throwExceptionIfNotFound
                ? throw new ElementNotSelectableException("Element not found")
                : element);
        }
        
        public IWebElement FindElement(string cssSelector, uint retrySeconds, bool throwExceptionIfNotFound = true)
        {
            if (cssSelector == null) throw new ArgumentNullException(nameof(cssSelector));
            if (retrySeconds <= 0) throw new ArgumentOutOfRangeException(nameof(retrySeconds));
            
            var retryPolicy = Policy
                .Handle<NoSuchElementException>()
                .Or<ElementNotVisibleException>()
                .Or<ElementNotSelectableException>()
                .WaitAndRetry(
                    retryCount: (int)(retrySeconds / 0.5), // Number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500), // Wait 500ms between each try
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        // This is your retry logic, you could log the exception or do something else.
                        Console.WriteLine($"Retrying {retryCount} time...");
                    });

            IWebElement element = null;
    
            retryPolicy.Execute(() =>
            {
                // Attempt to find the element
                element = FindElement(By.CssSelector(cssSelector)) 
                    ?? throw new ElementNotSelectableException("Element not found");
            });

            return element ?? (throwExceptionIfNotFound
                ? throw new ElementNotSelectableException("Element not found")
                : element);
        }
     
        public IWebElement FindElement(
            string cssSelector,
            string hrefStartWith,
            uint retrySeconds)
        {
            if (cssSelector == null) throw new ArgumentNullException(nameof(cssSelector));
            if (hrefStartWith == null) throw new ArgumentNullException(nameof(hrefStartWith));
            if (retrySeconds <= 0) throw new ArgumentOutOfRangeException(nameof(retrySeconds));
            
            var retryPolicy = Policy
                .Handle<NoSuchElementException>()
                .Or<ElementNotVisibleException>()
                .Or<ElementNotSelectableException>()
                .WaitAndRetry(
                    retryCount: (int)(retrySeconds / 0.5), // Number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500), // Wait 500ms between each try
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        // This is your retry logic, you could log the exception or do something else.
                        Console.WriteLine($"Retrying {retryCount} time...");
                    });

            IWebElement element = null;
    
            retryPolicy.Execute(() =>
            {
                // Attempt to find the element
                element = FindElements(By.CssSelector(cssSelector))?.FirstOrDefault(x =>
                    x.GetAttribute("href").StartsWith(hrefStartWith, StringComparison.InvariantCultureIgnoreCase))
                          ?? throw new ElementNotSelectableException("Element not found");
            });

            return element ?? throw new ElementNotSelectableException("Element not found");
        }

        public IWebElement FindElement(string cssSelector) 
            => _driver.FindElement(By.CssSelector(cssSelector));

        public IWebElement FindElement(string cssSelector, string hrefStartWith) =>
            _driver.FindElements(By.CssSelector(cssSelector))?.FirstOrDefault(x =>
                x.GetAttribute("href").StartsWith(hrefStartWith, StringComparison.InvariantCultureIgnoreCase));

        public ReadOnlyCollection<IWebElement> FindElements(string cssSelector)
            => _driver.FindElements(By.CssSelector(cssSelector));

        public ReadOnlyCollection<IWebElement> FindElements(By by)
            => _driver.FindElements(by);

        public IWebElement FindElement(By by)
            => _driver.FindElement(by);
        
        public void ExportPageCookiesToFile(IWebDriver driver, string cookiesTargetFilePath = "cookies.txt")
        {
            var cookies = GetPageCookies(driver);
            using (var f = File.Create(cookiesTargetFilePath))
            {
                var cookiesStr = JsonConvert.SerializeObject(cookies);
                f.Write(Encoding.UTF8.GetBytes(cookiesStr), 0, cookiesStr.Length);
                f.Close();
            }
        }

        /// <summary>
        /// Get all cookies from the current page 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public Cookie[] GetPageCookies(IWebDriver driver)
        {
            var cookies = driver.Manage().Cookies.AllCookies.ToArray();
            return cookies;
        }
        
        /// <summary>
        /// Execute JavaScript in the context of the currently selected frame or window. 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object ExecuteJavaScript(string script, params object[] args)
            => ((IJavaScriptExecutor)_driver).ExecuteScript(script, args);


        public void Dispose()
        {
            _driver?.Quit();    
            _driver?.Dispose();
        }
    }
}