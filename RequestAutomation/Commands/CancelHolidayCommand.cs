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
    public class CancelHolidayCommand : ICommand
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;

        public string CommandName { get => Constants.CommandNames.CancelHolidayCommand; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public CancelHolidayCommand(IRepository repository, IEmailSender emailSender)
        {
            this.repo = repository;
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
            daysOff.UserId = user.UserId;
            try
            {
                Response response = new Response();
                response.MailSubject = this.ResponseMailSubject;
                response.User = user;
                //possible attachments
                //validations
                string body = this.CommandBodyApproved;

                DateTime from = (DateTime)daysOff.FromDate;
                DateTime to = (DateTime)daysOff.ToDate;
                bool exists = this.repo.FindHoliday(from, to, daysOff.UserId);
                if (exists)
                {
                    bool deleted = repo.RemoveHoliday(from, to, daysOff.UserId);
                    if (!deleted)
                        throw new Exception("Could not delete the holiday.");


                    string shortDate = daysOff.FromDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                    body = body.Replace("<FromDate>", shortDate);
                    shortDate = daysOff.ToDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                    body = body.Replace("<ToDate>", shortDate);
                    response.MailBody = body;
                    emailSender.SendEmail(sender, response.MailBody, response.MailSubject, new List<string>());
                    return response;
                }
                else throw new Exception("Wrong holiday dates");
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
