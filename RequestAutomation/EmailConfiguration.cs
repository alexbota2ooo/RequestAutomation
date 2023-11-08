using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestAutomation
{
    public class EmailConfiguration
    {
        public string SMTPhost { get; set; }
        public int SMTPport { get; set; }
        public int IMAPport { get; set; }
        public int POP3port { get; set; }
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public string Password { get; set; }
    }
}
