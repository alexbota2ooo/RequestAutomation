using Common.Entities;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITokenizer
    {
        Response GetCommand(MimeMessage message);
    }
}
