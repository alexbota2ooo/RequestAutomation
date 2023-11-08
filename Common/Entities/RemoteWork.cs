using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class RemoteWork
    {
        public Guid RemoteWorkId { get; set; }
        public string? Location { get; set; }
        public string? WeekDay { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StartHour { get; set; }
        public int? EndHour { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}
