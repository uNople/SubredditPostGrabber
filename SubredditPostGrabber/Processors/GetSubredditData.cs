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
    public class GetSubredditData
    {
        public string DateTimeFormatString = "yyyy-MM-dd_HH-mm-ss";
        public string SubredditName { get; set; }
        public string SaveDirectory { get; set; }

        public GetSubredditData(string saveDirectory, string subredditName)
        {
            SaveDirectory = saveDirectory;
            SubredditName = subredditName;
        }

        public MattSubreddit GetData()
        {
            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit(SubredditName);
            var scrapedAt = DateTime.Now;
            var mattSubreddit = new MattSubreddit
            {
                ActiveUsers = subreddit.ActiveUsers.Value
                , Name = subreddit.Name
                , Created = subreddit.Created.Value
                , Subscribers = subreddit.Subscribers.Value
                , TimeScraped = scrapedAt
                , Title = subreddit.Title
                , Url = subreddit.Url.ToString()
            };
            SaveLoadData.SaveSubreddit(Path.Combine(SaveDirectory, string.Format("{0}SubredditData_{1}.xml", SubredditName, scrapedAt.ToString(DateTimeFormatString))), mattSubreddit);
            return mattSubreddit;
        }
    }
}
