using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Thread_OS
{
    class ID_Href_Or_Image_Post
    {
        public string Id;
        public List<string> HrefOrImage;
        public ID_Href_Or_Image_Post(string id, List<string> hrefOrImage)
        {
            this.Id = id;
            this.HrefOrImage = new List<string>(hrefOrImage);
        }
        public void Image_Parse(IWebElement feed_row, string Tag, string Attribute)
        {
            foreach (IWebElement image in feed_row.FindElements(By.TagName(Tag)).ToList())
            {
                if (image.GetAttribute(Attribute) == null || image.GetAttribute(Attribute) == "")
                    continue;
                if (this.HrefOrImage.Exists(p => p.Equals(image.GetAttribute(Attribute))))
                    continue;
                this.HrefOrImage.Add(image.GetAttribute(Attribute));
            }
        }
    }
}
