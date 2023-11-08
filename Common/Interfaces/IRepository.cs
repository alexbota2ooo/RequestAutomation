using Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IRepository
    {
        public User GetUserByEmail(string email);
        public bool DuplicateOrContainedDaysOffDatesByUser(User user, DateTime startDate, DateTime endDate);
        public DaysOff InsertHoliday(DaysOff request);
        public RemoteWork InsertRemoteWork(RemoteWork remoteWork);
        public int GetFreeDays(Guid userId);
        public bool FindHoliday(DateTime from, DateTime to, Guid userId);
        public bool RemoveHoliday(DateTime from, DateTime to, Guid userId);
        public bool DuplicateOrContainedRemoteWorkDatesByUser(User user, DateTime startDate, DateTime endDate);
        public List<User> GetUsers();
        public string GetLocationByDay(int day, int month, Guid userId);
        public string GetWorkingTypeByDay(string[] locations, int day, int month, Guid userId);
        public List<DaysOff> GetDaysOff(Guid userId);
    }
}
