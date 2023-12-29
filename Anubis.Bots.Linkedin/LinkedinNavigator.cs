using System;
using System.Linq;
using System.Threading;
using System.Web;
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
            =>  new Uri("https://www.linkedin.com/uas/login?session_redirect=https%3A%2F%2Fwww%2Elinkedin%2Ecom%2Ffeed%2F&fromSignIn=true&trk=cold_join_sign_in");

        private readonly IWebDriver _driver;
        private readonly LinkedinDriver _linkedinDriver;

        public LinkedinNavigator(IWebDriver driver, LinkedinDriver linkedinDriver)
        {
            _driver = driver;
            _linkedinDriver = linkedinDriver;
        }
        
        /// <summary>
        /// Navigate to the specified page 
        /// </summary>
        /// <param name="option"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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

        /// <summary>
        /// View people by hashtag 
        /// </summary>
        /// <param name="hashtag"></param>
        /// <param name="waitOnLoadSeconds"></param>
        public void ViewPeopleByHashtag(string hashtag)
        {
            var encodedHashTag = HttpUtility.UrlEncode(hashtag);
            var uriTemplate =
                "https://www.linkedin.com/search/results/people/?keywords=#{HASHTAG}&origin=SWITCH_SEARCH_VERTICAL";
            
            Navigate(uriTemplate.Replace("#{HASHTAG}", encodedHashTag));
        }

        /// <summary>
        /// View posts by hashtag 
        /// </summary>
        /// <param name="hashtag"></param>
        public void ViewPostsByHashtag(string hashtag)
        {
            var encodedHashTag = HttpUtility.UrlEncode(hashtag);
            var uriTemplate =
                "https://www.linkedin.com/search/results/content/?keywords=#{HASHTAG}&origin=SWITCH_SEARCH_VERTICAL";
            
            Navigate(uriTemplate.Replace("#{HASHTAG}", encodedHashTag));
        }
    }
}