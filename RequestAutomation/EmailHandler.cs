using Common;
using Common.Entities;
using Common.Interfaces;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestAutomation
{
    public class EmailHandler : IEmailHandler
    {
        private readonly EmailConfiguration options;
        public readonly ITokenizer tokenizer;
        private readonly IEmailSender emailSender;
        public EmailHandler(IOptions<EmailConfiguration> options, ITokenizer tokenizer, IEmailSender emailSender)
        {
            this.options = options.Value;
            this.tokenizer = tokenizer;
            this.emailSender = emailSender;
        }

        public void ReadEmail()
        {
            using (var client = new ImapClient())
            {
                client.Connect(options.SMTPhost, Convert.ToInt32(options.IMAPport), true);
                client.Authenticate(options.AddressFrom, options.Password);
                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadWrite);

                Console.WriteLine("Total messages: {0}", inbox.Count);
                Console.WriteLine("Recent messages: {0}", inbox.Recent);
                Console.WriteLine("Unread emails:");

                foreach(var i in inbox.Search(MailKit.Search.SearchQuery.NotSeen))
                {
                    var message = inbox.GetMessage(i);
                    Console.WriteLine("Subject: {0}", message.Subject);
                    Console.WriteLine("BODY: {0}", message.TextBody);
                    Console.WriteLine(message.From.OfType<MailboxAddress>().Single().Address);
                    string sender = message.From.OfType<MailboxAddress>().Single().Address;
                    string error = "";
                    Response response = new Response();
                    try
                    {
                        response = this.tokenizer.GetCommand(message);
                        //this.emailSender.SendEmail(sender, response.MailBody, response.MailSubject, new List<string>());
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                        string subject = "Rejected response " + message.Subject;
                        string body = error;
                        List<string> attach = new List<string>();
                        /*if (error == "Date interval given may already be requested or duplicate!")
                        {
                            attach.Add("Template-CerereDeConcediu.docx");
                        }*/

                        this.emailSender.SendEmail(sender, body, subject, attach);
                    }
                    //  SendEmail(sender, response, error);
                    inbox.AddFlags(i, MailKit.MessageFlags.Seen, true);
                }
                client.Disconnect(true);
            }
        }
    }
}
