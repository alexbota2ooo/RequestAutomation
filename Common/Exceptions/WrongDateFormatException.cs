using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class WrongDateFormatException : Exception
    {
        public WrongDateFormatException() { }
        public WrongDateFormatException(string message)
        : base(message) { }
        public WrongDateFormatException(string message, Exception inner)
            : base(message, inner) { }
    }
}
