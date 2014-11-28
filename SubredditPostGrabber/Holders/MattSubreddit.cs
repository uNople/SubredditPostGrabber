using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Holders
{
    public class MattSubreddit
    {
        /// <summary>
        /// Gets or sets the title of the Subreddit
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets the name of the subreddit
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to the subreddit
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the number of subscribers to this subreddit
        /// </summary>
        public int Subscribers { get; set; }

        /// <summary>
        /// Gets or sets the active users of this subreddit
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Gets or sets the time this subreddit was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the time the subreddit data was gotten.
        /// </summary>
        public DateTime TimeScraped { get; set; }
    }
}
