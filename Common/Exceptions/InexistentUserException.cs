using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    [Serializable]
    public class InexistentUserException : Exception 
    {
        public InexistentUserException(string message)
        : base(message) { }
        public InexistentUserException() { }
        public InexistentUserException(string message, Exception inner)
            : base(message, inner) { }
    }
}
