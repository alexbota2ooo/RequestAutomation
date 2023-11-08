using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class PastDateException : Exception
    {
        public PastDateException() { }
        public PastDateException(string message)
        : base(message) { }
        public PastDateException(string message, Exception inner)
            : base(message, inner) { }
    }
}
