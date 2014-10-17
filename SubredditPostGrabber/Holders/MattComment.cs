using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Holders
{
    public class MattComment
    {
        public string Body { get; set; }
        public string Author { get; set; }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string Shortlink { get; set; }

        public MattComment() {} // needed for serialization

        public MattComment(Comment comment)
        {
            Body = comment.Body;
            Author = comment.Author;
            Id = comment.Id;
            ParentId = comment.ParentId;
            Shortlink = comment.Shortlink;
        }
    }
}
