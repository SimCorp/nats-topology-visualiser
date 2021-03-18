using System;
using System.Text;
using System.Threading;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using backend.models;
using Connection = NATS.Client.Connection;

namespace backend
{
    class Program
    {
        private static readonly Uri Nats =
            new Uri("nats://sysadmin:zZn6MvjhbSP8RG9f@nats1.westeurope.cloudapp.azure.com:4222/");

        //private static readonly List<Type> types = new List<Type>() { typeof(Server), typeof(Connection) };
        public static Dictionary<string, Server> serverMap;
        public static List<Server> Servers = new List<Server>();
        public static List<backend.models.Connection> Connections = new List<backend.models.Connection>();
        public static List<Route> Routes = new List<Route>();

        private static void Main(string[] args)
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Nats.OriginalString;
            
            serverMap = new Dictionary<string, Server>();

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

            }

            foreach (var c in Connections)
            {
                if (serverMap.TryGetValue(c.server_id, out var s))
                {
                    s.connectionsList.Add(c);
                    //Add does not overwrite, so the below code is to overwrite the map entry.
                    serverMap[c.server_id] = s;

                }
            }
            
            // I don't really understand how routes work - they don't have a matching server_id as far as I can tell
            // 
            /*foreach (Route r in Routes)
            {
                Console.WriteLine("remote id: " );
                if (serverMap.TryGetValue(r.remote_id, out var s))
                {
                    s.routesList.Add(r);
                    serverMap[s.server_id] = s;
                }
            }*/

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
            //var server_json = token.SelectToken("server");

            Server server = new Server();

            try
            {
                server = JsonConvert.DeserializeObject<Server>(data_json.ToString());
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            serverMap.Add(server.server_id, server);
            Servers.Add(server);
        }

        private static void IncomingMessageHandlerConnection(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");
            //var server_json = token.SelectToken("server");

            backend.models.Connection connection = new backend.models.Connection();

            try
            {
                connection = JsonConvert.DeserializeObject<backend.models.Connection>(data_json.ToString());
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }

            Connections.Add(connection);
        }



        private static void IncomingMessageHandlerRoute(object sender, MsgHandlerEventArgs e)
        {
            Console.WriteLine(e.Message.ToString());

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            var data_json = token.SelectToken("data");
            //var server_json = token.SelectToken("server");

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


    }
}