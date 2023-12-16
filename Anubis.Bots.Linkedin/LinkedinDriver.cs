using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Cookie = System.Net.Cookie;

namespace Anubis.Bots.Linkedin
{
    public class LinkedinDriver : WebBrowserDriver
    {
        private readonly IWebDriver _driver;
        private readonly LinkedinNavigator _linkedinNavigator;

        /// <summary>
        /// Linkedin driver constructor 
        /// </summary>
        /// <param name="driver"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public LinkedinDriver(
            IWebDriver driver) 
            : base(driver)
        {
            _driver = driver;
            _linkedinNavigator = new LinkedinNavigator(driver, this);
            
            if(!IsUserLoggedIn()) 
                throw new UnauthorizedAccessException("User is not logged in to Linkedin");  
        }
        
        /// <summary>
        /// Linkedin driver constructor
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="cookies"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public LinkedinDriver(
            IWebDriver driver, 
            Cookie[] cookies) 
            : base(driver) 
        {
            _driver = driver;
            _linkedinNavigator = new LinkedinNavigator(driver, this);
            
            EnsureLoggedIn(cookies);
        }

        /// <summary>
        /// Linkedin driver constructor 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public LinkedinDriver(
            IWebDriver driver, 
            string userName, 
            string password) : base(driver) 
        {
            _driver = driver;
            _linkedinNavigator = new LinkedinNavigator(driver, this);
            
            EnsureLoggedIn(userName, password);
        }

        /// <summary>
        /// Linkedin driver constructor 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="cookiesSourceFilePath"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public LinkedinDriver(
            IWebDriver driver, 
            string cookiesSourceFilePath) 
            : base(driver)
        {
            _driver = driver;
            _linkedinNavigator = new LinkedinNavigator(driver, this);
            
            EnsureLoggedIn(cookiesSourceFilePath);
        }
        
        /// <summary>
        /// Ensure user is logged in 
        /// </summary>
        /// <param name="cookies"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void EnsureLoggedIn(Cookie[] cookies)
        {
            if (IsUserLoggedIn()) return;
            
            Login(cookies);
        }
        
        /// <summary>
        /// Ensure user is logged in 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        private void EnsureLoggedIn(string userName, string password)
        {
            if (IsUserLoggedIn()) return;
            
            Login(userName, password);
        }
        
        /// <summary>
        /// Ensure user is logged in 
        /// </summary>
        /// <param name="cookiesSourceFilePath"></param>
        private void EnsureLoggedIn(string cookiesSourceFilePath)
        {
            if (IsUserLoggedIn()) return;
            
            Login(cookiesSourceFilePath);
        }
        
        /// <summary>
        /// Check if user is logged in 
        /// </summary>
        /// <returns></returns>
        private bool IsUserLoggedIn()
        {
            try
            {
                _linkedinNavigator.Navigate(LinkedinNavigatorOptions.Feed);

                if (CurrentPageUrl != LinkedinNavigator.FeedUrl.ToString()) return false;
            
                var element = FindElement(".share-box-feed-entry__top-bar",10);
                return element != null;
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }
        
        /// <summary>
        /// Login to Linkedin using cookies 
        /// </summary>
        /// <param name="cookies"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void Login(Cookie[] cookies)
        {
            _linkedinNavigator.Navigate(LinkedinNavigatorOptions.Feed);
            
            foreach (var cookie in cookies)
            {
                _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value, cookie.Domain,
                    cookie.Path, DateTime.Now.AddDays(365), cookie.Secure, cookie.HttpOnly, "None"));
            }

            if (!IsUserLoggedIn()) throw new UnauthorizedAccessException("Failed to login");
        }
        
        /// <summary>
        /// Login to Linkedin using username and password
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private void Login(string userName, string password)
        {
            if (IsUserLoggedIn()) return;
            
           _linkedinNavigator.Navigate(LinkedinNavigatorOptions.LoginPage);

           var loginUserName = FindElement(By.Id("username"));
            
            loginUserName.SendKeys(userName);

            var loginPass = FindElement(By.Id("password"));
            
            loginPass.SendKeys(password);
            
            //aria-label="Sign in"
            var loginButton = FindElement(By.CssSelector("[aria-label='Sign in']"));
            
            loginButton.Click();
            
            if (!IsUserLoggedIn()) throw new UnauthorizedAccessException("Failed to login");
        }
        
        /// <summary>
        /// Login to Linkedin using cookies from file
        /// </summary>
        /// <param name="cookiesSourceFilePath"></param>
        private void Login(string cookiesSourceFilePath = "cookies.txt")
        {
            var cookies = ImportCookiesFromFile(cookiesSourceFilePath);

            Login(cookies);
        }
        
        /// <summary>
        /// Import cookies from file to Cookie[]  
        /// </summary>
        /// <param name="cookiesSourceFilePath"></param>
        /// <returns></returns>
        private Cookie[] ImportCookiesFromFile(string cookiesSourceFilePath)
        {
            var cookiesStr = File.ReadAllText(cookiesSourceFilePath);
            var cookies = JsonConvert.DeserializeObject<Cookie[]>(cookiesStr);
            return cookies;
        }
        
        /// <summary>
        /// Navigate to Linkedin page 
        /// </summary>
        /// <param name="option"></param>
        public void Navigate(LinkedinNavigatorOptions option)
            => _linkedinNavigator.Navigate(option);
        
        /// <summary>
        /// Publish post to Linkedin 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Returns the post uri</returns>
        public Uri PublishPost(string text)
        {
            _linkedinNavigator.Navigate(LinkedinNavigatorOptions.Feed);
        
            Thread.Sleep(new Random().Next(1000,2000));
            
            var startPostButton = FindElement(".share-box-feed-entry__trigger",10);

            startPostButton.Click();
            
            Thread.Sleep(new Random().Next(1000,2000));
            
            var postTextArea = FindElement( "[aria-label='Text editor for creating content']", 10);

            Thread.Sleep(new Random().Next(1000,2000));
            
            postTextArea.SendKeys(text);

            // wait until the click ^ action will be done!
            Thread.Sleep(new Random().Next(500,1000));
            
            var postButton = FindElement(".share-actions__primary-action", 10);
            
            postButton.Click();
            
            // wait until the click ^ action will be done!
            Thread.Sleep(new Random().Next(500,1000));
            
            var toastMsg = FindElement( ".artdeco-toast-item__cta", 10);
            
            var postUri = toastMsg.GetAttribute("href"); // postUri
            
            return new Uri(postUri); 
        }
        
        /// <summary>
        /// Get post reactions count 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="postUri"></param>
        /// <returns></returns>
        public int GetPostReactionsCount(IWebDriver driver, string postUri)
        {
            var reactionCountElement = FindElement(".social-details-social-counts__reactions-count", 10);
            
            int.TryParse(reactionCountElement?.Text, out var reactionCount);
            
            return reactionCount;
        }

        /// <summary>
        /// Add comment to post 
        /// </summary>
        /// <param name="postUri"></param>
        /// <returns></returns>
        public Uri AddCommentToPost(Uri postUri, string commentText)
        {
            _linkedinNavigator.Navigate(postUri);
            
            var cssSelector = "[aria-label='Text editor for creating content']";

            var commentBox = FindElement(cssSelector, retrySeconds: 2);
            
            commentBox.Click();
            commentBox.SendKeys(commentText);
            
            var postButton = FindElement("[aria-label*='Post comment on']", retrySeconds: 2);
            postButton.Click();

            Thread.Sleep(1000);

            var commentActionMenu = FindElements(".comment-options-trigger")?.FirstOrDefault();
            commentActionMenu.Click();

            var commentActions = FindElement(By.CssSelector(".artdeco-dropdown__content-inner"));

            var copyLinkButton = commentActions.FindElement(By.TagName("ul")).FindElements(By.TagName("li"))
                .FirstOrDefault(x=>x.Text.StartsWith("Copy link to comment", StringComparison.InvariantCultureIgnoreCase));
            
            copyLinkButton.Click();

            var toastMsg = FindElement( ".artdeco-toast-item__cta", 10);
            
            toastMsg.Click();

            return new Uri(_driver.Url);
            
            var commentUri = toastMsg.GetAttribute("href"); // postUri
            
            return new Uri(commentUri); 
        }

        /// <summary>
        /// Get unread notifications count 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public int GetUnreadNotificationsCount()
        {
            _linkedinNavigator.Navigate(LinkedinNavigatorOptions.Feed);
            
            var notificationBadge = FindElement(".notification-badge__count", 10);

            var unreadNotificationCountStr = notificationBadge.Text?.Trim();

            int.TryParse(unreadNotificationCountStr, out var unreadNotificationCount);

            return unreadNotificationCount;
        }
        
        public int GetUnseenMessagesCount()
        {
            var unseenMessagesCount = 0;
            
            try
            {
                var element = FindElement("[title*='unseen message']", retrySeconds: 10);
                var str = element?.Text?.Trim();
                
                int.TryParse(str, out unseenMessagesCount);
            }
            catch (Exception)
            {
                // ignored
            }

            return unseenMessagesCount;
        }

        /// <summary>
        /// Check if user is connected (A friend) with the current user 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsConnectedUser(string userId)
        {
            var isConnectedUser = false;
            
            _linkedinNavigator.NavigateToUserProfile(userId);
            
            try
            {
                 isConnectedUser =
                    FindElements(By.CssSelector("[aria-label*='Invite']"))?
                        .FirstOrDefault(x => x.GetCssValue("class").Contains("pvs-profile-actions__action"))
                    == null;
            }
            catch (Exception)
            {
                // ignored
            }

            return isConnectedUser;
        }

        /// <summary>
        /// Send message to user 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        public void SendMessageToUser( 
            string userId, 
            string message)
        {
            _linkedinNavigator.NavigateToUserProfile(userId);
            
            var messageButton = FindElement(cssSelector: "[aria-label*='Message ']", retrySeconds:5);
            
            ExecuteJavaScript("arguments[0].scrollIntoView(true);", messageButton);

            Thread.Sleep(500);

            ExecuteJavaScript("arguments[0].click();", messageButton);

            Thread.Sleep(1000);

            var messageTextArea = FindElement("[aria-label='Write a message…']", retrySeconds:5);
            
            messageTextArea.SendKeys(message);

            var buttonSendMessage =
                FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                    x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));

            buttonSendMessage.Click();

            // wait until the click ^ action will be done!
            FindElement("[aria-label='Write a message…']", retrySeconds: 10);

            // close the message box
            var buttonCloseConversation = FindElements(By.CssSelector(".artdeco-button__text"))
                ?.FirstOrDefault(x =>
                    x.Text.StartsWith("Close your conversation", StringComparison.InvariantCultureIgnoreCase));

            ExecuteJavaScript("arguments[0].click();", buttonCloseConversation);
        }

        /// <summary>
        /// Send user invite to connect
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="inviteNote"></param>
        /// <returns></returns>
        public bool SendConnectionRequest(string userId, string inviteNote)
        {
            _linkedinNavigator.NavigateToUserProfile(userId);

            if (IsUserConnectionRequestPending(userId)) return true;
            
            // connect
            var buttonConnect =
                FindElement(By.CssSelector("[aria-label*='Invite']"), retrySeconds: 10);

            ExecuteJavaScript("arguments[0].click();", buttonConnect);

            try
            {
                //Invitation not sent to the user. You can resend an invitation 3 weeks after withdrawing it.
                var withdrawingStatus =
                    FindElement(By.CssSelector(".artdeco-toast-item__message"), retrySeconds:10);

                if (withdrawingStatus != null)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // add a note
            var buttonAddANote =
                FindElement(By.CssSelector("[aria-label='Add a note']"), retrySeconds: 10);
            
            ExecuteJavaScript("arguments[0].scrollIntoView(true);", buttonAddANote);
            
            Thread.Sleep(500);
            
            ExecuteJavaScript("arguments[0].click();", buttonAddANote);
                
            Thread.Sleep(1000);
            
            // write and send a note with the connection request 
            var textAreaNote =
                FindElement(By.CssSelector("[name='message']"), retrySeconds:10);
            
            textAreaNote.SendKeys(inviteNote);
            
            var buttonSendNote =
                FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                    x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));
            
            buttonSendNote.Click();
            
            return true; 
        }

        /// <summary>
        /// Check if user invite status is pending 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUserConnectionRequestPending(string userId)
        {
            _linkedinNavigator.NavigateToUserProfile(userId);
            
            try
            {
                // check if the user is already connected
                var pendingBtnExists = FindElement(By.CssSelector("[aria-label*='Pending']"), retrySeconds: 2);
                if (pendingBtnExists != null) // user is waiting for connection approval 
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return false;
        }

        public void Search(string searchQuery)
        {
            var searchPageTemplate = "https://www.linkedin.com/search/results/all/?keywords=#{searchCriteria}&origin=GLOBAL_SEARCH_HEADER";
            
            _linkedinNavigator.Navigate(searchPageTemplate.Replace("#{searchCriteria}", searchQuery));
            
            // _linkedinNavigator.Navigate(LinkedinNavigatorOptions.Feed);
            //
            // var searchInput = FindElement(By.CssSelector("[aria-label='Search']"), retrySeconds: 10);
            //
            // searchInput.SendKeys(searchQuery);
            //
            // var searchButton = FindElement(By.CssSelector("[aria-label='Search']"), retrySeconds: 10);
            //
            // searchButton.Click();
        }

        public void LikePost(Uri postUri, PostReaction reaction = PostReaction.Like)
        {
            _linkedinNavigator.Navigate(postUri);

            try
            {
                var reactionMenu = FindElement(By.CssSelector(".reactions-react-button"), retrySeconds: 2);
                reactionMenu.Click();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}