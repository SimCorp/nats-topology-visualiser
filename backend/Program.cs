using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace backend
{
    public class Program
    {
        public static DateTime dateOfNatsRequest;
        public static EventParser eventParser;
        public static DrawablesProcessor drawablesProcessor;
        private static void Main(string[] args)
        {
            UpdateData();
            var host = CreateHostBuilder(args).Build();

            CreateHostBuilder(args).Build().Run();
            host.Run();
        }

        public static void UpdateData() 
        {
            eventParser = new EventParser(new DataStorage());
            //eventParser.Parse();

            dateOfNatsRequest = DateTime.Now;

            drawablesProcessor = new DrawablesProcessor(eventParser.dataStorage);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => 
                { 
                    webBuilder.UseStartup<Startup>(); 
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(eventParser.dataStorage);
                });
    }
}
