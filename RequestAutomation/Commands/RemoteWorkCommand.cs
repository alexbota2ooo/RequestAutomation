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
    public class RemoteWorkCommand : ICommand
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;
        public string CommandName { get => Constants.CommandNames.RemoteWorkCommand; }

        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public RemoteWorkCommand(IRepository repository, IEmailSender emailSender)
        {
            this.repo = repository;
            this.emailSender = emailSender;
        }

        public Response Execute()
        {
            throw new NotImplementedException();
        }

        public Response Execute(DaysOff daysOff, string email)
        {
            throw new NotImplementedException();
        }

        public Response Execute(string body, string email)
        {
            throw new NotImplementedException();
        }

        public Response Execute(RemoteWork request, string sender)
        {
            User user = this.repo.GetUserByEmail(sender);
            if (user == null)
            {
                throw new InexistentUserException("Userul cu mailul " + sender + " nu exista in baza de date. Trebuie inserat manual!");
            }
            request.UserId = user.UserId;
            try
            {
                Response response = new Response();
                response.MailSubject = this.ResponseMailSubject;
                response.User = user;
                //possible attachments
                //validations
                string body = this.CommandBodyApproved;

                if (request.WeekDay != "")
                {
                    this.repo.InsertRemoteWork(request);
                    string days = request.WeekDay;
                    body = body.Replace("<interval>", days);
                    response.MailBody = body;
                }
                else
                {
                    if (this.repo.DuplicateOrContainedRemoteWorkDatesByUser(user, (DateTime)request.StartDate, (DateTime)request.EndDate))
                    {
                        throw new DuplicateOrExistentDateException("Date interval given may already be requested or duplicate!");
                    }

                    string shortStartDate = request.StartDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                    string shortEndDate = request.EndDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                    body = body.Replace("<interval>", shortStartDate + " to " + shortEndDate);

                    this.repo.InsertRemoteWork(request);

                    response.MailBody = body;
                }
                emailSender.SendEmail(sender, response.MailBody, response.MailSubject, new List<string>());
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(this.CommandBodyRejected + '\n' + ex.Message);
            }

        }
    }
}
