using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;

namespace Anubis.Bots.Linkedin
{
    public enum LinkedinNavigatorOptions
    {
        Home,
        Feed,
        ViewProfile,
        LoginPage
    }
    
    public class LinkedinNavigator
    {
        public static Uri FeedUrl 
            => new Uri("https://www.linkedin.com/feed/");

        private static Uri LoginPageUrl 
            =>  new Uri("https://www.linkedin.com/signup/cold-join?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Ffeed%2F&trk=login_reg_redirect");

        private readonly IWebDriver _driver;
        private readonly LinkedinDriver _linkedinDriver;

        public LinkedinNavigator(IWebDriver driver, LinkedinDriver linkedinDriver)
        {
            _driver = driver;
            _linkedinDriver = linkedinDriver;
        }
        
        public void Navigate(LinkedinNavigatorOptions option)
        {
            switch (option)
            {
                case LinkedinNavigatorOptions.Home:
                    NavigateToFeed();
                    break;
                case LinkedinNavigatorOptions.Feed:
                    NavigateToFeed();
                    break;
                case LinkedinNavigatorOptions.ViewProfile:
                    NavigateToMyProfile();
                     break;
                case LinkedinNavigatorOptions.LoginPage:
                    NavigateToLoginPage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }

        /// <summary>
        /// Navigate to the feed page 
        /// </summary>
        private void NavigateToFeed()
        {
            if (_linkedinDriver.CurrentPageUrl.StartsWith(FeedUrl.ToString())) return;

            try
            {
                var homeNavButton =
                    _linkedinDriver.FindElement(".global-nav__primary-link", FeedUrl.ToString(), retrySeconds:1);

                homeNavButton.Click();
            }
            catch (Exception)
            {
                Navigate(FeedUrl);
            }
        }

        /// <summary>
        /// Navigate to the login page 
        /// </summary>
        private void NavigateToLoginPage()
            => Navigate(LoginPageUrl);

        /// <summary>
        /// Navigate to the user profile page 
        /// </summary>
        private void NavigateToMyProfile()
        {
            var meDdl = _linkedinDriver.FindElement(".global-nav__me", 10);
            meDdl.Click();

            var elems = _linkedinDriver.FindElement(".global-nav__me-content", 10)
                .FindElements(By.TagName("a"))
                .ToList();

            Thread.Sleep(1000);
            
            var viewProfileLink =
                elems.FirstOrDefault(x => x.Text.Equals("View Profile", StringComparison.InvariantCultureIgnoreCase));

            viewProfileLink.Click();
        }

        /// <summary>
        /// Navigate to the user profile page 
        /// </summary>
        /// <param name="userProfileId"></param>
        public void NavigateToUserProfile(string userProfileId)
        {
            // open user profile by id
            var userUrl = $"https://www.linkedin.com/in/{userProfileId}/";

            Navigate(userUrl);
        }

        /// <summary>
        /// Navigate to the specified page
        /// </summary>
        /// <param name="pageUrl"></param>
        public void Navigate(string pageUrl)
            => Navigate(new Uri(pageUrl));
        
        /// <summary>
        /// Navigate to the specified page   
        /// </summary>
        /// <param name="pageUrl"></param>
        public void Navigate(Uri pageUrl)
        {
            if(_driver.Url != pageUrl.ToString())
                _driver.Navigate().GoToUrl(pageUrl);
        }
    }
}