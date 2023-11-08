using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Constants
    {
        public static List<DateTime> holidayDates = new List<DateTime>
        {
            // zile de sarbatoare 2022
            /*new DateTime(2022, 04, 22),
            new DateTime(2022, 04, 24),
            new DateTime(2022, 04, 25),
            new DateTime(2022, 05, 01),
            new DateTime(2022, 06, 01),
            new DateTime(2022, 06, 12),
            new DateTime(2022, 06, 13),
            new DateTime(2022, 08, 15),
            new DateTime(2022, 11, 30),
            new DateTime(2022, 12, 01),
            new DateTime(2022, 12, 25),
            new DateTime(2022, 12, 26)*/
        };

        public static class CommandNames
        {
            public const string HolidayCommand = "Holiday command";
            public const string RemoteWorkCommand = "Remote work command";
            public const string DaysLeftCommand = "Rest days left command";
            public const string SickCommand = "Sick command";
            public const string CancelHolidayCommand = "Cancel holiday command";
            public const string MonthlyReportCommand = "Monthly report command";
            public const string DaysOffCommand = "Days off command";
        }

        public static class BodyTemplates
        {
            public const string HolidayCommandTemplate = "Hello! Your request was approved from <FromDate> to <ToDate>.";
        }

        public enum Months
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }

        public enum WorkingHours
        {
            FullNormStartHour = 9,
            FullNormEndHour = 17,
            HalfNormStartHour = 9,
            HalfNormEndHour = 13
        }
    }
}
