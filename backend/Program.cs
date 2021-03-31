using System.IO;
using NATS.Client;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using backend.helpers;
using Env = System.Environment;
namespace backend
{
    class Program
    {
        
        public static List<NetworkInstance> instances = new List<NetworkInstance>();
        public static int instanceCount = 0;
        private static void Main(string[] args)
        {
            SetEnv();

            var options = ConnectionFactory.GetDefaultOptions();
            NewInstance(Env.GetEnvironmentVariable("NATS_URL"));
            instances[0].GetInformation();

            CreateHostBuilder(args).Build().Run();
        }

        public static void NewInstance(string url)
        {
            var instance = new NetworkInstance(instanceCount, url);
            instances.Add(instance);
            instanceCount++;
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        
        private static void SetEnv() 
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);
            
            var config =
                new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();
        }
    }
}
