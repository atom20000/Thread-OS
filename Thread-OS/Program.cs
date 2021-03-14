﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace Thread_OS
{
    class Program
    {
        List<ID_Text_Post> iD_Text_Posts = new List<ID_Text_Post>();
        List<ID_Href_Or_Image_Post> iD_Href_Posts = new List<ID_Href_Or_Image_Post>();
        List<ID_Href_Or_Image_Post> iD_Image_Posts = new List<ID_Href_Or_Image_Post>();
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
            foreach (IWebElement webElement in chromeDriver.FindElementsById("feed_rows").ToList())
            {
                if (!webElement.Displayed)
                    continue;
                feed_rows = webElement;
                break;
            }
            if (feed_rows == null)
                return;
            List<IWebElement> feed_row_list = (from f in feed_rows.FindElements(By.TagName("div"))
                                               where f.Displayed &&
                                               !(f.GetAttribute("class") == null) &&
                                               f.GetAttribute("class").Trim().Equals("feed_row") &&
                                               !(f.FindElement(By.TagName("div")).GetAttribute("id") == "")
                                               select f).ToList();

            //Thread thread_Text_file = new Thread(() => WriteinFile("ID_Text_Posts.json", iD_Text_Posts))
            //{
            //    Name = "File_ID_Text"
            //};
            //thread_Text_file.Start();
            //Thread thread_Href_file = new Thread(() => WriteinFile("ID_Href_Posts.json", iD_Href_Posts))
            //{
            //    Name = "File_ID_Href"
            //};
            //thread_Href_file.Start();

            //Thread thread_Image_file = new Thread(() => WriteinFile("ID_Image_Posts.json", iD_Image_Posts))
            //{
            //    Name = "File_ID_Image"
            //};
            //thread_Image_file.Start();

        }
        private static void WriteinFile<T>(string path, List<T> objects)
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                file.WriteLine(JsonConvert.SerializeObject(objects, Formatting.Indented));
                file.Flush();
                file.Close();
                file.Dispose();
            }
        }
        private static void ReadinFile<T>(string path, out List<T> objects)
        {
            using (StreamReader file = new StreamReader(path))
            {
                objects = JsonConvert.DeserializeObject<List<T>>(file.ReadToEnd());
                file.Close();
                file.Dispose();
            }
        }
        private Thread Thread_Text(List<IWebElement> feed_row_list) =>
            new Thread(() =>
            {
                ReadinFile<ID_Text_Post>("ID_Text_Posts.json", out iD_Text_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    try
                    {
                        iD_Text_Posts.Add(new ID_Text_Post(id_post, feed_row.FindElement(By.ClassName("wall_post_text")).Text));
                    }
                    catch (Exception)
                    {
                        iD_Text_Posts.Add(new ID_Text_Post(id_post, null));
                    }
                }
            })
            { Name = "Sort_Text" };
        private Thread Thread_Href(List<IWebElement> feed_row_list) =>
            new Thread(() =>
            {
                ReadinFile<ID_Href_Or_Image_Post>("ID_Href_Posts.json", out iD_Href_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    iD_Href_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "a", "href"));
                }

            })
            { Name = "Sort_Href" };
        private Thread Thread_Image(List<IWebElement> feed_row_list) =>
            new Thread(() =>
            {
                ReadinFile<ID_Href_Or_Image_Post>("ID_Href_Posts.json", out iD_Image_Posts);
                string id_post;
                foreach (IWebElement feed_row in feed_row_list)
                {
                    id_post = feed_row.FindElement(By.TagName("div")).GetAttribute("id");// FindElement(By.TagName("div")) можно выкинуть
                    iD_Image_Posts.Add(new ID_Href_Or_Image_Post(id_post, feed_row, "img", "src", "a", "aria-label"));
                }

            })
            { Name = "Sort_Image" };
    }
}
