using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace backend
{
    class Program
    {
        public static DateTime dateOfNatsRequest;
        public static EventParser eventParser;
        public static DrawablesProcessor drawablesProcessor;
        private static void Main(string[] args)
        {
            eventParser = new EventParser(new DataStorage());
            eventParser.Parse();

            dateOfNatsRequest = DateTime.Now;

            drawablesProcessor = new DrawablesProcessor(eventParser.dataStorage);

            var host = CreateHostBuilder(args).Build();

            CreateHostBuilder(args).Build().Run();
            host.Run();
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
<<<<<<< HEAD
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    //configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                
                }).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });


        private static void IncomingMessageHandlerRawJson(object sender, MsgHandlerEventArgs e)
        {
            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            Console.WriteLine(e.Message.ArrivalSubscription.Subject);

            Console.WriteLine(json.ToString());
        }

        private static void IncomingMessageHandlerServer(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");

            Server server = new Server();

            try
            {
                server = JsonConvert.DeserializeObject<Server>(data_json.ToString());
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            idToServer.TryAdd(server.server_id, server);
            servers.Add(server);
        }

        private static void IncomingMessageHandlerConnection(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");

            backend.models.Connection connection = new backend.models.Connection();

            try
            {
                connection = JsonConvert.DeserializeObject<backend.models.Connection>(data_json.ToString());
                connections.Add(connection);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
        }



        private static void IncomingMessageHandlerRoute(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");

            Route route = new Route();

            try
            {
                route = JsonConvert.DeserializeObject<Route>(data_json.ToString());
                routes.Add(route);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            
        }
        
        private static void IncomingMessageHandlerGateWay(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");

            try
            {
                Console.WriteLine(data_json);
                var gateway = JsonConvert.DeserializeObject<Gateway>(data_json.ToString());
                gateways.Add(gateway);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            
        }

        private static void IncomingMessageHandlerLeaf(object sender, MsgHandlerEventArgs e)
        {
            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");

            try
            {
                Console.WriteLine(data_json);
                var leaf = JsonConvert.DeserializeObject<Leaf>(data_json.ToString());
                leafs.Add(leaf);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            
        }
        
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
=======
                .ConfigureWebHostDefaults(webBuilder => 
                { 
                    webBuilder.UseStartup<Startup>(); 
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(eventParser.dataStorage);
                });
>>>>>>> Refactor entire program into appropriate classes
    }
}
