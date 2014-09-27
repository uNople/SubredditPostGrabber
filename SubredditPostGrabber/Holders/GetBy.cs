using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Holders
{
    /// <summary>
    /// Holds what we're getting the posts by. This is an enum which should 
    /// cover basically most scenarios except search which we cover elsewhere
    /// </summary>
    public enum GetBy
    {
        Posts,
        New,
        Hot,
        GetAllViaSearch,
        GetTopAll,
        GetTopYear,
        GetTopMonth,
    }
}
