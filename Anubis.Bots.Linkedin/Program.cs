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

            var tmpSearchPage = searchPage.Replace("#{searchCriteria}", searchCriteria);
            
            driver.Navigate().GoToUrl(tmpSearchPage);
            
            IWebElement messageButton = driver.FindElement(By.CssSelector("[aria-label='Message ']"));
            
            messageButton.Click();
            
            IWebElement messageTextArea = driver.FindElement(By.CssSelector("[aria-label='Write a message…']"));
            
            messageTextArea.SendKeys("Hi, I would like to connect with you!!! - anubis bot mother fucker hahahaha");

            IWebElement buttonSendMessage =
                driver.FindElement(By.CssSelector(".msg-form__send-button.artdeco-button.artdeco-button--1"));
            
            buttonSendMessage.Click();
            
            // // Find the element using the aria-label attribute
            // IWebElement element = driver.FindElement(By.CssSelector("[aria-label='Invite "+searchCriteria+" to connect']"));
            //
            // // Perform actions on the element, like clicking it
            // element.Click();

            // Close the browser after task completion
            driver.Quit();
            
        }
    }
}