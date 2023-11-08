using Common.Entities;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class Repository : IRepository
    {
        private readonly RequestAutomationContext context;
        public Repository(RequestAutomationContext context)
        {
            this.context = context;
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                var UserByMail = context.Users.Where(
                                u => u.Email == email);
                return UserByMail.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error at GetUserByEmail. User does not exist!");
                return null;
            }
        }

        public bool DuplicateOrContainedDaysOffDatesByUser(User user, DateTime startDate, DateTime endDate)
        {
            var entries = context.DaysOff
                       .Where(d => d.UserId == user.UserId &&
                                                 ((d.ToDate == null) || (d.FromDate == null) ||
                                                (DateTime.Compare(startDate, d.FromDate.Value) <= 0 && DateTime.Compare(endDate, d.FromDate.Value) >= 0 && DateTime.Compare(endDate, d.ToDate.Value) <= 0) ||
                                                (DateTime.Compare(startDate, d.FromDate.Value) >= 0 && DateTime.Compare(endDate, d.ToDate.Value) <= 0) ||
                                                (DateTime.Compare(startDate, d.FromDate.Value) >= 0 && DateTime.Compare(startDate, d.ToDate.Value) <= 0 && DateTime.Compare(endDate, d.ToDate.Value) >= 0) ||
                                                (DateTime.Compare(startDate, d.FromDate.Value) <= 0 && DateTime.Compare(endDate, d.ToDate.Value) >= 0 && DateTime.Compare(endDate, d.FromDate.Value) > 0 && DateTime.Compare(startDate, d.ToDate.Value) < 0)
                                                ));
            return entries.Any();
        }

        public bool DuplicateOrContainedRemoteWorkDatesByUser(User user, DateTime startDate, DateTime endDate)
        {
            var entries = context.RemoteWork
                       .Where(d => d.UserId == user.UserId &&
                                                 ((d.EndDate == null) || (d.StartDate == null) ||
                                                (DateTime.Compare(startDate, d.StartDate.Value) <= 0 && DateTime.Compare(endDate, d.StartDate.Value) >= 0 && DateTime.Compare(endDate, d.EndDate.Value) <= 0) ||
                                                (DateTime.Compare(startDate, d.StartDate.Value) >= 0 && DateTime.Compare(endDate, d.EndDate.Value) <= 0) ||
                                                (DateTime.Compare(startDate, d.StartDate.Value) >= 0 && DateTime.Compare(startDate, d.EndDate.Value) <= 0 && DateTime.Compare(endDate, d.EndDate.Value) >= 0) ||
                                                (DateTime.Compare(startDate, d.StartDate.Value) <= 0 && DateTime.Compare(endDate, d.EndDate.Value) >= 0 && DateTime.Compare(endDate, d.StartDate.Value) > 0 && DateTime.Compare(startDate, d.EndDate.Value) < 0)
                                                ));
            return entries.Any();
        }

        public DaysOff InsertHoliday(DaysOff request)
        {
            try
            {
                context.DaysOff.Add(request);
                context.SaveChanges();
                return request;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception(ex.Message + " error adding holiday to database");
            }
        }

        public RemoteWork InsertRemoteWork(RemoteWork remoteWork)
        {
            try
            {
                context.RemoteWork.Add(remoteWork);
                context.SaveChanges();
                return remoteWork;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int GetFreeDays(Guid userId)
        {
            var freeDays = context.Users.Where(
                            u => u.UserId == userId).Select(
                u => u.TotalDaysOff);

            var usedDays = context.DaysOff.Where(d => d.UserId == userId && d.Holiday == true)
                .GroupBy(d => d.UserId)
                .Select(d => d.Sum(dem => dem.WorkingDays));

            if (usedDays.Any())
            {
                var result = Convert.ToInt32(freeDays.First()) - Convert.ToInt32(usedDays.First());
                return result;
            }

            return Convert.ToInt32(freeDays.First());
        }

        public bool FindHoliday(DateTime from, DateTime to, Guid userId)
        {
            var result = context.DaysOff.Where(d => d.UserId == userId && from == d.FromDate && to == d.ToDate).Select(d => d.UserId);
            return result.Any();
        }

        public bool RemoveHoliday(DateTime from, DateTime to, Guid userId)
        {
            var holiday = context.DaysOff.Where(d => d.FromDate == from && d.ToDate == to && d.UserId == userId).FirstOrDefault();
            if (holiday != null)
            {
                context.DaysOff.Remove(holiday);
                context.SaveChanges();
                return true;
            }
            else
            {
                Console.WriteLine("Error at remove holiday!");
                throw new Exception("Error at remove holiday!");
            }
        }

        public List<User> GetUsers()
        {
            var allUsers = from all in context.Users select all;
            List<User> users = allUsers.ToList();
            return users;
        }

        public string GetLocationByDay(int day, int month, Guid userId)
        {
            string location = "not specified";
            int year = DateTime.Now.Year;
            DateTime wantedDay = new DateTime(year, month, day);
            var locationDates = context.RemoteWork.Where(w => w.UserId == userId && w.WeekDay != "").OrderByDescending(w => w.StartDate).Select(w => w.StartDate);
            // if locations are on permanent demand (eg monday, friday)
            if (locationDates.Any())
            {
                DateTime startDate = Convert.ToDateTime(locationDates.First());
                //case if there is another location requested before in the month
                if (startDate.Month == month && day < startDate.Day && locationDates.Count() > 1)
                {
                    startDate = Convert.ToDateTime(locationDates.Skip(1).First());
                }
                var remoteWorks = context.RemoteWork.Where(w => w.UserId == userId && w.StartDate == startDate).Select(w => w);

                if ((remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("monday") && wantedDay.DayOfWeek == DayOfWeek.Monday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("tuesday") && wantedDay.DayOfWeek == DayOfWeek.Tuesday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("wednesday") && wantedDay.DayOfWeek == DayOfWeek.Wednesday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("thursday") && wantedDay.DayOfWeek == DayOfWeek.Thursday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("friday") && wantedDay.DayOfWeek == DayOfWeek.Friday))
                {
                    location = remoteWorks.Select(w => w.Location).First().ToString();
                }
                else if (wantedDay.DayOfWeek == DayOfWeek.Monday || wantedDay.DayOfWeek == DayOfWeek.Tuesday || wantedDay.DayOfWeek == DayOfWeek.Wednesday ||
                    wantedDay.DayOfWeek == DayOfWeek.Thursday || wantedDay.DayOfWeek == DayOfWeek.Friday)
                {
                    location = "not specified";
                }
                else
                    location = "-";
            }
            // if locations are intervals
            var intervalDatesLocation = context.RemoteWork.Where(w => w.UserId == userId && w.WeekDay == "" && w.StartDate <= wantedDay && w.EndDate >= wantedDay).OrderByDescending(w => w.StartDate).Select(w => w.Location);
            if (intervalDatesLocation.Any())
            {
                location = intervalDatesLocation.First().ToString();
            }

            if (wantedDay.DayOfWeek == DayOfWeek.Saturday || wantedDay.DayOfWeek == DayOfWeek.Sunday)
                location = "-";

            return location;
        }
        public string GetWorkingTypeByDay(string[] locations, int day, int month, Guid userId)
        {
            string type = "-";
            int year = DateTime.Now.Year;
            DateTime wantedDay = new DateTime(year, month, day);
            //default on site work
            if (wantedDay.DayOfWeek == DayOfWeek.Monday || wantedDay.DayOfWeek == DayOfWeek.Tuesday || wantedDay.DayOfWeek == DayOfWeek.Wednesday ||
                    wantedDay.DayOfWeek == DayOfWeek.Thursday || wantedDay.DayOfWeek == DayOfWeek.Friday)
            {
                type = "on site";
            }
            //work type remote
            var remoteInterval = context.RemoteWork.Where(d => d.UserId == userId && d.StartDate <= wantedDay && d.EndDate >= wantedDay).Select(u => u);
            if (remoteInterval.Any())
                type = "remote interval";
            var startDatesRemote = context.RemoteWork.Where(w => w.UserId == userId && w.WeekDay != "").OrderByDescending(w => w.StartDate).Select(w => w.StartDate);
            if (startDatesRemote.Any())
            {
                DateTime startDate = Convert.ToDateTime(startDatesRemote.First());
                var remoteWorks = context.RemoteWork.Where(w => w.UserId == userId && w.StartDate == startDate).Select(w => w);

                if ((remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("monday") && wantedDay.DayOfWeek == DayOfWeek.Monday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("tuesday") && wantedDay.DayOfWeek == DayOfWeek.Tuesday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("wednesday") && wantedDay.DayOfWeek == DayOfWeek.Wednesday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("thursday") && wantedDay.DayOfWeek == DayOfWeek.Thursday)
                || (remoteWorks.Select(w => w.WeekDay).First().ToString().Contains("friday") && wantedDay.DayOfWeek == DayOfWeek.Friday))
                {
                    type = "remote permanent";
                }
            }

            if (wantedDay.DayOfWeek == DayOfWeek.Saturday || wantedDay.DayOfWeek == DayOfWeek.Sunday)
                type = "-";
            else
            {
                //holiday
                var holiday = context.DaysOff.Where(d => d.UserId == userId && d.FromDate <= wantedDay && d.ToDate >= wantedDay && d.Holiday == true).Select(u => u);
                if (holiday.Any())
                {
                    type = "holiday";
                }
                //sick leave
                var sickLeave = context.DaysOff.Where(d => d.UserId == userId && d.FromDate <= wantedDay && d.ToDate >= wantedDay && d.Holiday == false).Select(u => u);
                if (sickLeave.Any())
                {
                    type = "sick leave";
                }
            }
            return type;
        }
        
        public List<DaysOff> GetDaysOff(Guid userId)
        {
            var daysOff = context.DaysOff.Where(d => d.UserId == userId).Select(u => u);
            if (daysOff.Any())
                return daysOff.ToList();
            return new List<DaysOff>();
        }
    }
}
