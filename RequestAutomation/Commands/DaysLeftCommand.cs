using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RequestAutomation.Commands
{
    public class DaysLeftCommand : ICommand
    {
        private readonly IRepository repo;

        private readonly IEmailSender emailSender;
        public string CommandName { get => Constants.CommandNames.DaysLeftCommand; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public DaysLeftCommand(IRepository repo, IEmailSender emailSender)
        {
            this.repo = repo;
            this.emailSender = emailSender;
        }
        public Response Execute()
        {
            throw new NotImplementedException();
        }

        public Response Execute(DaysOff daysOff, string sender)
        {
            User user = this.repo.GetUserByEmail(sender);
            if (user == null)
            {
                throw new InexistentUserException("Userul cu mailul " + sender + " nu exista in baza de date. Trebuie inserat manual!");
            }
            try
            {
                Response response = new Response();
                response.MailSubject = this.ResponseMailSubject;
                response.User = user;
                //possible attachments
                //validations
                string body = this.CommandBodyApproved;

                int nrDays = this.repo.GetFreeDays(user.UserId);

                body = body.Replace("<days>", nrDays.ToString());
                response.MailBody = body;
                emailSender.SendEmail(sender, response.MailBody, response.MailSubject, new List<string>());
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(this.CommandBodyRejected + '\n' + ex.Message);
            }
        }

        public Response Execute(string body, string email)
        {
            throw new NotImplementedException();
        }

        public Response Execute(RemoteWork remoteWork, string email)
        {
            throw new NotImplementedException();
        }
    }
}
