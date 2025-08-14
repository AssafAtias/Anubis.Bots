using System;
using System.IO;
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
            // IWebDriver driver = new ChromeDriver();

            // get current directory path
            var currentDirectoryPath = Directory.GetCurrentDirectory();

            // Cross-platform path handling
            var debugPath = Path.Combine("bin", "Debug");
            var releasePath = Path.Combine("bin", "Release");

            if (currentDirectoryPath.EndsWith(debugPath))
                currentDirectoryPath = currentDirectoryPath.Replace(debugPath, "");
            else if (currentDirectoryPath.EndsWith(releasePath))
                currentDirectoryPath = currentDirectoryPath.Replace(releasePath, "");

            var extensionPath =
                Path.Combine(currentDirectoryPath, "ChromeExtensions", "pgojnojmmhpofjgdmaebadhbocahppod");

            var options = new ChromeOptions();
            options.AddArguments("load-extension=" + extensionPath);

            try
            {
                IWebDriver driver = new ChromeDriver(options);
                driver.Manage().Window.Maximize();

                var userName =
                    "assaf.atias@gmail.com"; //"neta931test@gmail.com"; // "neta931test931@gmail.com"; //"neta931test@gmail.com";
                var password = "Assaf@3490";

                var linkedinDriver = new LinkedinDriver(driver, userName, password);

                linkedinDriver.ExportPageCookiesToFile();
                linkedinDriver.RequestToConnectPeopleByGroupUrl(new Uri("https://www.linkedin.com/groups/2548564/"),
                    10);
                linkedinDriver.LikeOnPostByHashtag("#dotnet", 10);

                driver.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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