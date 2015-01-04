using OfficeOpenXml;
using SubredditPostGrabber.Holders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;

namespace SubredditPostGrabber.Utils
{
    public enum DateFormat
    {
        Normal,
        NormalWithDayOfWeek
    }

    public static class SpreadsheetHelper
    {
        /// <summary>
        /// Creates a spreadsheet for all OC posts last week.
        /// Creates 3 worksheets, one sorted by votes, comment count, and date
        /// </summary>
        /// <param name="posts">The posts.</param>
        /// <param name="spreadsheetSaveDir">The spreadsheet save dir.</param>
        /// <returns></returns>
        public static string CreateSpreadsheetForOCPostsLastWeek(List<MattPost> posts, string spreadsheetSaveDir)
        {
            var ocPosts = posts
                .Where(x => x.GetTag().CIEqual("OC")).ToList();

            return CreateSpreadsheetForPostsLastWeek(ocPosts, spreadsheetSaveDir);
        }

        /// <summary>
        /// Creates a spreadsheet for posts last week.
        /// Creates 3 worksheets, one sorted by votes, comment count, and date
        /// </summary>
        /// <param name="posts">The posts.</param>
        /// <param name="spreadsheetSaveDir">The spreadsheet save dir.</param>
        /// <returns></returns>
        public static string CreateSpreadsheetForPostsLastWeek(List<MattPost> posts, string spreadsheetSaveDir)
        {
            var thisMonday = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var lastMonday = thisMonday.AddDays(-7);

            var lastWeek = posts
                .Where(x => x.PostDate >= lastMonday && x.PostDate < thisMonday);

            var lastWeekByVotes = lastWeek
                .OrderByDescending(x => x.Votes)
                .ToList();

            var lastWeekByDate = lastWeek
                .OrderBy(x => x.PostDate)
                .ToList();

            var lastWeekByCommentCount = lastWeek
                .OrderByDescending(x => x.CommentCount)
                .ToList();

            var fileName = "HFY Top posts for week starting " + lastMonday.ToString("yyyy-MM-dd");
            var path = SpreadsheetHelper.CreateSpreadsheet(spreadsheetSaveDir, lastWeekByVotes, fileName, "Top by Votes", DateFormat.NormalWithDayOfWeek);
            SpreadsheetHelper.CreateSpreadsheet(spreadsheetSaveDir, lastWeekByCommentCount, fileName, "Top by Comment Count", DateFormat.NormalWithDayOfWeek, true);
            SpreadsheetHelper.CreateSpreadsheet(spreadsheetSaveDir, lastWeekByDate, fileName, "Sorted by Date", DateFormat.NormalWithDayOfWeek, true);
            
            return path;
        }

        /// <summary>
        /// Creates a spreadsheet for all posts in a year.
        /// </summary>
        /// <param name="posts">The posts.</param>
        /// <param name="spreadsheetSaveDir">The spreadsheet save dir.</param>
        /// <param name="subredditName">Name of the subreddit.</param>
        /// <param name="year">The year.</param>
        /// <returns>The path to the spreadsheet created</returns>
        public static string CreateSpreadsheetForPostsInAYear(List<MattPost> posts, string spreadsheetSaveDir, string subredditName, string year = null)
        {
            if (year == null)
            {
                year = DateTime.Now.ToString("yyyy");
            }

            var yearStartDate = DateTime.ParseExact(year + "-01-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var postsFromThisYear = posts
                .Where(x => x.PostDate >= yearStartDate && x.PostDate < yearStartDate.AddYears(1)).ToList();
            return SpreadsheetHelper.CreateSpreadsheetWithWorksheetPerTag(postsFromThisYear, spreadsheetSaveDir, subredditName + " " + year);
        }

        /// <summary>
        /// Creates the spreadsheet with a worksheet per tag.
        /// </summary>
        /// <param name="posts">The posts.</param>
        /// <param name="spreadsheetSaveDir">The spreadsheet save dir.</param>
        /// <param name="fileNameWithoutExtension">The file name without extension.</param>
        /// <returns>The full path to the created spreadsheet</returns>
        public static string CreateSpreadsheetWithWorksheetPerTag(List<MattPost> posts, string spreadsheetSaveDir, string fileNameWithoutExtension)
        {
            var allTags = posts.GroupBy(x => x.GetTag().ToUpper())
                .Select(x => new { Tag = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Force the OC flair to be at the start.
            var ocTag = allTags.FirstOrDefault(x => x.Tag.CIEqual("OC"));
            if (ocTag != null)
            {
                allTags.Remove(ocTag);
                allTags.Insert(0, ocTag);    
            }

            string path = "";

            foreach (var tag in allTags) // .Where(x=>x.Count > 70)
            {
                if (tag.Equals(allTags.First()))
                {
                    path = SpreadsheetHelper.CreateSpreadsheet(spreadsheetSaveDir, posts.Where(x => x.GetTag().CIEqual(tag.Tag)).ToList(), fileNameWithoutExtension, tag.Tag, DateFormat.Normal, allTags.First() == tag ? false : true);
                }
                else
                {
                    SpreadsheetHelper.CreateSpreadsheet(spreadsheetSaveDir, posts.Where(x => x.GetTag().CIEqual(tag.Tag)).ToList(), fileNameWithoutExtension, tag.Tag, DateFormat.Normal, allTags.First() == tag ? false : true);
                }
            }

            return path;
        }

        /// <summary>
        /// Creates the spreadsheet.
        /// </summary>
        /// <param name="saveDirectory">The save directory.</param>
        /// <param name="posts">The posts.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="workSheetTitle">The work sheet title.</param>
        /// <param name="dateTimeFormat">The date time format.</param>
        /// <param name="appendToExisting">if set to <c>true</c> [append to existing].</param>
        /// <returns>The full path to the spreadsheet created</returns>
        public static string CreateSpreadsheet(string saveDirectory, List<MattPost> posts, string fileName, string workSheetTitle, DateFormat dateTimeFormat, bool appendToExisting = false)
        {
            var dateTimeFormatString = "";
            if (dateTimeFormat == DateFormat.Normal)
            {
                dateTimeFormatString = "yyyy-MM-dd HH:mm";
            }
            if (dateTimeFormat == DateFormat.NormalWithDayOfWeek)
            {
                dateTimeFormatString = "dddd, yyyy-MM-dd HH:mm";
            }

            var headerRow = new List<string> { "Author", "Votes", "CommentCount", "Title", "PostDate", "URL", "FuzzyWikiThing" };
            var newFile = new FileInfo(Path.Combine(saveDirectory, fileName + ".xlsx"));

            if (posts.Count == 0)
                return ""; //can't do shit with 0 posts

            if (!appendToExisting)
            {
                Console.WriteLine("Creating worksheet {0} in new spreadsheet {1}", workSheetTitle, newFile.ToString());
                File.Delete(newFile.ToString());
            }
            else
            {
                Console.WriteLine("Appending worksheet {0} to spreadsheet {1}", workSheetTitle, newFile.ToString());
            }

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(workSheetTitle);

                var rowNumber = 1;
                for (int i = 1; i <= headerRow.Count; i++)
                {
                    //Add the headers
                    worksheet.Cells[rowNumber, i].Value = headerRow[i - 1];
                    worksheet.Cells[rowNumber, i].Style.Font.Bold = true;
                }

                if (!package.Workbook.Styles.NamedStyles.Any(x => x.Name.CIEqual("HyperLink")))
                {
                    var hyperlinkStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");   //This one is language dependent
                    hyperlinkStyle.Style.Font.UnderLine = true;
                    hyperlinkStyle.Style.Font.Color.SetColor(Color.Blue);
                }

                foreach (var post in posts)
                {
                    rowNumber++;

                    worksheet.Cells[rowNumber, 1].Value = post.Author;
                    worksheet.Cells[rowNumber, 2].Value = post.Votes;
                    worksheet.Cells[rowNumber, 3].Value = post.CommentCount;
                    worksheet.Cells[rowNumber, 4].Value = post.Title;
                    worksheet.Cells[rowNumber, 5].Value = post.PostDate;
                    worksheet.Cells[rowNumber, 6].Value = post.URL;
                    worksheet.Cells[rowNumber, 6].Hyperlink = new Uri(post.URL);
                    worksheet.Cells[rowNumber, 6].StyleName = "HyperLink";
                    worksheet.Cells[rowNumber, 7].Value = string.Format("[{0}]({1}) [{2}](/r/HFY/wiki/authors/{2})", post.Title, post.URL, post.Author);
                }

                worksheet.Cells["E2:E" + rowNumber.ToString()].Style.Numberformat.Format = dateTimeFormatString; //Format as text 
                worksheet.Calculate();
                worksheet.Cells.AutoFitColumns(0);  //Autofit columns for all cells

                package.Save();
            }

            return newFile.ToString();
        }
    }
}
