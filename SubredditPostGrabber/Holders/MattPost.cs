using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Holders
{
    public class MattPost
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public int Votes { get; set; }
        public int CommentCount { get; set; }
        public bool IsSelfPost { get; set; }
        public DateTime PostDate { get; set; }
        public string Author { get; set; }
        public string Tag { get; set; }

        public string ToRedditString()
        {
            return string.Format
            (
                "{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7}",
                Votes,
                Tag,
                Title,
                Description.Length,
                CommentCount,
                IsSelfPost,
                PostDate,
                Author
            );
        }

        public override string ToString()
        {
            return string.Format
            (
                "({0}) [{1}]{2} - [DL: {3}] [CC: {4}] [Self:{5}] [UTC:{6}] by {7}",
                Votes,
                Tag,
                Title,
                Description.Length,
                CommentCount,
                IsSelfPost,
                PostDate,
                Author
            );
        }
    }

}
