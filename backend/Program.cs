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

namespace backend
{
    class Program
    {
        private static readonly Uri Nats =
            new Uri("nats://sysadmin:zZn6MvjhbSP8RG9f@nats1.westeurope.cloudapp.azure.com:4222/");

        //private static readonly List<Type> types = new List<Type>() { typeof(Server), typeof(Connection) };
        public static ConcurrentDictionary<string, Server> serverMap;
        public static List<Server> Servers = new List<Server>();
        public static ConcurrentBag<backend.models.Connection> Connections = new ConcurrentBag<backend.models.Connection>();
        public static ConcurrentBag<Route> Routes = new ConcurrentBag<Route>();
        public static ConcurrentBag<Gateway> GateWays = new ConcurrentBag<Gateway>();

        private static void Main(string[] args)
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Nats.OriginalString;
            
            serverMap = new ConcurrentDictionary<string, Server>();

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

            }

            Parallel.ForEach(Connections, connection =>
            {
                if (serverMap.TryGetValue(connection.server_id, out var s))
                {
                    s.connectionsList.Add(connection);
                    //Add does not overwrite, so the below code is to overwrite the map entry.
                    serverMap[connection.server_id] = s;
                }
            });

            Parallel.ForEach(Routes, route =>
            {
                if (serverMap.TryGetValue(route.server_id, out var s))
                {
                    s.routesList.Add(route);
                    serverMap[s.server_id] = s;
                }
            });
            
            Parallel.ForEach(GateWays, gateway =>
            {
                if (serverMap.TryGetValue(gateway.server_id, out var s))
                {
                    Console.WriteLine("G id: " + gateway.server_id);
                    s.gateway = new Gateway();
                    s.gateway = gateway;
                    serverMap[s.server_id] = s;
                }
            });

            CreateHostBuilder(args).Build().Run();
            
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });


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
            serverMap.TryAdd(server.server_id, server);
            Servers.Add(server);
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
                Connections.Add(connection);
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
                Routes.Add(route);
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
                GateWays.Add(gateway);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            
        }
        
    }
}