using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    [Serializable]
    public class DuplicateOrExistentDateException : Exception
    {
        public DuplicateOrExistentDateException() { }
        public DuplicateOrExistentDateException(string message)
        : base(message) { }
        public DuplicateOrExistentDateException(string message, Exception inner)
            : base(message, inner) { }
    }
}
