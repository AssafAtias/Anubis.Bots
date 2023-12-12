using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

            // string userName = "neta931test931@gmail.com"; //"neta931test@gmail.com";
            // string password = "bcfhF.xe28!jdY8";
            // string loginPageUrl = "https://www.linkedin.com/uas/login?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Fsearch%2Fresults%2Fall%2F%3Fkeywords%3D"+searchCriteria+"&fromSignIn=true&trk=cold_join_sign_in";
            
            //Login(driver, loginPageUrl, userName, password);
            
            driver.Navigate().GoToUrl(searchPage.Replace("#{searchCriteria}", searchCriteria));
            
            SetCookiesFromFile(driver);

            driver.Navigate().GoToUrl(searchPage.Replace("#{searchCriteria}", searchCriteria));
            
            KeepCookiesInFile(driver);
            
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

            KeepCookiesInFile(driver);

            // Close the browser after task completion
            driver.Quit();
        }

        private static void SetCookiesFromFile(IWebDriver driver, string fileName ="cookies.txt")
        {
            var cookiesStr = File.ReadAllText(fileName);
            var cookies = JsonConvert.DeserializeObject<Cookie[]>(cookiesStr);

            foreach (var cookie in cookies)
            {
                driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain,
                    cookie.Path, DateTime.Now.AddDays(365), cookie.Secure, cookie.HttpOnly, "None"));
            }
        }

        private static void KeepCookiesInFile(IWebDriver driver, string fileName ="cookies.txt")
        {
            var cookies = driver.Manage().Cookies.AllCookies.ToArray();
            using (var f = File.Create(fileName))
            {
                var cookiesStr = JsonConvert.SerializeObject(cookies);
                f.Write(System.Text.Encoding.UTF8.GetBytes(cookiesStr), 0, cookiesStr.Length);
                f.Close();
            }
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
            
            // Thread.Sleep(rnd.Next(500, 5000)); // Sleep for a random amount of time (1-5 seconds) to simulate a human
            
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
                var str = driver.FindElement(By.CssSelector("[title*='unseen message']"))
                    ?.Text;
                
                int.TryParse(str?.Trim(), out unseenMessagesCount);
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

        private static void SendMessageToUser(IWebDriver driver, string userId, string message = "Hi, my name is Netanel. Let's connect!")
        {
            VisitUserProfile(driver, userId);
            
            // send message to the user
            var cssSelector = "[aria-label*='Message ']";
            IWebElement messageButton = driver.FindElement(By.CssSelector(cssSelector));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", messageButton);

            Thread.Sleep(500);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", messageButton);

            Thread.Sleep(1000);

            // send message
            IWebElement messageTextArea = driver.FindElement(By.CssSelector("[aria-label='Write a message…']"));

            messageTextArea.SendKeys(message);

            IWebElement buttonSendMessage =
                driver.FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                    x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));

            buttonSendMessage.Click();

            // wait until the click ^ action will be done!
            Thread.Sleep(1000);

            // close the message box
            IWebElement buttonCloseConversation = driver.FindElements(By.CssSelector(".artdeco-button__text"))
                ?.FirstOrDefault(x =>
                    x.Text.StartsWith("Close your conversation", StringComparison.InvariantCultureIgnoreCase));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonCloseConversation);
        }

        private static bool SendUserInvite(IWebDriver driver, string userId)
        {
            VisitUserProfile(driver, userId);
            
            Thread.Sleep(1000);
            
            // connect
            IWebElement buttonConnect =
                driver.FindElement(By.CssSelector("[aria-label*='Invite']"));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonConnect);

            try
            {
                //Invitation not sent to the user. You can resend an invitation 3 weeks after withdrawing it.
                IWebElement withdrawingStatus =
                    driver.FindElement(By.CssSelector(".artdeco-toast-item__message"));

                if (withdrawingStatus != null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {  }
            
            // add a note
            IWebElement buttonAddANote =
                driver.FindElement(By.CssSelector("[aria-label='Add a note']"));
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", buttonAddANote);
            
            Thread.Sleep(500);
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonAddANote);
                
            Thread.Sleep(1000);
            
            // write and send a note with the connection request 
            IWebElement textAreaNote =
                driver.FindElement(By.CssSelector("[name='message']"));
            
            textAreaNote.SendKeys("Let's connect! Netanel");
            
            IWebElement buttonSendNote =
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

        private static void Login(IWebDriver driver, string loginPageUrl, string userName, string password)
        {
            // Open the webpage
            driver.Navigate().GoToUrl(loginPageUrl);
            
            IWebElement login_UserName = driver.FindElement(By.Id("username"));
            
            login_UserName.SendKeys(userName);
            
            IWebElement login_Pass = driver.FindElement(By.Id("password"));
            
            login_Pass.SendKeys(password);
            
            //aria-label="Sign in"
            IWebElement login_Button = driver.FindElement(By.CssSelector("[aria-label='Sign in']"));
            
            login_Button.Click();
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
