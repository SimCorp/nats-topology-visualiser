using System;
using System.IO;
using System.Threading;
using NATS.Client;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using backend.helpers;
using Env = System.Environment;
namespace backend
{
    class EventParser
    {
        public DataStorage dataStorage;

        public EventParser(DataStorage dataStorage) {
            this.dataStorage = dataStorage;

            SetEnv();
        }

        public void Parse()
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Env.GetEnvironmentVariable("NATS_URL");

            MessageHandler messageHandler = new MessageHandler(dataStorage);

            using (var connection = new ConnectionFactory().CreateConnection(options))
            {
                var inbox = connection.NewInbox();

                Subscribe(inbox, messageHandler.IncomingMessageHandlerServer, "$SYS.REQ.SERVER.PING.VARZ", connection);
                Subscribe(inbox, messageHandler.IncomingMessageHandlerConnection, "$SYS.REQ.SERVER.PING.CONNZ", connection);
                Subscribe(inbox, messageHandler.IncomingMessageHandlerRoute, "$SYS.REQ.SERVER.PING.ROUTEZ", connection);
                Subscribe(inbox, messageHandler.IncomingMessageHandlerGateWay, "$SYS.REQ.SERVER.PING.GATEWAYZ", connection);
                Subscribe(inbox, messageHandler.IncomingMessageHandlerLeaf, "$SYS.REQ.SERVER.PING.LEAFZ", connection);
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
                    s.gateway = new Gateway();
                    s.gateway = gateway;
                    dataStorage.idToServer[s.server_id] = s;
                }
            });

            Parallel.ForEach(dataStorage.leafs, leaf =>
            {
                if (dataStorage.idToServer.TryGetValue(leaf.server_id, out var s))
                {
                    s.leaf = leaf;
                    dataStorage.idToServer[s.server_id] = s;
                }
            });

        }

        public void Subscribe(string inbox, EventHandler<MsgHandlerEventArgs> handler, string subject, IConnection connection){
            using(var subscription = connection.SubscribeAsync(inbox, handler)) {
                subscription.Start();
                connection.Publish(subject, inbox, new byte[0]);
                Thread.Sleep(TimeSpan.FromSeconds(2));
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
    }
}
