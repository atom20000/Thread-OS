using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace Thread_OS
{
    class Program
    {
        static void Main(string[] args)
        {
            PathBrowserUserData pathBrowserUserData = new PathBrowserUserData("config.json");
            ChromeOptions options = new ChromeOptions
            {
                BinaryLocation = pathBrowserUserData.BrowserPath
            };
            options.AddArgument(pathBrowserUserData.User_DataPath);
            ChromeDriver chromeDriver = new ChromeDriver(options);
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
        }
    }
}
