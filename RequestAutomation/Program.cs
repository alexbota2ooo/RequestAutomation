using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System;
using System.IO;
using DAL;
using System.Data.Common;
using DAL.Repositories;
using RequestAutomation.Commands;

namespace RequestAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            var configuration = builder.Build();

            string connectionString = configuration.GetSection("ConnectionStrings:ConnectionString").Value;
            

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddTransient<ICommand, HolidayCommand>()
                .AddTransient<ICommand, RemoteWorkCommand>()
                .AddTransient<ICommand, SickLeaveCommand>()
                .AddTransient<ICommand, DaysLeftCommand>()
                .AddTransient<ICommand, CancelHolidayCommand>()
                .AddTransient<ICommand, MonthlyReportCommand>()
                .AddTransient<ICommand, DaysOffCommand>()
                .AddTransient<ITokenizer, Tokenizer>()
                .AddTransient<IEmailHandler, EmailHandler>()
                .AddTransient<IEmailSender, EmailSender>()
                .AddTransient<IPythonRESTfulAPI, PythonRESTfulAPI>()
                .AddTransient<IRepository, Repository>()
                .AddTransient<IFileConfiguration, FileConfigurationReader>()
                .AddDbContext<RequestAutomationContext>(options => options.UseSqlServer(connectionString))
                .Configure<EmailConfiguration>(configuration.GetSection("Email"))
                .Configure<CommandConfiguration>(configuration.GetSection("FileLocation"))
                .BuildServiceProvider();

            var fileConfiguration = serviceProvider.GetService<IFileConfiguration>();
            var emailHandler = serviceProvider.GetService<IEmailHandler>();

            emailHandler.ReadEmail();
        }
        static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false,
                reloadOnChange: true);
        }
    }
}
