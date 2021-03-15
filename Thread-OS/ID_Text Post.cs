using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
namespace Thread_OS
{
    class ID_Text_Post
    {
        public string Id;
        public string Text;
        public ID_Text_Post(string id, string text)
        {
            this.Id = id;
            this.Text = text;
        }
        public ID_Text_Post() { }
        //public string ID_TextPostToJson() =>
        //    JsonConvert.SerializeObject(this);

    }
}
