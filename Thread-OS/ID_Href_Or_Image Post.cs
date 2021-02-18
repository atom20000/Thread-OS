using System;
using System.Collections.Generic;
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
    }
}
