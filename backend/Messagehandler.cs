using System;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using backend.models;

namespace backend
{
    public class MessageHandler
    {
        public DataStorage dataStorage;
        public MessageHandler(DataStorage dataStorage) {
            this.dataStorage = dataStorage;
        }

        public void IncomingMessageHandlerRawJson(object sender, MsgHandlerEventArgs e)
        {
            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            Console.WriteLine(e.Message.ArrivalSubscription.Subject);

            Console.WriteLine(json.ToString());
        }

        private JToken ParseData(MsgHandlerEventArgs e)
        {
            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            JToken token = JObject.Parse(json.ToString());

            return token.SelectToken("data");
        }

        public void IncomingMessageHandlerServer(object sender, MsgHandlerEventArgs e)
        {
            var data_json = ParseData(e);

            Server server = new Server();

            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            try
            {
                server = JsonConvert.DeserializeObject<Server>(data_json.ToString());
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
            dataStorage.idToServer.TryAdd(server.server_id, server);
            dataStorage.servers.Add(server);
        }

        public void IncomingMessageHandlerConnection(object sender, MsgHandlerEventArgs e)
        {
            var data_json = ParseData(e);

            backend.models.Connection connection = new backend.models.Connection();

            try
            {
                connection = JsonConvert.DeserializeObject<backend.models.Connection>(data_json.ToString());
                dataStorage.connections.Add(connection);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
        }

        public void IncomingMessageHandlerRoute(object sender, MsgHandlerEventArgs e)
        {
            var data_json = ParseData(e);

            Route route = new Route();

            try
            {
                route = JsonConvert.DeserializeObject<Route>(data_json.ToString());
                dataStorage.routes.Add(route);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }  
        }
        
        public void IncomingMessageHandlerGateWay(object sender, MsgHandlerEventArgs e)
        {
            var data_json = ParseData(e);

            try
            {
                var gateway = JsonConvert.DeserializeObject<Gateway>(data_json.ToString());
                dataStorage.gateways.Add(gateway);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }  
        }

        public void IncomingMessageHandlerLeaf(object sender, MsgHandlerEventArgs e)
        {
            var data_json = ParseData(e);

            try
            {
                var leaf = JsonConvert.DeserializeObject<Leaf>(data_json.ToString());
                dataStorage.leafs.Add(leaf);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.StackTrace);
            }
        }

    }
}
