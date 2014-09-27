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
        /// <param name="savePath">The path you want to save this into</param>
        public static void SavePosts(string savePath, List<MattPost> posts)
        {
            var xs = new XmlSerializer(typeof(List<MattPost>));

            using (var wr = new StreamWriter(savePath))
            {
                xs.Serialize(wr, posts);
            }
            Console.WriteLine("Saved posts into file {0}", savePath);
        }

        /// <Summary>
        /// Deserialises the saved XML into a List{MattPost}
        /// Will throw an exception if there's an issue
        /// </Summary>
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
    }
}
