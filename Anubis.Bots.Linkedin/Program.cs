using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Anubis.Bots.Linkedin
{
    internal class Program
    {
        static string userName = "netanelabergel@gmail.com";
        static string password = "Na27986772";
        static string searchCriteria = "Assaf Atias";
        static string startUrl = "https://www.linkedin.com/uas/login?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Fsearch%2Fresults%2Fall%2F%3Fkeywords%3D"+searchCriteria+"&fromSignIn=true&trk=cold_join_sign_in";
        
        static string searchPage = "https://www.linkedin.com/search/results/all/?keywords=#{searchCriteria}&origin=GLOBAL_SEARCH_HEADER";
        
        public static void Main(string[] args)
        {
            // Initialize the Chrome WebDriver
            IWebDriver driver = new ChromeDriver();

            // Open the webpage
            driver.Navigate().GoToUrl(startUrl);
            
            IWebElement login_UserName = driver.FindElement(By.Id("username"));
            
            login_UserName.SendKeys(userName);
            
            IWebElement login_Pass = driver.FindElement(By.Id("password"));
            
            login_Pass.SendKeys(password);
            
            //aria-label="Sign in"
            IWebElement login_Button = driver.FindElement(By.CssSelector("[aria-label='Sign in']"));
            
            login_Button.Click();

            // var tmpSearchPage = searchPage.Replace("#{searchCriteria}", searchCriteria);
            // driver.Navigate().GoToUrl(tmpSearchPage);
            //IWebElement messageButton = driver.FindElement(By.CssSelector("[aria-label='Message ']"));

            // open user profile by id
            var userId = "assaf-atias-84099832";
            
            var userUrl = $"https://www.linkedin.com/in/{userId}/";
            driver.Navigate().GoToUrl(userUrl);
            
            Thread.Sleep(1000);
            
            var cssSelector = "[aria-label*='Message ']";
            
            IWebElement messageButton = driver.FindElement(By.CssSelector(cssSelector));

            // ((IJavaScriptExecutor)driver).ExecuteScript("$('" + cssSelector + "').click();");

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", messageButton);
            
            Thread.Sleep(500);
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", messageButton);
            
            Thread.Sleep(1000);
            
            // send message
            IWebElement messageTextArea = driver.FindElement(By.CssSelector("[aria-label='Write a message…']"));
            
            messageTextArea.SendKeys("Hi, my name is Netanel. Let's connect!");
            
            IWebElement buttonSendMessage =
                driver.FindElement(By.CssSelector(".msg-form__send-button.artdeco-button.artdeco-button--1"));
            
            buttonSendMessage.Click();
            
            // wait until the click ^ action will be done!
            Thread.Sleep(1000);
            
            // close the message box
            IWebElement buttonCloseConversation = driver.FindElements(By.CssSelector(".artdeco-button__text"))
                ?.FirstOrDefault(x =>
                    x.Text.StartsWith("Close your conversation", StringComparison.InvariantCultureIgnoreCase));
            
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", buttonCloseConversation);
            
            // Close the browser after task completion
            driver.Quit();
            
        }
    }
}
