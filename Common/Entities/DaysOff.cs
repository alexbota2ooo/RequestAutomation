using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class DaysOff
    {
        public Guid DaysOffId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int WorkingDays { get; set; }
        public bool Holiday { get; set; }
        public Guid UserId { get; set; }
        public virtual User User{ get; set; }
    }
}
