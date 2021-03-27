using System;
using System.Text;
using System.Threading;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using backend.models;
using System.Collections.Concurrent;
using Connection = NATS.Client.Connection;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace backend
{
    public class Program
    {

        public static Logic logic;
        private static void Main(string[] args)
        {
            logic = new Logic(new DataStorage());
            logic.Startup();

            var host = CreateHostBuilder(args).Build();

            host.Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => 
                { 
                    webBuilder.UseStartup<Startup>(); 
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(logic.dataStorage);
                });


        
        
    }
}