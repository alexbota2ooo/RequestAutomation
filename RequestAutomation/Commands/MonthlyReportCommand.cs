using Common;
using Common.Entities;
using Common.Exceptions;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Common.Constants;

namespace RequestAutomation.Commands
{
    public class MonthlyReportCommand : ICommand
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;

        public string CommandName { get => Constants.CommandNames.MonthlyReportCommand; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }

        public MonthlyReportCommand(IRepository repository, IEmailSender emailSender)
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
            if (requestUser == null)
            {
                throw new InexistentUserException("Userul cu mailul " + sender + " nu exista in baza de date. Trebuie inserat manual!");
            }

            int month = 1;
            bool containsInt = body.Trim().Any(char.IsDigit);
            Regex monthNumberRegex = new Regex(@"\b([1-9]|1[0-2])\b");
            string cleanedBody = body.Replace("\n", "").Replace("\r", "");
            if (containsInt && monthNumberRegex.IsMatch(cleanedBody.Trim()))
            {
                MatchCollection monthNr = monthNumberRegex.Matches(cleanedBody.Trim());
                month = Convert.ToInt32(monthNr[0].Value.ToString());
            }
            else
                month = DateTime.Now.Month;
            var monthString = (Months)month;
            try
            {
                Response response = new Response();
                response.MailSubject = this.ResponseMailSubject;
                response.User = requestUser;
                response.MailBody = this.CommandBodyApproved;

                ExcelHandler<ExcelInfo> excelHandler = new ExcelHandler<ExcelInfo>(this.repo);
                excelHandler.setupFile("MonthlyReport" + monthString + ".xlsx");
                List<ExcelStatistics> statistics = new List<ExcelStatistics>();
                List<User> allUsers = this.repo.GetUsers();
                
                foreach(User user in allUsers)
                {
                    string[] locations = new string[32];
                    string[] typesOfWork = new string[32];
                    int[,] hours = new int[32, 2];
                    int[] nrHours = new int[32];
                    int totalWorkDays = 0;
                    int totalHolidayDays = 0;
                    int totalSickDays = 0;
                    int totalWorkHours = 0;
                    ExcelStatistics statistic = new ExcelStatistics();
                    List<ExcelInfo> excelInfos = new List<ExcelInfo>();
                    int year = DateTime.Now.Year;
                    int daysThisMonth = DateTime.DaysInMonth(year, month);

                    //adding locations, type of work and hours
                    for (int i = 1; i <= daysThisMonth; i++)
                    {
                        locations[i] = this.repo.GetLocationByDay(i, month, user.UserId);
                        typesOfWork[i] = this.repo.GetWorkingTypeByDay(locations, i, month, user.UserId);
                        if (typesOfWork[i] != "-" && typesOfWork[i] != "holiday" && typesOfWork[i] != "sick leave")
                        {
                            hours[i, 0] = (int)Constants.WorkingHours.FullNormStartHour;
                            hours[i, 1] = (int)Constants.WorkingHours.FullNormEndHour;
                            nrHours[i] = hours[i, 1] - hours[i, 0];
                        }
                    }
                    for (int i = 1; i <= daysThisMonth; i++)
                    {
                        ExcelInfo excelInfo = new ExcelInfo();
                        excelInfo.Day = i.ToString();
                        excelInfo.Location = locations[i];
                        excelInfo.WayOfWorking = typesOfWork[i];
                        excelInfo.StartHour = hours[i, 0].ToString();
                        excelInfo.EndHour = hours[i, 1].ToString();
                        excelInfo.NrHours = nrHours[i];
                        totalWorkHours += nrHours[i];
                        if (typesOfWork[i] == "on site" || typesOfWork[i] == "remote interval" || typesOfWork[i] == "remote permanent")
                            totalWorkDays += 1;
                        if (typesOfWork[i] == "holiday")
                            totalHolidayDays += 1;
                        if (typesOfWork[i] == "sick leave")
                            totalSickDays += 1;
                        excelInfos.Add(excelInfo);
                    }
                    statistic.Name = user.FirstName + " " + user.LastName;
                    statistic.TotalWorkDays = totalWorkDays;
                    statistic.TotalHolidayDays = totalHolidayDays;
                    statistic.TotalSickDays = totalSickDays;
                    statistic.TotalWorkHours = totalWorkHours;
                    statistics.Add(statistic);
                    excelHandler.SaveWorksheet(excelInfos, user);
                }
                excelHandler.SaveStatisticsWorkSheet(statistics);
                string approvedBody = this.CommandBodyApproved;
                response.MailBody = approvedBody;
                List<string> attachments = new List<string>();
                attachments.Add("MonthlyReport" + monthString + ".xlsx");
                emailSender.SendEmail(sender, response.MailBody, response.MailSubject, attachments);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(this.CommandBodyRejected + '\n' + ex.Message);
            }
            throw new NotImplementedException();
        }

        public Response Execute(RemoteWork remoteWork, string email)
        {
            throw new NotImplementedException();
        }
    }
}
