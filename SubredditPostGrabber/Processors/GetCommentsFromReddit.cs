using RedditSharp;
using SubredditPostGrabber.Holders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Processors
{
    public class GetCommentsFromReddit
    {
        public string PostsDirectory { get; set; }
        public string SubredditName { get; set; }
        public string DateTimeFormatString = "yyyy-MM-dd_HH-mm-ss";
        public string SaveFile { get; set; }
        
        public GetCommentsFromReddit(string postsDirectory, string subredditName)
        {
            PostsDirectory = postsDirectory;
            SubredditName = subredditName;
            SaveFile = Path.Combine(PostsDirectory, string.Format("{0}Comments_{1}.xml", SubredditName, DateTime.Now.ToString(DateTimeFormatString)));
        }

        public void GetCommentsForAllPosts()
        {
            var posts = SaveLoadData.LoadDirectoryOfPosts(PostsDirectory, SubredditName + "_*", SearchOption.TopDirectoryOnly);
            var comments = new List<MattComment>();

            foreach (var post in posts)
            {
                var reddit = new Reddit();
                try
                {
                    var postData = reddit.GetPost(new Uri(post.URL));

                    int commentCount = 0;
                    foreach (var comment in postData.Comments)
                    {
                        comments.Add(new MattComment(comment));
                        commentCount++;
                    }
                    Console.WriteLine("Got {0} comments for post titled '{1}'", commentCount, postData.Title);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problem when getting comments for post {0}. Stacktrace: {1}", post.Title, ex.ToString());
                }
                SaveLoadData.SaveComments(SaveFile, comments);
            }
        }
    }
}
