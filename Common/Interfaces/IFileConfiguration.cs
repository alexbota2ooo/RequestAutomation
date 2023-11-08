using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IFileConfiguration
    {
        void ReadConfiguration(IEnumerable<ICommand> commands);
        void ReadFreeDates();
    }
}
