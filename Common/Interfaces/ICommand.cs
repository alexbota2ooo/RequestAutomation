using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ICommand
    {
        public Response Execute();
        public Response Execute(DaysOff daysOff, string email);
        public Response Execute(string body, string email);
        public Response Execute(RemoteWork remoteWork, string email);
        public string CommandName { get; }
        public string CommandSubject { get; set; }
        public string ResponseMailSubject { get; set; }
        public string CommandBodyApproved { get; set; }
        public string CommandBodyRejected { get; set; }
    }
}
