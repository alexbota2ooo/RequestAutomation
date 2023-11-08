using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using MailKit.Net.Imap;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;


namespace RequestAutomation
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration options;
        public EmailSender(IOptions<EmailConfiguration> options)
        {
            this.options = options.Value;

        }
        public void SendEmail(string clientEmail, string mailBody, string mailSubject, List<string> attachments)
        {
            using (var imapclient = new ImapClient())
            {
                imapclient.Connect(options.SMTPhost, Convert.ToInt32(options.IMAPport), true);
                imapclient.Authenticate(options.AddressFrom, options.Password);

                var client = new SmtpClient(options.SMTPhost, Convert.ToInt32(options.SMTPport))
                {
                    Credentials = new NetworkCredential(options.AddressFrom, this.options.Password),
                    EnableSsl = true
                };
                try
                {
                    var message = new MailMessage(options.AddressFrom, clientEmail);

                    message.Subject = mailSubject;
                    message.Body = string.Format(mailBody, Environment.NewLine);

                    foreach (var item in attachments)
                    {
                        message.Attachments.Add(new Attachment(item));
                    }
                    client.Send(message);
                    Console.WriteLine("Sent");
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                imapclient.Disconnect(true);
            }
        }

    }
}
