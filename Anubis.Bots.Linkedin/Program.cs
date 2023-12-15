using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Cookie = System.Net.Cookie;

namespace Anubis.Bots.Linkedin
{
    internal class Program
    {
        static string searchCriteria = "Assaf Atias";
        static string searchPage = "https://www.linkedin.com/search/results/all/?keywords=#{searchCriteria}&origin=GLOBAL_SEARCH_HEADER";
        
        public static void Main(string[] args)
        {
            // Initialize the Chrome WebDriver
            IWebDriver driver = new ChromeDriver();
            
            driver.Manage().Window.Maximize();

            // var userName = "neta931test931@gmail.com"; //"neta931test@gmail.com";
            // var password = "bcfhF.xe28!jdY8";
            // Login(driver, userName, password);
            
            var cookies = ImportCookiesFromFile("cookies.txt");
            Login(driver, cookies);

            driver.Navigate().GoToUrl(searchPage.Replace("#{searchCriteria}", searchCriteria));
            
            ExportPageCookiesToFile(driver, "cookies.txt");
            
            DummyMouseMovement(driver);
            
            var unseenMessagesCount = GetUnseenMessages(driver);

            if (unseenMessagesCount > 0)
            {
                var element = driver.FindElement(By.CssSelector(".msg-overlay-bubble-header__control"));
                
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
                
                Thread.Sleep(500);
            }
            
            var userId = "assaf-atias-84099832";

            userId = "netanel-abergel";

            if(!IsUserInviteStatusIsPending(driver, userId) && !IsConnectedUser(driver, userId))
                SendUserInvite(driver, userId);
            else
                SendMessageToUser(driver, userId);

            ExportPageCookiesToFile(driver, "cookies.txt");
            
            VisitPage(driver, "https://www.linkedin.com/feed/");
            
            var startPostButton = WaitUntilLoaded(driver, ".share-box-feed-entry__trigger");
            
            startPostButton.Click();
            
            var postTextArea = WaitUntilLoaded(driver, "[aria-label='Text editor for creating content']");
            
            postTextArea.SendKeys("testtttt");
            
            var postButton = WaitUntilLoaded(driver, ".share-actions__primary-action");
            
            postButton.Click();
            
            // Close the browser after task completion
            driver.Quit();
        }

        private static void EnsureLoggedIn(IWebDriver driver, Cookie[] cookies)
        {
            if (IsUserLoggedIn(driver)) return;
            
            Login(driver, cookies);
        }
        
        private static void EnsureLoggedIn(IWebDriver driver, string userName, string password, bool forceLogin = false)
        {
            if (IsUserLoggedIn(driver) && !forceLogin) return;

            
            
            // login
          
            // "https://www.linkedin.com/uas/login?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Fsearch%2Fresults%2Fall%2F%3Fkeywords%3D" +
                // searchCriteria + "&fromSignIn=true&trk=cold_join_sign_in";
            
            Login(driver, userName, password);
        }

        private static bool IsUserLoggedIn(IWebDriver driver)
        {
            var feedUrl = "https://www.linkedin.com/feed/";
            VisitPage(driver, feedUrl);

            if (driver.Url == feedUrl)
            {
                WaitUntilLoaded(driver, ".share-box-feed-entry__top-bar");
                return true;
            }

            return false;
        }

        private static void VisitPage(IWebDriver driver, string pageUrl)
        {
            if(driver.Url != pageUrl)
                driver.Navigate().GoToUrl(pageUrl);
        }

        private static void Login(IWebDriver driver, Cookie[] cookies)
        {
            VisitPage(driver, "https://www.linkedin.com/feed/");
            
            foreach (var cookie in cookies)
            {
                driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain,
                    cookie.Path, DateTime.Now.AddDays(365), cookie.Secure, cookie.HttpOnly, "None"));
            }

            if (!IsUserLoggedIn(driver)) throw new UnauthorizedAccessException("Failed to login");
        }
        
        private static void Login(IWebDriver driver, string cookiesSourceFilePath = "cookies.txt")
        {
            var cookies = ImportCookiesFromFile(cookiesSourceFilePath);

            Login(driver, cookies);
        }

        private static Cookie[] ImportCookiesFromFile(string cookiesSourceFilePath)
        {
            var cookiesStr = File.ReadAllText(cookiesSourceFilePath);
            var cookies = JsonConvert.DeserializeObject<Cookie[]>(cookiesStr);
            return cookies;
        }

        private static void ExportPageCookiesToFile(IWebDriver driver, string cookiesTargetFilePath = "cookies.txt")
        {
            var cookies = GetPageCookies(driver);
            using (var f = File.Create(cookiesTargetFilePath))
            {
                var cookiesStr = JsonConvert.SerializeObject(cookies);
                f.Write(System.Text.Encoding.UTF8.GetBytes(cookiesStr), 0, cookiesStr.Length);
                f.Close();
            }
        }

        private static OpenQA.Selenium.Cookie[] GetPageCookies(IWebDriver driver)
        {
            var cookies = driver.Manage().Cookies.AllCookies.ToArray();
            return cookies;
        }

        private static void DummyMouseMovement(IWebDriver driver)
        {
            // Create an instance of the Actions class
            var actions = new OpenQA.Selenium.Interactions.Actions(driver);

            var rnd = new Random(0);
            
            // Perform dummy mouse movements
            // You can chain multiple actions together
            actions.MoveByOffset(rnd.Next(0, 100), rnd.Next(0, 100)) // Move the mouse to a specific offset (x, y) from the current position
                .MoveByOffset(rnd.Next(0, 100), 0)    // Move right by 100 pixels
                .MoveByOffset(0, rnd.Next(0, 100))    // Move down by 100 pixels
                .MoveByOffset(-rnd.Next(0, 100), 0)   // Move left by 100 pixels
                .MoveByOffset(0, -rnd.Next(0, 100))   // Move up by 100 pixels
                .Perform();              // Perform all the actions
            
            // Scroll down by a certain number of pixels (e.g., 500 pixels)
            actions.MoveByOffset(0, 500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);

            // Scroll up by a certain number of pixels (e.g., -500 pixels)
            actions.MoveByOffset(0, -500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);
            
            // Thread.Sleep(rnd.Next(500, 5000)); // Sleep for a random amount of time (1-5 waitSeconds) to simulate a human
            
            // Perform dummy mouse movements
            // You can chain multiple actions together
            actions.MoveByOffset(rnd.Next(0, 100), rnd.Next(0, 100)) // Move the mouse to a specific offset (x, y) from the current position
                .MoveByOffset(rnd.Next(0, 100), 0)    // Move right by 100 pixels
                .MoveByOffset(0, rnd.Next(0, 100))    // Move down by 100 pixels
                .MoveByOffset(-rnd.Next(0, 100), 0)   // Move left by 100 pixels
                .MoveByOffset(0, -rnd.Next(0, 100))   // Move up by 100 pixels
                .Perform();              // Perform all the actions
            
            // Scroll down by a certain number of pixels (e.g., 500 pixels)
            actions.MoveByOffset(0, 500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            System.Threading.Thread.Sleep(1000);

            // Scroll up by a certain number of pixels (e.g., -500 pixels)
            actions.MoveByOffset(0, -500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);
        }

        private static int GetUnseenMessages(IWebDriver driver)
        {
            var unseenMessagesCount = 0;
            
            try
            {
                var element = WaitUntilLoaded(driver, "[title*='unseen message']");
                var str = element?.Text?.Trim();
                
                int.TryParse(str, out unseenMessagesCount);
            }
            catch (Exception)
            { }

            return unseenMessagesCount;
        }

        private static bool IsConnectedUser(IWebDriver driver, string userId)
        {
            var isConnectedUser = false;
            
            VisitUserProfile(driver, userId);
            
            try
            {
                 isConnectedUser =
                    driver.FindElements(By.CssSelector("[aria-label*='Invite']"))?
                        .FirstOrDefault(x => x.GetCssValue("class").Contains("pvs-profile-actions__action"))
                    == null;
            }
            catch (Exception)
            { }

            return isConnectedUser;
        }

        private static void SendMessageToUser(IWebDriver driver, 
            string userId, 
            string message = "Hi, my name is Netanel. Let's connect!")
        {
            VisitUserProfile(driver, userId);
            
            var messageButton = WaitUntilLoaded(driver, cssSelector: "[aria-label*='Message ']", waitSeconds:10);
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", messageButton);

            Thread.Sleep(500);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", messageButton);

            Thread.Sleep(1000);

            var messageTextArea = WaitUntilLoaded(driver, cssSelector: "[aria-label='Write a message…']", waitSeconds:10);
            
            // send message
            // var messageTextArea = driver.FindElement(By.CssSelector("[aria-label='Write a message…']"));

            messageTextArea.SendKeys(message);
            
            var buttonSendMessage =
                driver.FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                    x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));

            buttonSendMessage.Click();

            // wait until the click ^ action will be done!
            WaitUntilLoaded(driver, 
                cssSelector: "[aria-label='Write a message…']", 
                waitSeconds:10);

            // close the message box
            var buttonCloseConversation = driver.FindElements(By.CssSelector(".artdeco-button__text"))
                ?.FirstOrDefault(x =>
                    x.Text.StartsWith("Close your conversation", StringComparison.InvariantCultureIgnoreCase));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonCloseConversation);
        }

        private static IWebElement WaitUntilLoaded(ISearchContext driver, string cssSelector, uint waitSeconds = 10)
        {
            DateTime start = DateTime.Now; 
            IWebElement element = null;
            var maxIdx = 1000;
            while (waitSeconds > (DateTime.Now - start).TotalSeconds && maxIdx-- > 0)
            {
                try
                {
                    element = driver.FindElement(By.CssSelector(cssSelector));
                    if (element != null)
                    {
                        break;
                    }

                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                }
            }
            
            return element ?? throw new ElementNotSelectableException("Element not found");
        }

        private static bool SendUserInvite(IWebDriver driver, string userId)
        {
            VisitUserProfile(driver, userId);
            
            Thread.Sleep(1000);
            
            // connect
            var buttonConnect =
                driver.FindElement(By.CssSelector("[aria-label*='Invite']"));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonConnect);

            try
            {
                //Invitation not sent to the user. You can resend an invitation 3 weeks after withdrawing it.
                var withdrawingStatus =
                    driver.FindElement(By.CssSelector(".artdeco-toast-item__message"));

                if (withdrawingStatus != null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {  }
            
            // add a note
            var buttonAddANote =
                driver.FindElement(By.CssSelector("[aria-label='Add a note']"));
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", buttonAddANote);
            
            Thread.Sleep(500);
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonAddANote);
                
            Thread.Sleep(1000);
            
            // write and send a note with the connection request 
            var textAreaNote =
                driver.FindElement(By.CssSelector("[name='message']"));
            
            textAreaNote.SendKeys("Let's connect! Netanel");
            
            var buttonSendNote =
                driver.FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                    x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));
            
            buttonSendNote.Click();
            
            return true; 
        }

        private static bool IsUserInviteStatusIsPending(IWebDriver driver, string userId)
        {
            VisitUserProfile(driver, userId);
            
            try
            {
                // check if the user is already connected
                var pendingBtnExists = driver.FindElement(By.CssSelector("[aria-label*='Pending, ']"));
                if (pendingBtnExists != null) // user is waiting for connection approval 
                {
                    return true;
                }
            }
            catch (Exception e)
            { }

            return false;
        }

        private static void Login(IWebDriver driver, string userName, string password)
        {
            var loginPageUrl =
                "https://www.linkedin.com/signup/cold-join?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Ffeed%2F&trk=login_reg_redirect";

            VisitPage(driver, loginPageUrl);

            var login_UserName = driver.FindElement(By.Id("username"));
            
            login_UserName.SendKeys(userName);
            
            var login_Pass = driver.FindElement(By.Id("password"));
            
            login_Pass.SendKeys(password);
            
            //aria-label="Sign in"
            var login_Button = driver.FindElement(By.CssSelector("[aria-label='Sign in']"));
            
            login_Button.Click();
            
            if (!IsUserLoggedIn(driver)) throw new UnauthorizedAccessException("Failed to login");
        }

        private static void VisitUserProfile(IWebDriver driver, string userId)
        {
            // open user profile by id
            var userUrl = $"https://www.linkedin.com/in/{userId}/";

            if (driver.Url != userUrl)
            {
                driver.Navigate().GoToUrl(userUrl);
                Thread.Sleep(1000); // wait for page loading
            }
        }
        
    }
}
