using System;
using System.Collections.ObjectModel;
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

            if (_driver.Url.Contains("checkpoint/challenge"))
            {
                WaitForCapchaSolver();
            }
            
            if (!IsUserLoggedIn()) throw new UnauthorizedAccessException("Failed to login");
        }

        private void WaitForCapchaSolver()
        {
            var start = DateTime.UtcNow;
            do
            {
                Thread.Sleep(1000);
            } while (_driver.Url.Contains("checkpoint/challenge") && (DateTime.UtcNow - start).TotalSeconds < 60);
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
        /// <param name="postUri"></param>
        /// <returns></returns>
        public int GetPostReactionsCount(Uri postUri)
        {
            _linkedinNavigator.Navigate(postUri);
            
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
        public bool SendConnectionRequest(
            string userId, 
            string inviteNote = "Hey, I would like to increase my network and connect with you. I hope you don't mind. Thanks!")
        {
            _linkedinNavigator.NavigateToUserProfile(userId);

            if (IsUserConnectionRequestPending(userId)) return true;
            
            // clean notifications from the page
            CleanBadgeNotification();
            
            var moreActionsButton = FindElement("[aria-label='More actions']", retrySeconds: 5, false);

            if (moreActionsButton != null)
            {
                ExecuteJavaScript("arguments[0].click();", moreActionsButton);

                RandomizeThreadSleep(1000, 1500);
            }

            // connect
            var buttonConnect =
                FindElement(By.CssSelector("[aria-label*='Invite']"), retrySeconds: 5, throwExceptionIfNotFound: true);

            ExecuteJavaScript("arguments[0].click();", buttonConnect);

            try
            {
                //Invitation not sent to the user. You can resend an invitation 3 weeks after withdrawing it.
                var withdrawingStatus =
                    FindElement(By.CssSelector(".artdeco-toast-item__message"), retrySeconds:5);

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
            
            RandomizeThreadSleep(500,1000);
            
            ExecuteJavaScript("arguments[0].click();", buttonAddANote);
              
            RandomizeThreadSleep(1000,2000);

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

        private void CleanBadgeNotification()
        {
            try
            {
                var notificationElem = FindElement("[aria-label*='Dismiss']", retrySeconds: 1, false);
                if (notificationElem != null)
                {
                    ExecuteJavaScript("arguments[0].click();", notificationElem);
                }
            }
            catch (Exception)
            {
                // ignored
            }
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

        public void LikeOnPost(Uri postUri, PostReaction reaction = PostReaction.Like)
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
        
        /// <summary>
        /// Request to join group by group uri 
        /// </summary>
        /// <param name="groupUri"></param>
        /// <exception cref="NotImplementedException"></exception>
       public void RequestToJoinGroup(Uri groupUri)
       {
           _linkedinNavigator.Navigate(groupUri);
           
           // var joinButton = FindElement(By.CssSelector(".artdeco-button--primary"), retrySeconds: 10);
           //
           // joinButton.Click();
           
           throw new NotImplementedException();
       }
        
       /// <summary>
       /// Connect with people by hashtag 
       /// </summary>
       /// <param name="hashtag"></param>
       /// <param name="numOfPeopleToConnect"></param>
       /// <exception cref="ArgumentNullException"></exception>
       /// <exception cref="ArgumentOutOfRangeException"></exception>
       /// <exception cref="NotImplementedException"></exception>
       public void RequestToConnectPeopleByHashtag(
           string hashtag, 
           uint numOfPeopleToConnect = 10, 
           string inviteNote = "Hey, I would like to increase my network and connect with you. I hope you don't mind. Thanks!")
       {
           if (hashtag == null) throw new ArgumentNullException(nameof(hashtag));
           if (numOfPeopleToConnect <= 0) throw new ArgumentOutOfRangeException(nameof(numOfPeopleToConnect));
              
           _linkedinNavigator.ViewPeopleByHashtag(hashtag);

           RandomizeThreadSleep();
           
           var maxLoop = 100;
           var invited = 0;
           do
           {
               // connect
               var connectButtons =
                   FindElements(By.CssSelector("[aria-label*='Invite']"));

               if (connectButtons == null || !connectButtons.Any())
               {
                   var elem = FindElement("[aria-label='Next']", retrySeconds: 10, false);
               
                   if (elem == null || elem.GetAttribute("disabled") != null) break;
               
                   // scroll to next page
                   elem.Click();
               
                   RandomizeThreadSleep();
                   continue;
               }

               IWebElement[] selectedConnectButtons; 

               if (connectButtons.Count > numOfPeopleToConnect - invited)
                   selectedConnectButtons = connectButtons.Take((int)numOfPeopleToConnect- invited).ToArray();
               else if (connectButtons.Count > numOfPeopleToConnect)
                   selectedConnectButtons = connectButtons.Take((int)numOfPeopleToConnect).ToArray();
               else
                   selectedConnectButtons = connectButtons.ToArray();
               
               invited += InviteToConnect(selectedConnectButtons, inviteNote);
               
               RandomizeThreadSleep();
               
           } while (invited < numOfPeopleToConnect
                    && maxLoop-- > 0);
       }

       private int InviteToConnect(IWebElement[] selectedConnectButtons, string inviteNote)
       {
           var invited = 0;
           
           foreach (var buttonConnect in selectedConnectButtons)
           {
              var successInvite = InviteToConnect(inviteNote, buttonConnect);

              if(successInvite)
                invited++;
              
               RandomizeThreadSleep(500, 3500);
           }
           
           return invited; 
       }

       private bool InviteToConnect(string inviteNote, IWebElement buttonConnect)
       {
           try
           {
               ExecuteJavaScript("arguments[0].scrollIntoView(true);", buttonConnect);
               
               RandomizeThreadSleep(500, 1000);
               
               ExecuteJavaScript("arguments[0].click();", buttonConnect);
               
               // add a note
               var buttonAddANote =
                   FindElement(By.CssSelector("[aria-label='Add a note']"), retrySeconds: 10);

               ExecuteJavaScript("arguments[0].scrollIntoView(true);", buttonAddANote);

               RandomizeThreadSleep(500, 1000);

               ExecuteJavaScript("arguments[0].click();", buttonAddANote);

               RandomizeThreadSleep(1000, 2000);

               // write and send a note with the connection request 
               var textAreaNote =
                   FindElement(By.CssSelector("[name='message']"), retrySeconds: 10);

               textAreaNote.SendKeys(inviteNote);

               var buttonSendNote =
                   FindElements(By.CssSelector(".artdeco-button"))?.FirstOrDefault(x =>
                       x.Text.StartsWith("Send", StringComparison.InvariantCultureIgnoreCase));

               buttonSendNote.Click();
           }
           catch (Exception)
           {
               return false;
           }

           return true;
       }

       /// <summary>
       /// Like on post by hashtag 
       /// </summary>
       /// <param name="hashtag"></param>
       /// <param name="numOfPostsToLike"></param>
       /// <exception cref="ArgumentNullException"></exception>
       /// <exception cref="ArgumentOutOfRangeException"></exception>
       /// <exception cref="NotImplementedException"></exception>
       public void LikeOnPostByHashtag(string hashtag, uint numOfPostsToLike = 10)
       {
           if (hashtag == null) throw new ArgumentNullException(nameof(hashtag));
           if (numOfPostsToLike <= 0) throw new ArgumentOutOfRangeException(nameof(numOfPostsToLike));
           
           _linkedinNavigator.ViewPostsByHashtag(hashtag);

           var likeButtons = GetPostsLikeButton();

           var js = (IJavaScriptExecutor)_driver;

           var maxLoop = 1000;
           var likedPosts = 0;
           
           var lastHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
           
           while (likedPosts < numOfPostsToLike 
                  && maxLoop-- > 0)
           {
               ExecuteJavaScript("window.scrollTo(0, document.body.scrollHeight);");
       
               RandomizeThreadSleep();
       
               likeButtons = GetPostsLikeButton();

               if (likeButtons.Length == 0)
                   continue;
           
               foreach (var likeButton in likeButtons)
               {
                   var likeClicked = Click(likeButton);

                   if (!likeClicked)
                       continue;
               
                   likedPosts++;

                   if (likedPosts >= numOfPostsToLike)
                       break;
                   
                   RandomizeThreadSleep();
               }
           
               var newHeight = (long)js.ExecuteScript("return document.body.scrollHeight");

               if (newHeight == lastHeight || likedPosts >= numOfPostsToLike)
                   break; // Break the loop if scroll height hasn't changed or if we reached the max liked posts
           
               lastHeight = newHeight;
           }
       }
       
       /// <summary>
       /// Follow people by hashtag 
       /// </summary>
       /// <param name="hashtag"></param>
       /// <param name="numOfPeopleToFollow"></param>
       public void FollowByHashtag(string hashtag, int numOfPeopleToFollow = 10)
       {
           _linkedinNavigator.ViewPostsByHashtag(hashtag);
           
           var followButtons = FindElements("[aria-label*='Follow']");
           
           var js = (IJavaScriptExecutor)_driver;

           var maxLoop = 1000;
           var followed = 0;
           
           var lastHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
           
           while (followed < numOfPeopleToFollow 
                  && maxLoop-- > 0)
           {
               ExecuteJavaScript("window.scrollTo(0, document.body.scrollHeight);");
       
               RandomizeThreadSleep();
       
               followButtons = FindElements("[aria-label*='Follow']");

               if (followButtons.Count == 0)
                   continue;
           
               foreach (var likeButton in followButtons)
               {
                   var clicked = Click(likeButton);

                   if (!clicked)
                       continue;
               
                   followed++;

                   if (followed >= numOfPeopleToFollow)
                       break;
                   
                   RandomizeThreadSleep();
               }
           
               var newHeight = (long)js.ExecuteScript("return document.body.scrollHeight");

               if (newHeight == lastHeight || followed >= numOfPeopleToFollow)
                   break; // Break the loop if scroll height hasn't changed or if we reached the max liked posts
           
               lastHeight = newHeight;
           }
       }

       /// <summary>
       /// Randomize thread sleep 
       /// </summary>
       /// <param name="min"></param>
       /// <param name="max"></param>
       private static void RandomizeThreadSleep(int min = 750, int max = 2500)   
        => Thread.Sleep(new Random().Next(min, max));

       /// <summary>
       /// Get posts like button 
       /// </summary>
       /// <returns></returns>
       private IWebElement[] GetPostsLikeButton()
        => FindElements("[aria-label*='React Like']")
               .Where(x => x.GetAttribute("aria-label")
                   .EndsWith("post", StringComparison.InvariantCultureIgnoreCase))
               .ToArray();

       private bool Click(IWebElement button)
       {
           try
           {
               ExecuteJavaScript("arguments[0].focus();", button);
               RandomizeThreadSleep();
               
               button.Click();

               return true;
           }
           catch (Exception)
           {
               // ignored
           }
           
           return false;
       }

       /// <summary>
       /// Like on post by user id 
       /// </summary>
       /// <param name="userId"></param>
       /// <param name="numOfPostsToLike"></param>
       /// <exception cref="ArgumentNullException"></exception>
       /// <exception cref="ArgumentOutOfRangeException"></exception>
       /// <exception cref="NotImplementedException"></exception>
       public void LikeOnPostByUserId(string userId, uint numOfPostsToLike = 10)
       {
           if (userId == null) throw new ArgumentNullException(nameof(userId));
           if (numOfPostsToLike <= 0) throw new ArgumentOutOfRangeException(nameof(numOfPostsToLike));
           
           _linkedinNavigator.NavigateToUserProfile(userId);
           
           throw new NotImplementedException();
       }
       
    }
}