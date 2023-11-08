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
    public class DaysOffCommand : ICommand
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;
        public string CommandName { get => Constants.CommandNames.DaysOffCommand; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public DaysOffCommand(IRepository repository, IEmailSender emailSender)
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

        public Response Execute(string body, string sender)
        {
            User requestUser = this.repo.GetUserByEmail(sender);
            if(requestUser == null)
            {
                throw new InexistentUserException("Userul cu mailul " + sender + " nu exista in baza de date. Trebuie inserat manual!");
            }

            try
            {
                Response response = new Response();
                response.MailSubject = this.ResponseMailSubject;
                response.User = requestUser;

                List<DaysOff> daysOff = this.repo.GetDaysOff(requestUser.UserId);
                foreach (DaysOff dayOff in daysOff)
                {
                    string mailBodyLine = this.CommandBodyApproved;
                    if (dayOff.FromDate.Value.Year == DateTime.Now.Year)
                    {
                        string shortDate = dayOff.FromDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                        mailBodyLine = mailBodyLine.Replace("<FromDate>", shortDate);
                        shortDate = dayOff.ToDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                        mailBodyLine = mailBodyLine.Replace("<ToDate>", shortDate);
                        if (dayOff.Holiday == false)
                        {
                            mailBodyLine = mailBodyLine.Replace("<holiday>", "sick leave");
                        }
                        else if (dayOff.Holiday == true)
                        {
                            mailBodyLine = mailBodyLine.Replace("<holiday>", "holiday");
                        }
                        response.MailBody += mailBodyLine + "\n";
                    }
                }
                emailSender.SendEmail(sender, response.MailBody, response.MailSubject, new List<string>());
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(this.CommandBodyRejected + '\n' + ex.Message);
            }
        }

        public Response Execute(RemoteWork remoteWork, string email)
        {
            throw new NotImplementedException();
        }
    }
}
