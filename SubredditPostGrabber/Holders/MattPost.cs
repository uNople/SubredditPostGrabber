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

        /// <Summary>
        /// Gets the first tag out of the title of a post.
        /// Passing in "[oc] my first post, please be gentle" will return "OC"
        /// If there's no tag in the title of the post itself it returns the tag.
        /// </Summary>
        /// <param name="title">the title of the post</param>
        public string GetTag()
        {
            //If there's no tag in the post itself
            if (string.IsNullOrEmpty(Tag))
            {
                // Does the ] come after [, and does [ exist?
                if (Title.IndexOf(']') > Title.IndexOf('[') && Title.IndexOf('[') > -1)
                {
                    return string.Join("", Title.ToCharArray().SkipWhile(x => x != '[').Skip(1).TakeWhile(x => x != ']')).ToUpper();
                }
                return "NO TAG";
            }
            else
            {
                return Tag;
            }
        }

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
