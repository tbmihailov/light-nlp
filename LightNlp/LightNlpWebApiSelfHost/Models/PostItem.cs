using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNlpWebApiSelfHost.Models
{

    public class PostData
    {
        public string PostId { get; set; }

        public string User { get; set; }
        public DateTime? PostDate { get; set; }

        public string ContentText { get; set; }

        public Dictionary<string, string> AdditionalFields { get; set; }

        //public string MoodString { get; set; }

        //public string Username { get; set; }

        //public int NumberInPub { get; set; }

        //public int CommentsCountInPub { get; set; }

        //public bool IsReply { get; set; }
        //public string Label { get; set; }
    }

}
