using System;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using backend.models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Connection = NATS.Client.Connection;
using System.IO;
using backend.helpers;

using Env = System.Environment;

namespace backend
{
    public class Logic
    {
        public DataStorage dataStorage;

        public Logic(DataStorage dataStorage) {
            this.dataStorage = dataStorage;

            SetEnv();
        }


        public void Startup()
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Env.GetEnvironmentVariable("NATS_URL");
        
            MessageHandler messageHandler = new MessageHandler(dataStorage);

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                Console.WriteLine(connection.State);

                var inbox = connection.NewInbox();

                using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerServer))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.VARZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerConnection))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.CONNZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

                using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerRoute))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.ROUTEZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
                
                using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerGateWay))
                {
                    subscription.Start();
                    connection.Publish("$SYS.REQ.SERVER.PING.GATEWAYZ", inbox, new byte[0]);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }

            }

            Parallel.ForEach(dataStorage.connections, connection =>
            {
                if (dataStorage.idToServer.TryGetValue(connection.server_id, out var s))
                {
                    s.connection = connection;
                    //Add does not overwrite, so the below code is to overwrite the map entry.
                    dataStorage.idToServer[connection.server_id] = s;
                }
            });

            Parallel.ForEach(dataStorage.routes, route =>
            {
                if (dataStorage.idToServer.TryGetValue(route.server_id, out var s))
                {
                    s.route = route;
                    dataStorage.idToServer[s.server_id] = s;
                }
            });
            
            Parallel.ForEach(dataStorage.gateways, gateway =>
            {
                if (dataStorage.idToServer.TryGetValue(gateway.server_id, out var s))
                {
                    Console.WriteLine("G id: " + gateway.server_id);
                    s.gateway = new Gateway();
                    s.gateway = gateway;
                    dataStorage.idToServer[s.server_id] = s;
                }
            });
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



    }
}