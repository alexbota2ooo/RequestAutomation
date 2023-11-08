using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestAutomation
{
    public class Utils
    {
        public DateTime transformTextDate(string textDate)
        {
            int day = 0;
            int month = 0;
            int year = 0000;
            string[] textDateParts = textDate.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (textDateParts.Count() > 1)
            {
                if (textDateParts[0].Contains("Jan"))
                    month = 1;
                else if (textDateParts[0].Contains("Feb"))
                    month = 2;
                else if (textDateParts[0].Contains("Mar"))
                    month = 3;
                else if (textDateParts[0].Contains("Apr"))
                    month = 4;
                else if (textDateParts[0].Contains("May"))
                    month = 5;
                else if (textDateParts[0].Contains("Jun"))
                    month = 6;
                else if (textDateParts[0].Contains("Jul"))
                    month = 7;
                else if (textDateParts[0].Contains("Aug"))
                    month = 8;
                else if (textDateParts[0].Contains("Sep"))
                    month = 9;
                else if (textDateParts[0].Contains("Oct"))
                    month = 10;
                else if (textDateParts[0].Contains("Nov"))
                    month = 11;
                else if (textDateParts[0].Contains("Dec"))
                    month = 12;
                day = Convert.ToInt32(textDateParts[1]);
            }
            if (textDateParts.Count() == 3)
                year = Convert.ToInt32(textDateParts[2]);
            else
               year = DateTime.Now.Year;
            DateTime date = new DateTime(year, month, day);
            return date;
        }

        public int BusinessDaysUntil(DateTime firstDay, DateTime lastDay)
        {
            DateTime[] bankHolidays = Constants.holidayDates.ToArray();

            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (firstDay <= bh && bh <= lastDay && bh.DayOfWeek != DayOfWeek.Sunday && bh.DayOfWeek != DayOfWeek.Saturday)
                    --businessDays;
            }

            return businessDays;
        }
    }
}
