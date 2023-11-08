﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Entities
{
    public class MailResponse
    {
        public Guid UserId { get; set; }
        public string MailBody { get; set; }
        public string MailSubject { get; set; }

        // public Boolean success { get; set; }
        public int CommandType { get; set; }
    }
}
