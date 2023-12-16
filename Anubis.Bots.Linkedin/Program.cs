using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace Anubis.Bots.Linkedin
{
    internal class Program
    {
        // static string searchCriteria = "Assaf Atias";
        // static string searchPage = "https://www.linkedin.com/search/results/all/?keywords=#{searchCriteria}&origin=GLOBAL_SEARCH_HEADER";
        //
        public static void Main(string[] args)
        {
            // Initialize the Chrome WebDriver
            IWebDriver driver = new ChromeDriver();
            
            driver.Manage().Window.Maximize();

            // var userName = "neta931test931@gmail.com"; //"neta931test@gmail.com";
            // var password = "bcfhF.xe28!jdY8";
            
            var linkedinDriver = new LinkedinDriver(driver, "cookies.txt"); 
            
            // linkedinDriver.Navigate(LinkedinNavigatorOptions.ViewProfile);
           
            // var postUri = linkedinDriver.PublishPost("postttt" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //
            // linkedinDriver.Navigate(LinkedinNavigatorOptions.Feed);

            var postUri = new Uri("https://www.linkedin.com/feed/update/urn:li:share:7141863247749533696");
            
            linkedinDriver.LikeOnPost(postUri, PostReaction.Like);
             
            //
            // var commentUri = linkedinDriver.AddCommentToPost(postUri, commentText: "commentttt" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //

            // var unseenMessagesCount = linkedinDriver.GetUnseenMessagesCount();
            // var unreadNotificationsCount = linkedinDriver.GetUnreadNotificationsCount();
            //
            // var userId = "assaf-atias-84099832";
            // // userId = "netanel-abergel";
            //
            // if(!linkedinDriver.IsConnectedUser(userId) && !linkedinDriver.IsUserConnectionRequestPending(userId))
            //     linkedinDriver.SendConnectionRequest(userId, "messageee" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            // else if(!linkedinDriver.IsUserConnectionRequestPending(userId))
            //     linkedinDriver.SendMessageToUser(userId, "messageee" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //
            
            
            
            // Close the browser after task completion
            driver.Quit();
        }


        private static void DummyMouseMovement(IWebDriver driver)
        {
            // Create an instance of the Actions class
            var actions = new Actions(driver);

            var rnd = new Random(0);

            // Perform dummy mouse movements
            // You can chain multiple actions together
            actions
                .MoveByOffset(rnd.Next(0, 100),
                    rnd.Next(0, 100)) // Move the mouse to a specific offset (x, y) from the current position
                .MoveByOffset(rnd.Next(0, 100), 0) // Move right by 100 pixels
                .MoveByOffset(0, rnd.Next(0, 100)) // Move down by 100 pixels
                .MoveByOffset(-rnd.Next(0, 100), 0) // Move left by 100 pixels
                .MoveByOffset(0, -rnd.Next(0, 100)) // Move up by 100 pixels
                .Perform(); // Perform all the actions

            // Scroll down by a certain number of pixels (e.g., 500 pixels)
            actions.MoveByOffset(0, 500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);

            // Scroll up by a certain number of pixels (e.g., -500 pixels)
            actions.MoveByOffset(0, -500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);

            // Thread.Sleep(rnd.Next(500, 5000)); // Sleep for a random amount of time (1-5 retrySeconds) to simulate a human

            // Perform dummy mouse movements
            // You can chain multiple actions together
            actions
                .MoveByOffset(rnd.Next(0, 100),
                    rnd.Next(0, 100)) // Move the mouse to a specific offset (x, y) from the current position
                .MoveByOffset(rnd.Next(0, 100), 0) // Move right by 100 pixels
                .MoveByOffset(0, rnd.Next(0, 100)) // Move down by 100 pixels
                .MoveByOffset(-rnd.Next(0, 100), 0) // Move left by 100 pixels
                .MoveByOffset(0, -rnd.Next(0, 100)) // Move up by 100 pixels
                .Perform(); // Perform all the actions

            // Scroll down by a certain number of pixels (e.g., 500 pixels)
            actions.MoveByOffset(0, 500).Perform();

            // Wait for a moment to see the scroll effect (you can adjust the duration)
            Thread.Sleep(1000);

            // Scroll up by a certain number of pixels (e.g., -500 pixels)
        }
        
    }
}
