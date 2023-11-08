using Common.Entities;
using Common.Interfaces;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IronOcr;
using System.IO;
using Common;
using Common.Exceptions;

namespace RequestAutomation
{
    public class Tokenizer : ITokenizer
    {
        IEnumerable<ICommand> commands;
        IFileConfiguration commandConfiguration;
        IPythonRESTfulAPI pythonAPI;
        Utils utils = new Utils();
        //private readonly IValidator validator;

        public Tokenizer(IEnumerable<ICommand> commands, IFileConfiguration commandConfiguration, IPythonRESTfulAPI pythonAPI)
        {
            this.commands = commands;
            this.commandConfiguration = commandConfiguration;
            this.pythonAPI = pythonAPI;
            commandConfiguration.ReadConfiguration(commands);
        }

        public Response GetCommand(MimeMessage message)
        {
            string subject = message.Subject;
            string body = message.TextBody;
            string email = message.From.OfType<MailboxAddress>().Single().Address;
            IEnumerable<MimeKit.MimeEntity> attachments = message.Attachments;
            email = email.ToLower();
            Console.WriteLine(subject);
            string[] splitted = subject.Trim().Split(new char[] { ',', '-', ';' });
            string commandTxt = "";
            string name = "";
            if (splitted.Length == 1)
            {
                commandTxt = splitted[0].Trim();
            }
            else
            {
                commandTxt = splitted[0].Trim();
                name = splitted[1].Trim();
            }

            Console.WriteLine(commandTxt);
            Console.WriteLine(name);
            Console.WriteLine(body);

            foreach(var command in this.commands)
            {
                //check if configuration file is correct
                if(command.CommandSubject == null || command.CommandBodyApproved == null || command.CommandBodyRejected == null || command.ResponseMailSubject == null)
                    throw new Exception("Settings were not configured correctly. Check again!");
                if (levenshtein(command.CommandSubject, commandTxt) <= 4)
                {
                    if (command.CommandName == Constants.CommandNames.HolidayCommand)
                    {
                        //return CallHolidayCommand(body, email, command);

                        DateTime fromDate = new DateTime();
                        DateTime toDate = new DateTime();
                        
                        if (body != null)
                            GetDatesFromBody(body, ref fromDate, ref toDate);
                        else
                        {
                            fromDate = DateTime.MinValue;
                            toDate = DateTime.MinValue;
                        }

                        if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)       // if dates were not found in text we check attachments
                        {
                            GetDatesFromAttachments(attachments, ref fromDate, ref toDate);
                        }

                        if (fromDate == default(DateTime) || toDate == default(DateTime))
                            throw new WrongDateFormatException("Wrong date formats or no dates in your request!");

                        if (fromDate < DateTime.Now)
                            throw new PastDateException("Your request was rejected because :\n Requested interval is from the past!");

                        DaysOff daysOff = new DaysOff();
                        daysOff.DaysOffId = Guid.NewGuid();
                        daysOff.FromDate = fromDate;
                        daysOff.ToDate = toDate;
                        daysOff.Holiday = true;
                        daysOff.WorkingDays = utils.BusinessDaysUntil(fromDate, toDate);

                        try
                        {
                            Response feedbackRequest = command.Execute(daysOff, email);
                            return feedbackRequest;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else if (command.CommandName == Constants.CommandNames.SickCommand)
                    {
                        DateTime fromDate = new DateTime();
                        DateTime toDate = new DateTime();
                        GetDatesFromBody(body, ref fromDate, ref toDate);

                        if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)       // if dates were not found in text we check attachments
                        {
                            GetDatesFromAttachments(attachments, ref fromDate, ref toDate);
                        }

                        if (fromDate == default(DateTime) || toDate == default(DateTime))
                            throw new WrongDateFormatException("Wrong date formats or no dates in your request!");

                        if (fromDate < DateTime.Now)
                            throw new PastDateException("Your request was rejected because :\n Requested interval is from the past!");

                        DaysOff daysOff = new DaysOff();
                        daysOff.DaysOffId = Guid.NewGuid();
                        daysOff.FromDate = fromDate;
                        daysOff.ToDate = toDate;
                        daysOff.Holiday = false;
                        daysOff.WorkingDays = utils.BusinessDaysUntil(fromDate, toDate);

                        try
                        {
                            Response feedbackRequest = command.Execute(daysOff, email);
                            return feedbackRequest;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else if (command.CommandName == Constants.CommandNames.RemoteWorkCommand)
                    {
                        //user can work from home in an interval/ some selected days every week
                        DateTime fromDate = new DateTime();
                        DateTime toDate = new DateTime();
                        RemoteWork remoteWorkDays = new RemoteWork();
                        //optional: user adds his location
                        string location = "not specified";
                        location = GetLocation(body);   

                        string daysOfWeek = "";
                        Regex daysOfWeekRegex = new Regex(@"((mon|tues|wed(nes)?|thur(s)?|fri)(day)?)",
                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        MatchCollection matchDaysOfWeek = daysOfWeekRegex.Matches(body);
                        if (matchDaysOfWeek.Count > 0)      //if user demands same days every week from now on
                        {
                            foreach (var dayOfWeek in matchDaysOfWeek)
                            {
                                if (daysOfWeek == "")
                                    daysOfWeek = dayOfWeek.ToString();
                                else
                                    daysOfWeek = daysOfWeek.ToString() + "," + dayOfWeek.ToString();
                            }
                            remoteWorkDays.RemoteWorkId = Guid.NewGuid();
                            remoteWorkDays.WeekDay = daysOfWeek;
                            remoteWorkDays.StartDate = DateTime.Now;
                            remoteWorkDays.EndDate = DateTime.Now;
                            remoteWorkDays.Location = location;
                        }
                        else
                        {                           //else its a date interval
                            GetDatesFromBody(body, ref fromDate, ref toDate);

                            if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)       // if dates were not found in text we check attachments
                            {
                                GetDatesFromPDFAttachment(attachments, ref fromDate, ref toDate);
                            }
                            if (fromDate == default(DateTime) || toDate == default(DateTime))
                                throw new WrongDateFormatException("Your request was rejected because :\n Wrong date formats or no dates in your request!");

                            if (fromDate < DateTime.Now)
                                throw new PastDateException("Your request was rejected because :\n Requested interval is from the past!");

                            remoteWorkDays.RemoteWorkId = Guid.NewGuid();
                            remoteWorkDays.WeekDay = daysOfWeek;
                            remoteWorkDays.StartDate = fromDate;
                            remoteWorkDays.EndDate = toDate;
                            remoteWorkDays.Location = location;
                        }

                        try
                        {
                            Response feedbackRequest = command.Execute(remoteWorkDays, email);
                            return feedbackRequest;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }

                    }
                    else if (command.CommandName == Constants.CommandNames.DaysLeftCommand)
                    {
                        return command.Execute(new DaysOff(), email);
                    }    
                    else if (command.CommandName == Constants.CommandNames.DaysOffCommand)
                    {
                        return command.Execute("", email);
                    }
                    else if (command.CommandName == Constants.CommandNames.CancelHolidayCommand)
                    {
                        DateTime fromDate = new DateTime();
                        DateTime toDate = new DateTime();
                        GetDatesFromBody(body, ref fromDate, ref toDate);

                        if (fromDate == DateTime.MinValue || toDate == DateTime.MinValue)       // if dates were not found in text we check attachments
                        {
                            GetDatesFromPDFAttachment(attachments, ref fromDate, ref toDate);
                        }

                        DaysOff daysOff = new DaysOff();
                        daysOff.DaysOffId = Guid.NewGuid();
                        daysOff.FromDate = fromDate;
                        daysOff.ToDate = toDate;
                        daysOff.WorkingDays = utils.BusinessDaysUntil(fromDate, toDate);

                        try
                        {
                            Response feedbackRequest = command.Execute(daysOff, email);
                            return feedbackRequest;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                    else if (command.CommandName == Constants.CommandNames.MonthlyReportCommand)
                    {
                        try
                        {
                            Response feedbackRequest = command.Execute(body, email);
                            return feedbackRequest;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    }
                }
            }
            throw new Exception("The command does not exist!");
        }

        private string GetLocation(string body)
        {
            string location = "";
            string[] lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line != "")
                {
                    string[] splittedSubcommand = line.Trim().Split(new char[] { ':', ';' });

                    if (splittedSubcommand.Length > 1)
                    {
                        string subcommand = splittedSubcommand[0];
                        string value = splittedSubcommand[1];

                        if (levenshtein(subcommand.Trim(), "Location") <= 2 || levenshtein(subcommand.Trim(), "Locatia") <= 2)
                        {
                            location =  value;
                        }
                    }
                }
            }
            return location;
        }


        private void GetDatesFromPDFAttachment(IEnumerable<MimeEntity> attachments, ref DateTime fromDate, ref DateTime toDate)
        {
            var Ocr = new IronTesseract();
            foreach (MimeEntity attachment in attachments)
            {
                if (attachment is MessagePart)
                {
                    var fileName = attachment.ContentDisposition?.FileName;
                    var rfc822 = (MessagePart)attachment;

                    if (string.IsNullOrEmpty(fileName))
                        fileName = "attached-message.eml";

                    using (var stream = File.Create(fileName))
                    {
                        rfc822.Message.WriteTo(stream);
                    }
                }
                else
                {
                    var part = (MimePart)attachment;
                    var fileName = part.FileName;
                    Regex pdfRegex = new Regex(@"^.*\.(pdf|PDF)$");

                    if (pdfRegex.IsMatch(fileName))
                    {
                        string pdfText = GetPdfText(Ocr, part, fileName);
                        GetDatesFromBody(pdfText, ref fromDate, ref toDate);
                    }
                }
            }
        }

        private void GetDatesFromAttachments(IEnumerable<MimeEntity> attachments, ref DateTime fromDate, ref DateTime toDate)
        {
            var Ocr = new IronTesseract();
            foreach (MimeEntity attachment in attachments)
            {
                if (attachment is MessagePart)
                {
                    var fileName = attachment.ContentDisposition?.FileName;
                    var rfc822 = (MessagePart)attachment;

                    if (string.IsNullOrEmpty(fileName))
                        fileName = "attached-message.eml";

                    using (var stream = File.Create(fileName))
                    {
                        rfc822.Message.WriteTo(stream);
                    }
                }
                else
                {
                    var part = (MimePart)attachment;
                    var fileName = part.FileName;
                    Regex pdfRegex = new Regex(@"^.*\.(pdf|PDF)$");
                    Regex imageRegex = new Regex(@"^.*\.(jpg|JPG|png|PNG|jpeg|JPEG)$");

                    if (pdfRegex.IsMatch(fileName))
                    {
                        string pdfText = GetPdfText(Ocr, part, fileName);
                        GetDatesFromBody(pdfText, ref fromDate, ref toDate);
                    }
                    else if (imageRegex.IsMatch(fileName))
                    {

                        using (var stream = File.Create("request.png"))
                        {
                            part.Content.DecodeTo(stream);
                        }
                        Console.WriteLine(fileName);

                        string uirWebAPI, exceptionMessage, webResponse;

                        // Set the UIR endpoint link. It should go to the application config file 
                        uirWebAPI = "http://localhost:5000/api";
                        exceptionMessage = string.Empty;

                        // Get web response by calling the CSharpPythonRestfulApiSimpleTest() method
                        //webResponse = csharpPythonRESTfulAPI.CSharpPythonRestfulApiSimpleTest(uirWebAPI, out exceptionMessage);
                        webResponse = pythonAPI.PythonDigitRecognizerAPI(uirWebAPI, "request.png", out exceptionMessage);

                        if (string.IsNullOrEmpty(exceptionMessage))
                        {
                            // No errors occurred. Write the string web response     
                            List<DateTime> recognizedDates = GetDateFromResponse(webResponse);
                            if (recognizedDates.Count == 2)
                            {
                                fromDate = recognizedDates[0];
                                toDate = recognizedDates[1];
                            }
                        }
                        else
                        {
                            // An error occurred. Write the exception message
                            throw new Exception(exceptionMessage);
                            //Console.WriteLine(exceptionMessage);
                        }
                    }
                }
            }
        }

        private static string GetPdfText(IronTesseract Ocr, MimePart part, string fileName)
        {
            using (var stream = File.Create(fileName))
            {
                part.Content.DecodeTo(stream);
            }

            using (var Input = new OcrInput(part.FileName))
            {
                // Input.Deskew();  // use if image not straight
                // Input.DeNoise(); // use if image contains digital noise
                var Result = Ocr.Read(Input);
                return Result.Text.ToString();
                //Console.WriteLine(Result.Text);
            }
        }

        private void GetDatesFromBody(string body, ref DateTime fromDate, ref DateTime toDate)
        {
            Regex textDateCurrentYear = new Regex(@"(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?)\s+\d{1,2}",
                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex textDateCommaYear = new Regex(@"(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?)\s+\d{1,2},\s+\d{4}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex textDateYear = new Regex(@"(Jan(uary)?|Feb(ruary)?|Mar(ch)?|Apr(il)?|May|Jun(e)?|Jul(y)?|Aug(ust)?|Sep(tember)?|Oct(ober)?|Nov(ember)?|Dec(ember)?)\s+\d{1,2}\s+\d{4}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex normalDateFormat = new Regex(@"(1[0-2]|0?[1-9])/(3[01]|[12][0-9]|0?[1-9])/(?:[0-9]{2})?[0-9]{2}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex dashDateFormat = new Regex(@"(1[0-2]|0?[1-9])\-(3[01]|[12][0-9]|0?[1-9])\-(?:[0-9]{2})?[0-9]{2}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matchTextDateCurrentYear = textDateCurrentYear.Matches(body);
            MatchCollection matchTextDateCommaYear = textDateCommaYear.Matches(body);
            MatchCollection matchTextDateYear = textDateYear.Matches(body);
            MatchCollection matchNormalDateFormat = normalDateFormat.Matches(body);
            MatchCollection matchDashDateFormat = dashDateFormat.Matches(body);

            if (matchTextDateCommaYear.Count == 2)
            {
                fromDate = utils.transformTextDate(matchTextDateCommaYear[0].ToString());
                toDate = utils.transformTextDate(matchTextDateCommaYear[1].ToString());
            }
            else if (matchTextDateYear.Count == 2)
            {
                fromDate = utils.transformTextDate(matchTextDateYear[0].ToString());
                toDate = utils.transformTextDate(matchTextDateYear[1].ToString());
            }
            else if (matchTextDateCurrentYear.Count == 2)
            {
                fromDate = utils.transformTextDate(matchTextDateCurrentYear[0].ToString());
                toDate = utils.transformTextDate(matchTextDateCurrentYear[1].ToString());
            }
            else if (matchNormalDateFormat.Count == 2)
            {
                //validations needed
                fromDate = Convert.ToDateTime(matchNormalDateFormat[0].ToString());
                toDate = Convert.ToDateTime(matchNormalDateFormat[1].ToString());
            }
            else if (matchDashDateFormat.Count == 2)
            {
                //validations needed
                fromDate = Convert.ToDateTime(matchDashDateFormat[0].ToString());
                toDate = Convert.ToDateTime(matchDashDateFormat[1].ToString());
            }
        }

        public static bool IsValidDate(int year, int month, int day)
        {
            if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
                return false;

            if (month < 1 || month > 12)
                return false;

            return day > 0 && day <= DateTime.DaysInMonth(year, month);
        }

        private static List<DateTime> GetDateFromResponse(string webResponse)
        {
            List<DateTime> recognizedDates = new List<DateTime>();
            string responseArrays = webResponse;
            responseArrays = responseArrays.Remove(0, 1);
            responseArrays = responseArrays.Remove(responseArrays.Length - 1);
            char[] characters = responseArrays.ToCharArray();
            bool inArray = false;           // checks if we are into an array from string . eg [], [1,2,3,4], ...
            int nrDigitsRecognized = 0;     //number of digits recognized in an array  (field of the document)
            List<List<int>> datesMatrix = new List<List<int>>();    //lists of dates, stored as array of int
            List<int> currentArray = new List<int>();               //current date
            foreach (char c in characters)
            {
                if (c == '[')
                {
                    nrDigitsRecognized = 0;
                    inArray = true;
                }
                else if (c == ']')
                {
                    inArray = false;
                    if (currentArray.Count == 8)
                        datesMatrix.Add(currentArray.ToList());
                    currentArray.Clear();
                }

                bool isNumeric = int.TryParse(c.ToString(), out int n);
                if (isNumeric)
                {
                    currentArray.Add(Convert.ToInt32(c.ToString()));
                }
            }

            foreach (var date in datesMatrix)
            {
                int[] currentDate = date.ToArray();
                int day = currentDate[0] * 10 + currentDate[1];
                int month = currentDate[2] * 10 + currentDate[3];
                int year = currentDate[4] * 1000 + currentDate[5] * 100 + currentDate[6] * 10 + currentDate[7];
                //validations
                if (IsValidDate(year, month, day))
                {
                    DateTime recognizedDate = new DateTime(year, month, day);
                    Console.WriteLine(recognizedDate);
                    recognizedDates.Add(recognizedDate);
                }
                else
                    throw new Exception("Date time not recognized. Use dd-MM-yyyy format");
            }
            return recognizedDates;
        }

        public int levenshtein(string first, string second)
        {

            int firstLen = first.Length;
            int secondLen = second.Length;
            int[,] distance = new int[firstLen + 1, secondLen + 1];

            // Verify arguments.
            if (firstLen == 0)
            {
                return secondLen;
            }

            if (secondLen == 0)
            {
                return firstLen;
            }

            // Initialize arrays.
            for (int i = 0; i <= firstLen; distance[i, 0] = i++)
            {
            }

            for (int j = 0; j <= secondLen; distance[0, j] = j++)
            {
            }

            // Begin looping.
            for (int i = 1; i <= firstLen; i++)
            {
                for (int j = 1; j <= secondLen; j++)
                {
                    // Compute cost.
                    int cost = (second[j - 1] == first[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
                }
            }
            // Return cos
            return distance[firstLen, secondLen];

        }
    }
}
