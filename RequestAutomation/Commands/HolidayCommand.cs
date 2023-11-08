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
    public class HolidayCommand : ICommand
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;
        public string CommandName { get=>Constants.CommandNames.HolidayCommand; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public HolidayCommand(IRepository repository, IEmailSender emailSender)
        {
            this.repo = repository;
            this.emailSender = emailSender;
        }

        public Response Execute()
        {
            throw new NotImplementedException();
        }

        public Response Execute(string body, string email)
        {
            throw new NotImplementedException();
        }

        public Response Execute(DaysOff request, string sender)
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

                if (this.repo.DuplicateOrContainedDaysOffDatesByUser(user, (DateTime)request.FromDate, (DateTime)request.ToDate))
                {
                    throw new DuplicateOrExistentDateException("Date interval given may already be requested or duplicate!");
                }

                int offDaysLeft = this.repo.GetFreeDays(user.UserId);
                int nrDaysRequested = request.WorkingDays;
                if (nrDaysRequested > offDaysLeft)
                    throw new Exception("Not enough rest days left for this holiday!");

                string shortDate = request.FromDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                body = body.Replace("<FromDate>", shortDate);
                shortDate = request.ToDate.Value.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                body = body.Replace("<ToDate>", shortDate);

                this.repo.InsertHoliday(request);
                response.MailBody = body;
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
