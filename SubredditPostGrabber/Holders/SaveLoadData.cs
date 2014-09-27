using SubredditPostGrabber.Utils;
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
    }
}
