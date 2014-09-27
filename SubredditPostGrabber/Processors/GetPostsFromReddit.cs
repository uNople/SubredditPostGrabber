using RedditSharp;
using RedditSharp.Things;
using SubredditPostGrabber.Holders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubredditPostGrabber.Utils;

namespace SubredditPostGrabber.Processors
{
    public class GetPostsFromReddit
    {
        public List<MattPost> AllPosts { get; set; }
        public string SaveDirectory { get; set; }
        public string Subreddit { get; set; }
        public string TemporarySaveFileFormatString { get { return Subreddit + "-{0}--{1}--{2}.xml"; } }
        public string SaveFileFormatString { get { return Subreddit + "_{0}_{1}.xml"; } }
        public string DateTimeFormatString = "yyyy-MM-dd_HH-mm-ss";

        public GetPostsFromReddit(string saveDirectory, string subreddit)
        {
            AllPosts = new List<MattPost>();
            SaveDirectory = saveDirectory;
            Subreddit = subreddit;
        }

        public void LoadPostsFromRedditAndSave()
        {
            LoadPostsFromRedditViaEnum();
            LoadPostsFromRedditViaSearching();
            LoadPostsByAuthor();
        }
        
        #region Utility
        /// <summary>
        /// Converts the reddit sharp post into a MattPost
        /// </summary>
        public MattPost ConvertRedditSharpPostIntoMattPost(Post post)
        {
            return new MattPost
            {
                Description = post.SelfText,
                URL = post.Url.ToString(),
                Title = post.Title,
                Votes = post.Upvotes,
                Id = post.Id,
                CommentCount = post.CommentCount,
                IsSelfPost = post.IsSelfPost,
                PostDate = post.CreatedUTC,
                Author = post.AuthorName,
                Tag = post.LinkFlairText
            };
        }
        #endregion

        #region Get Posts Via Search
        public void LoadPostsFromRedditViaSearching()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
            foreach (var letter in chars)
            {
                var posts = new List<MattPost>();
                var started = DateTime.Now.ToString(DateTimeFormatString);
                int counter = 0;
                //1000 is all we can get :(
                var postCount = 1000;
                var lastCounterValue = counter;
                var numTimesStayedSame = 0;
                //postCount.Dump("Total number of Posts we are going to try to get!");
                while (counter < postCount)
                {
                    lastCounterValue = counter;

                    var reddit = new Reddit();
                    var subreddit = reddit.GetSubreddit(Subreddit);

                    foreach (var post in subreddit.GetAllViaSearch().Skip(counter).Take(100))
                    {
                        try
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(TemporarySaveFileFormatString, started, letter, counter)), posts);
                    if (lastCounterValue == counter)
                    {
                        numTimesStayedSame++;
                    }
                    //If we've not changed the amount we're up to 4 times in a row, break out of here.
                    if (numTimesStayedSame > 4)
                    {
                        break;
                    }
                }
                SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(SaveFileFormatString, letter, started)), posts);
            }
        }
        #endregion

        #region Get Posts Via Enum
        /// <summary>
        /// Loads the posts from reddit via enum.
        /// It gets posts by iterating through the enum and getting posts based off that
        /// </summary>
        public void LoadPostsFromRedditViaEnum()
        {
            foreach (GetBy getBy in (GetBy[])Enum.GetValues(typeof(GetBy)))
            {
                var posts = new List<MattPost>();
                var started = DateTime.Now.ToString(DateTimeFormatString);
                int counter = 0;
                //1000 is all we can get :(
                var postCount = 1000;
                var lastCounterValue = counter;
                var numTimesStayedSame = 0;

                while (counter < postCount)
                {
                    lastCounterValue = counter;
                    counter = GetPosts(counter, getBy, posts, out posts);
                    SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(TemporarySaveFileFormatString, started, getBy, counter)), posts);
                    if (lastCounterValue == counter)
                    {
                        numTimesStayedSame++;
                    }
                    //If we've not changed the amount we're up to 4 times in a row, break out of here.
                    if (numTimesStayedSame > 4)
                    {
                        break;
                    }
                }
                SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(SaveFileFormatString, getBy, started)), posts);
            }
        }

        /// <Summary>
        /// Gets Posts from reddit. Limited by 100 so we don't fall afoul of the timeout
        /// The counter is what we're up to in the collection of Posts.
        /// </Summary>
        /// <param name="counter">the number of Posts to skip before parsing the next 100 Posts</param>
        /// <param name="getBy">An enum which shows what to call in the Reddit API to get these Posts</param>
        private int GetPosts(int counter, GetBy getBy, List<MattPost> inPosts, out List<MattPost> posts)
        {
            posts = inPosts;
            try
            {
                var reddit = new Reddit();
                var subreddit = reddit.GetSubreddit(Subreddit);

                switch (getBy)
                {
                    case GetBy.GetAllViaSearch:
                        foreach (var post in subreddit.GetAllViaSearch().Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.GetTopAll:
                        foreach (var post in subreddit.GetTop(FromTime.All).Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.GetTopMonth:
                        foreach (var post in subreddit.GetTop(FromTime.Month).Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.GetTopYear:
                        foreach (var post in subreddit.GetTop(FromTime.Year).Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.Hot:
                        foreach (var post in subreddit.Hot.Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.New:
                        foreach (var post in subreddit.New.Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                    case GetBy.Posts:
                        foreach (var post in subreddit.Posts.Skip(counter).Take(100))
                        {
                            counter++;
                            var mattPost = ConvertRedditSharpPostIntoMattPost(post);
                            posts.Add(mattPost);
                            Console.WriteLine("{0} - {1}", counter, mattPost);
                        }
                        break;
                }
                return counter;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return counter;
            }
        }
        #endregion

        #region Get Posts By Author
        /// <summary>
        /// Loads in the directory of posts first.
        /// Builds a list of all authors who've posted to this subreddit
        /// Goes through those authors and gets all posts to that subreddit.
        /// Saves to the SaveDir all posts by that author.
        /// </summary>
        public void LoadPostsByAuthor()
        {
            var posts = SaveLoadData.LoadDirectoryOfPosts(SaveDirectory, "*", SearchOption.TopDirectoryOnly);
            var allAuthors = posts
                    .Select(x => new { Author = x.Author, Post = x })
                    .GroupBy(x => x.Author)
                    .Select(x => new { Author = x.Key, Count = x.Count() })
                    .OrderBy(x => x.Count);

            var authorPosts = new List<MattPost>();
            var started = DateTime.Now.ToString(DateTimeFormatString);
            var reddit = new Reddit();

            foreach (var authorPost in allAuthors)
            {
                try
                {
                    var user = reddit.GetUser(authorPost.Author);
                    foreach (var post in user.Posts.Where(x => x.Subreddit.CIEqual(Subreddit)))
                    {
                        var mattPost = new MattPost
                        {
                            Description = post.SelfText,
                            URL = post.Url.ToString(),
                            Title = post.Title,
                            Votes = post.Upvotes,
                            Id = post.Id,
                            CommentCount = post.CommentCount,
                            IsSelfPost = post.IsSelfPost,
                            PostDate = post.CreatedUTC,
                            Author = post.AuthorName,
                            Tag = post.LinkFlairText
                        };
                        authorPosts.Add(mattPost);
                        Console.WriteLine("Added Author " + mattPost.Author + " post " + mattPost.Title);
                    }
                    Console.WriteLine("Got Posts for Author " + authorPost.Author);
                    SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(TemporarySaveFileFormatString, "Authors", started, posts.Count)), authorPosts);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception when getting author {0}'s posts. Exception: {1}", authorPost.Author, ex);
                }
            }
            SaveLoadData.SavePosts(Path.Combine(SaveDirectory, string.Format(SaveFileFormatString, "Authors", started)), authorPosts);
        }
        #endregion
    }
}
