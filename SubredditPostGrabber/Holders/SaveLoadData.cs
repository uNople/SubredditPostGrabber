﻿using SubredditPostGrabber.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SubredditPostGrabber.Holders
{
    public static class SaveLoadData
    {
        #region Save/load posts
        /// <Summary>
        /// Serialises the post object into XML and saves it to disk.
        /// This will throw an exception if it fails, so watch out for that
        /// </Summary>
        /// <param name="savePath">The path you want to save this into. This is c:\path\file.xml, not just c:\path</param>
        /// <param name="posts">The posts you want to save.</param>
        public static void SavePosts(string savePath, List<MattPost> posts)
        {
            var xs = new XmlSerializer(typeof(List<MattPost>));

            using (var wr = new StreamWriter(savePath))
            {
                xs.Serialize(wr, posts);
            }
            Console.WriteLine("Saved posts into file {0}", savePath);
        }

        /// <summary>
        /// Deserialises the saved XML into a List{MattPost}
        /// Will throw an exception if there's an issue
        /// </summary>
        /// <param name="fileName">Name of the file you want to load.</param>
        /// <returns>The file as a List{MattPost}</returns>
        public static List<MattPost> LoadPosts(string fileName)
        {
            var xs = new XmlSerializer(typeof(List<MattPost>));

            using (var rd = new StreamReader(fileName))
            {
                // This little beauty here strips out all un-readable Xml characters like in the comment above.
                // We can deserialise with confidence knowing that we're not going to run into parse errors.
                var xmlTextReader = new XmlTextReader(rd) { Normalization = false };

                return xs.Deserialize(xmlTextReader) as List<MattPost>;
            }
        }

        /// <summary>
        /// Loads all posts from the specified directory of posts and returns one list.
        /// This is so we can go through all the saved posts we got at certain times and
        /// amalgamate them into one nice list
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="pattern">The search pattern. This is a windows one, not a regex, so one like *.xml or HFY_*_*.xml.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A List{MattPost}</returns>
        public static List<MattPost> LoadDirectoryOfPosts(string directoryName, string pattern, SearchOption searchOptions)
        {
            var posts = new List<MattPost>();
            foreach (var file in ExtensionMethods.EnumerateFiles(directoryName, pattern, searchOptions))
            {
                try
                {
                    // This is why this is in a try/catch - if any files are attempted to be
                    // deserialized that aren't the right format it will blow up.
                    var newPosts = SaveLoadData.LoadPosts(file);
                    int nonUnique = 0;
                    foreach (var post in newPosts)
                    {
                        if (!posts.Any(x => x.Id.Equals(post.Id)))
                        {
                            posts.Add(post);
                        }
                        else
                        {
                            var existingPost = posts.FirstOrDefault(x => x.Id.Equals(post.Id));
                            if (post.Votes > existingPost.Votes)
                            {
                                existingPost.Votes = post.Votes;
                            }
                            nonUnique++;
                        }
                    }
                    Console.WriteLine("File {0} had {1} non-unique posts. That's {2} unique posts.", file, nonUnique, newPosts.Count - nonUnique);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File {0} is likely not the right format. Stacktrace: {1}", file, ex);
                }
            }
            return posts;
        }
        #endregion

        #region Save/load comments
        /// <Summary>
        /// Serialises the comment object into XML and saves it to disk.
        /// This will throw an exception if it fails, so watch out for that
        /// </Summary>
        /// <param name="savePath">The path you want to save this into. This is c:\path\file.xml, not just c:\path</param>
        /// <param name="comments">The comments you want to save.</param>
        public static void SaveComments(string savePath, List<MattComment> comments)
        {
            var xs = new XmlSerializer(typeof(List<MattComment>));

            using (var wr = new StreamWriter(savePath))
            {
                xs.Serialize(wr, comments);
            }
            Console.WriteLine("Saved {0} comments into file {1}", comments.Count, savePath);
        }

        /// <summary>
        /// Deserialises the saved XML into a List{MattComment}
        /// Will throw an exception if there's an issue
        /// </summary>
        /// <param name="fileName">Name of the file you want to load.</param>
        /// <returns>The file as a List{MattComment}</returns>
        public static List<MattComment> LoadComments(string fileName)
        {
            var xs = new XmlSerializer(typeof(List<MattComment>));

            using (var rd = new StreamReader(fileName))
            {
                // This little beauty here strips out all un-readable Xml characters like in the comment above.
                // We can deserialise with confidence knowing that we're not going to run into parse errors.
                var xmlTextReader = new XmlTextReader(rd) { Normalization = false };

                return xs.Deserialize(xmlTextReader) as List<MattComment>;
            }
        }

        /// <summary>
        /// Loads all comments from the specified directory of comments and returns one list.
        /// This is so we can go through all the saved comments we got at certain times and
        /// amalgamate them into one nice list
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="pattern">The search pattern. This is a windows one, not a regex, so one like *.xml or HFYComments_*_*.xml.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A List{MattComment}</returns>
        public static List<MattComment> LoadDirectoryOfComments(string directoryName, string pattern, SearchOption searchOptions)
        {
            var comments = new List<MattComment>();
            foreach (var file in ExtensionMethods.EnumerateFiles(directoryName, pattern, searchOptions))
            {
                try
                {
                    // This is why this is in a try/catch - if any files are attempted to be
                    // deserialized that aren't the right format it will blow up.
                    var newComments = SaveLoadData.LoadComments(file);
                    int nonUnique = 0;
                    foreach (var comment in newComments)
                    {
                        // If the comment doesn't already exist
                        if (!comments.Any(x => x.Id.Equals(comment.Id)))
                        {
                            comments.Add(comment);
                        }
                        else
                        {
                            // Check if the comment exists already. If the author or comment is [deleted]
                            // then restore what we used to have, if we have it.
                            var oldComment = comments.Where(x => x.Id.Equals(comment.Id)).FirstOrDefault();

                            if (oldComment != null)
                            {
                                // If the old comment has [deleted], but the new comment doesn't, then update it
                                if (oldComment.Author.CIEqual("[deleted]") && !comment.Author.CIEqual("[deleted"))
                                {
                                    oldComment.Author = comment.Author;
                                }
                                if (oldComment.Body.CIEqual("[deleted]") && !comment.Body.CIEqual("[deleted"))
                                {
                                    oldComment.Body = comment.Body;
                                }
                            }

                            if (oldComment.Upvotes < comment.Upvotes)
                            {
                                oldComment.Upvotes = comment.Upvotes;
                            }

                            nonUnique++;
                        }
                    }
                    Console.WriteLine("File {0} had {1} non-unique comments. That's {2} unique comments.", file, nonUnique, newComments.Count - nonUnique);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File {0} is likely not the right format. Stacktrace: {1}", file, ex);
                }
            }
            return comments;
        }
        #endregion

        #region Save / Load Subreddit Data
        /// <Summary>
        /// Serialises the post object into XML and saves it to disk.
        /// This will throw an exception if it fails, so watch out for that
        /// </Summary>
        /// <param name="savePath">The path you want to save this into. This is c:\path\file.xml, not just c:\path</param>
        /// <param name="subreddit">The posts you want to save.</param>
        public static void SaveSubreddit(string savePath, MattSubreddit subreddit)
        {
            var xs = new XmlSerializer(typeof(MattSubreddit));

            using (var wr = new StreamWriter(savePath))
            {
                xs.Serialize(wr, subreddit);
            }
            Console.WriteLine("Saved {0} data into file {1}", subreddit.Name, savePath);
        }

        /// <summary>
        /// Deserialises the saved XML into a List{MattPost}
        /// Will throw an exception if there's an issue
        /// </summary>
        /// <param name="fileName">Name of the file you want to load.</param>
        /// <returns>The file as a List{MattPost}</returns>
        public static MattSubreddit LoadSubreddit(string fileName)
        {
            var xs = new XmlSerializer(typeof(MattSubreddit));

            using (var rd = new StreamReader(fileName))
            {
                // This little beauty here strips out all un-readable Xml characters like in the comment above.
                // We can deserialise with confidence knowing that we're not going to run into parse errors.
                var xmlTextReader = new XmlTextReader(rd) { Normalization = false };

                return xs.Deserialize(xmlTextReader) as MattSubreddit;
            }
        }

        /// <summary>
        /// Loads all posts from the specified directory of posts and returns one list.
        /// This is so we can go through all the saved posts we got at certain times and
        /// amalgamate them into one nice list
        /// </summary>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="pattern">The search pattern. This is a windows one, not a regex, so one like *.xml or HFY_*_*.xml.</param>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>A List{MattSubreddit}</returns>
        public static List<MattSubreddit> LoadDirectoryOfSubredditData(string directoryName, string pattern, SearchOption searchOptions)
        {
            var subreddits = new List<MattSubreddit>();
            foreach (var file in ExtensionMethods.EnumerateFiles(directoryName, pattern, searchOptions))
            {
                try
                {
                    // This is why this is in a try/catch - if any files are attempted to be
                    // deserialized that aren't the right format it will blow up.
                    subreddits.Add(SaveLoadData.LoadSubreddit(file));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File {0} is likely not the right format. Stacktrace: {1}", file, ex);
                }
            }
            return subreddits;
        }
        #endregion
    }
}
