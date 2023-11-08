using Common.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public partial class RequestAutomationContext : DbContext
    {
        public RequestAutomationContext()
        {

        }

        public RequestAutomationContext(DbContextOptions<RequestAutomationContext> options)
            :base(options)
        {

        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<DaysOff> DaysOff { get; set; }
        public virtual DbSet<RemoteWork> RemoteWork { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-3J8SED5;Initial Catalog=RequestAutomation;Integrated Security=True");
                //throw new Exception("Connection string is wrong or it was not setted in appsettings.json");
                //optionsBuilder.UseSqlServer(connectionString);
            
        }

    }
}
