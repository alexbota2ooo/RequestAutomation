using Common.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace RequestAutomation
{
    public class FileConfigurationReader : IFileConfiguration
    {
        public readonly CommandConfiguration options;

        public FileConfigurationReader(IOptions<CommandConfiguration> options)
        {
            this.options = options.Value;
            ReadFreeDates();
        }

        public void ReadFreeDates()
        {
            try
            {
                string fileLocation = options.LocationFreeDays;
                string[] lines = System.IO.File.ReadAllLines(fileLocation);
                foreach (string line in lines)
                {
                    string[] stringDate = line.Trim().Split("-");
                    DateTime freeDay = new DateTime(Convert.ToInt32(stringDate[2]), Convert.ToInt32(stringDate[1]), Convert.ToInt32(stringDate[0]));
                    Constants.holidayDates.Add(freeDay);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Free days were not configured as required dd-MM-yyyy in file or location not found \n" + ex.Message);
            }
        }

        public void ReadConfiguration(IEnumerable<ICommand> commands)
        {
            try
            {
                string fileLocation = options.Location;

                string[] lines = System.IO.File.ReadAllLines(fileLocation);
                foreach (ICommand command in commands)
                {
                    for (int lineNr = 0; lineNr < lines.Length; lineNr++)
                    {
                        if (command.CommandName == lines[lineNr])
                        {
                            string[] subjectLine = lines[lineNr + 1].Split(":");
                            command.CommandSubject = subjectLine[1].Trim();
                            string[] approvedSubjectLine = lines[lineNr + 2].Split(":");
                            command.ResponseMailSubject = approvedSubjectLine[1].Trim();
                            string[] approvedResponse = lines[lineNr + 3].Split(":");
                            command.CommandBodyApproved = approvedResponse[1].Trim();
                            string[] rejectedResponse = lines[lineNr + 4].Split(":");
                            command.CommandBodyRejected = rejectedResponse[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commands not configured as required or location not found \n" + ex.Message);
            }

        }

    }
}
