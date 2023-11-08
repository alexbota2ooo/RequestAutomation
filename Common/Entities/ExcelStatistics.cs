using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class ExcelStatistics
    {
        public string Name { get; set; }
        public int TotalWorkDays { get; set; }
        public int TotalHolidayDays { get; set; }
        public int TotalSickDays { get; set; }
        public int TotalWorkHours { get; set; }
    }
}
