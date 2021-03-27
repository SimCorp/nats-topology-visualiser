using System;
using System.IO;
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
using backend.helpers;
using System.Collections.Concurrent;
using Connection = NATS.Client.Connection;
<<<<<<< HEAD
<<<<<<< HEAD
using Env = System.Environment;
=======
=======
>>>>>>> 1bb92b0ef76a8072405a057efb58c8f8617f0b4a
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

<<<<<<< HEAD
>>>>>>> 124075b (Refactor Program.cs into appropriate classes)
=======
>>>>>>> 1bb92b0ef76a8072405a057efb58c8f8617f0b4a
namespace backend
{
    public class Program
    {
<<<<<<< HEAD
<<<<<<< HEAD
        public static ConcurrentDictionary<string, Server> idToServer;
        public static List<Server> servers = new List<Server>();
        public static ConcurrentBag<backend.models.Connection> connections = new ConcurrentBag<backend.models.Connection>();
        public static ConcurrentBag<Route> routes = new ConcurrentBag<Route>();
        public static ConcurrentBag<Gateway> gateways = new ConcurrentBag<Gateway>();
        public static ConcurrentBag<Leaf> leafs = new ConcurrentBag<Leaf>();

=======
>>>>>>> 124075b (Refactor Program.cs into appropriate classes)
=======
>>>>>>> 1bb92b0ef76a8072405a057efb58c8f8617f0b4a

        public static Logic logic;
        private static void Main(string[] args)
        {
<<<<<<< HEAD
<<<<<<< HEAD
            SetEnv();

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Env.GetEnvironmentVariable("NATS_URL");
            idToServer = new ConcurrentDictionary<string, Server>();

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                Console.WriteLine(connection.State);

                var inbox = connection.NewInbox();

                using (var subscription = connection.SubscribeAsync(inbox, IncomingMessageHandlerServer))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.VARZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                using (var subscription = connection.SubscribeAsync(inbox, IncomingMessageHandlerConnection))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.CONNZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                using (var subscription = connection.SubscribeAsync(inbox, IncomingMessageHandlerRoute))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.ROUTEZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
                
                using (var subscription = connection.SubscribeAsync(inbox, IncomingMessageHandlerGateWay))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.GATEWAYZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                using (var subscription = connection.SubscribeAsync(inbox, IncomingMessageHandlerLeaf))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.LEAFZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

            }

            Parallel.ForEach(connections, connection =>
            {
                if (idToServer.TryGetValue(connection.server_id, out var s))
                {
                    s.connection = connection;
                    //Add does not overwrite, so the below code is to overwrite the map entry.
                    idToServer[connection.server_id] = s;
                }
            });

            Parallel.ForEach(routes, route =>
            {
                if (idToServer.TryGetValue(route.server_id, out var s))
                {
                    s.route = route;
                    idToServer[s.server_id] = s;
                }
            });

            Parallel.ForEach(gateways, gateway =>
            {
                if (idToServer.TryGetValue(gateway.server_id, out var s))
                {
                    Console.WriteLine("G id: " + gateway.server_id);
                    s.gateway = new Gateway();
                    s.gateway = gateway;
                    idToServer[s.server_id] = s;
                }
            });

            Parallel.ForEach(leafs, leaf =>
            {
                if (idToServer.TryGetValue(leaf.server_id, out var s))
                {
                    s.leaf = leaf;
                    idToServer[s.server_id] = s;
                }
            });

            CreateHostBuilder(args).Build().Run();
=======
            logic = new Logic(new DataStorage());
            logic.Startup();

            var host = CreateHostBuilder(args).Build();

            host.Run();
>>>>>>> 124075b (Refactor Program.cs into appropriate classes)
=======
            logic = new Logic(new DataStorage());
            logic.Startup();

            var host = CreateHostBuilder(args).Build();

            host.Run();
>>>>>>> 1bb92b0ef76a8072405a057efb58c8f8617f0b4a
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
<<<<<<< HEAD


        
<<<<<<< HEAD
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
=======
>>>>>>> 124075b (Refactor Program.cs into appropriate classes)
        
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


        
        
>>>>>>> 1bb92b0ef76a8072405a057efb58c8f8617f0b4a
    }
}