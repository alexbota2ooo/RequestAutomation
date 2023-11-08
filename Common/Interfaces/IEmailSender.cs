using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IEmailSender
    {
        public void SendEmail(string clientEmail, string mailBody, string mailSubject, List<string> attachments);
    }
}
