using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SubredditPostGrabber.Utils
{
    public static class EmailHelper
    {
        /// <summary>
        /// Sends an email via gmail
        /// </summary>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the email.</param>
        /// <param name="attachments">List of file paths of attachments you want to send</param>
        /// <param name="toAddresses">Comma separated list of email addresses to email this to</param>
        public static void SendEmailViaGmail(string subject, string body, List<string> attachments, string toAddresses, string GmailFromAddress, string GmailFromPassword)
        {
            var message = new MailMessage(GmailFromAddress, toAddresses, subject, body);
            foreach (var attachment in attachments)
            {
                message.Attachments.Add(new Attachment(attachment));
            }

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(GmailFromAddress, GmailFromPassword)
            })
            {
                // The below two links you need to do on the gmail account you are sending from
                // https://g.co/allowaccess
                // https://www.google.com/settings/security/lesssecureapps
                smtp.Send(message);
            }
        }
    }
}
