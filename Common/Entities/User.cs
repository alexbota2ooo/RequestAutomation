using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class User
    {
        public Guid UserId { get; set; } //holds the primery key for user
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TotalDaysOff { get; set; }
        public string Email { get; set; }
        public string Team { get; set; }

        public virtual ICollection<DaysOff> DaysOff { get; set; }  //references to other tables
        public virtual ICollection<RemoteWork> RemoteWork { get; set; }

    }
}
