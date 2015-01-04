using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Utils
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Foreach extension for IEnumerables, so we don't need to go .ToList().Foreach
        /// </summary>
        /// <param name="values">The IEnumerable you are passing in</param>
        /// <param name="action">The action you are taking (linq expression)</param>
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var v in values)
            {
                action(v);
            }
        }

        /// <summary>
        /// Checks if a string contains another string, but case insensitive
        /// </summary>
        /// <param name="str">The string you are passing in</param>
        /// <param name="secondString">The string you want to find in the first string</param>
        public static bool ContainsCI(this string str, string secondstring)
        {
            return str.ToUpper().Contains(secondstring.ToUpper());
        }

        /// <summary>
        /// Checks if a string is equal to another string, but case insensitive
        /// </summary>
        /// <param name="str">The string you are passing in</param>
        /// <param name="secondString">The string you want to check equality with</param>
        public static bool CIEqual(this string str, string secondstring)
        {
            return string.Equals(str, secondstring, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Takes in a path, search pattern and options. Lets you foreach over these and fails gracefully even if you don't have permission
        /// 
        /// This is to get a list of files, given a path, with everything in the directory output as IENumerable so you can foreach over it
        /// </summary>
        /// <param name="path">The top level directory you want to search</param>
        /// <param name="searchPattern">The windows search string of files you want to get back (eg, *filename*.ext)</param>
        /// <param name="searchOpt">This is either top level only or all subdirs</param>
        /// <returns></returns>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();

                //If we're searching all dirs, then call itself again
                if (searchOpt == SearchOption.AllDirectories)
                {
                    //Enumerate separately for each directory in this directory.
                    dirFiles = Directory.EnumerateDirectories(path).SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }

                //Return the list of files in this directory
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (Exception)
            {
                //when there's an exception return nothing
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the very start of a day
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Midnight on the day you enter in</returns>
        public static DateTime StartOfDay(this DateTime date)
        {
            return date.Date;
        }

        /// <summary>
        /// Gets the very end of the datetime you enter
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>23:59:59.999 on the date</returns>
        public static DateTime EndOfDay(this DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime StartOfHour(this DateTime date)
        {
            return DateTime.ParseExact(date.ToString("yyyy-MM-dd HH"), "yyyy-MM-dd HH", CultureInfo.InvariantCulture);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }
}
