using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.IO;

namespace Thread_OS
{
    class Program
    {
        static void Main(string[] args)
        {
            
            PathBrowserUserData pathBrowserUserData = new PathBrowserUserData("config.json");
            ChromeOptions options = new ChromeOptions
            {
                BinaryLocation = pathBrowserUserData.BrowserPath,
                LeaveBrowserRunning = true
            };
            options.AddArgument(pathBrowserUserData.User_DataPath);
            ChromeDriver chromeDriver = new ChromeDriver(options);
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed");

            IWebElement feed_rows = null;
            foreach( IWebElement webElement in chromeDriver.FindElementsById("feed_rows").ToList())
            {
                if (!webElement.Displayed)
                    continue;
                feed_rows = webElement;
                break;
            }
            if (feed_rows == null)
                return;



            List<ID_Text_Post> iD_Text_Posts = new List<ID_Text_Post>();
            List<ID_Href_Or_Image_Post> iD_Href_Posts = new List<ID_Href_Or_Image_Post>();
            List<ID_Href_Or_Image_Post> iD_Image_Posts = new List<ID_Href_Or_Image_Post>();
            string id_post;
            foreach (IWebElement feed_row in feed_rows.FindElements(By.TagName("div")))
            {
                if (!feed_row.Displayed)
                    continue;
                if (feed_row.GetAttribute("class") == null)
                    continue;
                if (!feed_row.GetAttribute("class").Trim().Equals("feed_row"))
                    continue;
                if (feed_row.FindElement(By.TagName("div")).GetAttribute("id") == "")   
                    continue;
                id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");

                try
                {
                    iD_Text_Posts.Add(new ID_Text_Post(id_post, feed_row.FindElement(By.ClassName("wall_post_text")).Text));
                }
                catch (Exception)
                {
                    iD_Text_Posts.Add(new ID_Text_Post(id_post, null));
                }

                iD_Href_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "a", "href"));

                iD_Image_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "img", "src", "a", "aria-label"));
            }
            using ( StreamWriter file = new StreamWriter("ID_Text_Posts.json"))
            {
                file.WriteLine(JsonConvert.SerializeObject(iD_Text_Posts, Formatting.Indented));
                file.Flush();
                file.Close();
                file.Dispose();
            }
            using (StreamWriter file = new StreamWriter("ID_Href_Posts.json"))
            {
                file.WriteLine(JsonConvert.SerializeObject(iD_Text_Posts, Formatting.Indented));
                file.Flush();
                file.Close();
                file.Dispose();
            }
            using (StreamWriter file = new StreamWriter("ID_Image_Posts.json"))
            {
                file.WriteLine(JsonConvert.SerializeObject(iD_Text_Posts, Formatting.Indented));
                file.Flush();
                file.Close();
                file.Dispose();
            }
        }
    }
}
