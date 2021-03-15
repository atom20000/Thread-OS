using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thread_OS
{
    class ID_Href_Or_Image_Post
    {
        public string Id = string.Empty;
        public List<string> HrefOrImage = new List<string>();
        /// <summary>
        /// Принимает объект новости и по заданным тегам и атрибутам распарсивает ссылки из новости 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feed_row"></param>
        /// <param name="Tag"></param>
        /// <param name="Attribute"></param>
        public ID_Href_Or_Image_Post(string id,IWebElement feed_row, string Tag, string Attribute)
        {
            this.Id = id;
            this.Image_Href_Parse(feed_row, Tag, Attribute);
        }
        /// <summary>
        /// Принимает объект новости и по заданным тегам и атрибутам распарсивает ссылки на картинки из новости  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="feed_row"></param>
        /// <param name="Tag_1"></param>
        /// <param name="Attribute_1"></param>
        /// <param name="Tag_2"></param>
        /// <param name="Attribute_2"></param>
        public ID_Href_Or_Image_Post(string id, IWebElement feed_row, string Tag_1, string Attribute_1, string Tag_2, string Attribute_2)
        {
            this.Id = id;
            this.Image_Href_Parse(feed_row, Tag_1, Attribute_1);
            this.Image_Parse(feed_row, Tag_2, Attribute_2);
        }
        public ID_Href_Or_Image_Post() { }
        private void Image_Href_Parse(IWebElement feed_row, string Tag, string Attribute)=>
            this.HrefOrImage.AddRange(from f in feed_row.FindElements(By.TagName(Tag))
                                      where !(f.GetAttribute(Attribute) == null || f.GetAttribute(Attribute) == "")
                                      && !this.HrefOrImage.Exists(p => p.Equals(f.GetAttribute(Attribute)))
                                      select f.GetAttribute(Attribute));
        private void Image_Parse(IWebElement feed_row, string Tag, string Attribute)=>
            this.HrefOrImage.AddRange(from f in feed_row.FindElements(By.TagName(Tag))
                                      where !(f.GetAttribute(Attribute) == null || f.GetAttribute(Attribute) == "")
                                      && f.GetAttribute(Attribute).Equals("фотография")
                                      let url = f.GetAttribute("style").Substring(f.GetAttribute("style").IndexOf('"') + 1)
                                      where !this.HrefOrImage.Exists(p => p.Equals(url.Remove(url.IndexOf('"'))))
                                      select url.Remove(url.IndexOf('"')));
    }
}
