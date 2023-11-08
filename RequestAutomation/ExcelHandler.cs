using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using Common.Interfaces;
using Common.Entities;

namespace RequestAutomation
{
    class ExcelHandler<T>
    {
        private readonly IRepository repo;

        private FileInfo file;

        public ExcelHandler(IRepository repo)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this.repo = repo;
        }

        public ExcelHandler(IRepository repo, string fileName)
        {
            this.repo = repo;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            this.file = new FileInfo(fileName);
            this.DeleteIfExists();
        }

        public void DeleteIfExists()
        {
            if (file.Exists)
                file.Delete();
        }

        public void setupFile(string fileName)
        {
            this.file = new FileInfo(fileName);
            this.DeleteIfExists();
        }
        public void setupWithoutDelete(string fileName)
        {
            this.file = new FileInfo(fileName);
        }

        public void setDateFormat(String column, ExcelWorksheet worksheet)
        {
            using (var package = new ExcelPackage(file))
                worksheet.Cells[column].Style.Numberformat.Format = "yyyy-mm-dd";
        }

        public void SaveWorksheet(List<T> list, User user)
        {
            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets.Add(user.LastName + " " + user.FirstName);

                worksheet.Cells["A1"].Value = "Name: ";
                worksheet.Cells["B1"].Value = user.LastName + " " + user.FirstName;
                worksheet.Cells["D1"].Value = "Signature:";
                worksheet.Cells["D1:E1"].Merge = true;

                var range = worksheet.Cells["A2"].LoadFromCollection(list, true);
                range.AutoFitColumns();

                package.Save();
            }
        }

        public void SaveStatisticsWorkSheet(List<ExcelStatistics> list)
        {
            using (var package = new ExcelPackage(file))
            {
                var worksheet = package.Workbook.Worksheets.Add("Statistics");

                var range = worksheet.Cells["A2"].LoadFromCollection(list, true);
                range.AutoFitColumns();

                package.Save();
            }
        }

    }
}
