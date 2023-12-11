using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Anubis.Bots.Linkedin
{
    internal class Program
    {
        private static string userName = "neta931test@gmail.com";
        private static string password = "bcfhF.xe28!jdY8";
        static string searchCriteria = "Assaf Atias";
        static string startUrl = "https://www.linkedin.com/uas/login?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Fsearch%2Fresults%2Fall%2F%3Fkeywords%3D"+searchCriteria+"&fromSignIn=true&trk=cold_join_sign_in";
        
        static string searchPage = "https://www.linkedin.com/search/results/all/?keywords=#{searchCriteria}&origin=GLOBAL_SEARCH_HEADER";
        
        public static void Main(string[] args)
        {
            // Initialize the Chrome WebDriver
            IWebDriver driver = new ChromeDriver();

            Login(driver);

            VisitUserProfile(driver);
            
            IsUserInvitePending(driver);
            
            SendConnectInvite(driver);

            SendMessageToUser(driver);

            // Close the browser after task completion
            driver.Quit();
        }

        private static void SendMessageToUser(IWebDriver driver)
        {
            // send message to the user
            var cssSelector = "[aria-label*='Message ']";
            IWebElement messageButton = driver.FindElement(By.CssSelector(cssSelector));

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", messageButton);

            Thread.Sleep(500);

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", messageButton);

            Thread.Sleep(1000);

            // send message
            IWebElement messageTextArea = driver.FindElement(By.CssSelector("[aria-label='Write a message…']"));

            messageTextArea.SendKeys("Hi, my name is Netanel. Let's connect!");

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

        private static bool SendConnectInvite(IWebDriver driver)
        {
            // connect
            IWebElement buttonConnect =
                driver.FindElement(By.CssSelector("[aria-label*='Invite']"));

            Thread.Sleep(500);

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

        private static bool IsUserInvitePending(IWebDriver driver)
        {
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

        private static void Login(IWebDriver driver)
        {
            // Open the webpage
            driver.Navigate().GoToUrl(startUrl);
            
            IWebElement login_UserName = driver.FindElement(By.Id("username"));
            
            login_UserName.SendKeys(userName);
            
            IWebElement login_Pass = driver.FindElement(By.Id("password"));
            
            login_Pass.SendKeys(password);
            
            //aria-label="Sign in"
            IWebElement login_Button = driver.FindElement(By.CssSelector("[aria-label='Sign in']"));
            
            login_Button.Click();
        }

        private static void VisitUserProfile(IWebDriver driver)
        {
            // open user profile by id
            var userId = "assaf-atias-84099832";
            
            var userUrl = $"https://www.linkedin.com/in/{userId}/";
            driver.Navigate().GoToUrl(userUrl);
            
            Thread.Sleep(1000);
        }

        private static void SendConnectionRequest(IWebDriver driver)
        {
            // Code for sending a connection request goes here
        }

        private static void AddNoteAndSendMessage(IWebDriver driver)
        {
            // Code for adding a note and sending a message goes here
        }
    }
}
