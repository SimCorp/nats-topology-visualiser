using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.models;
using System;
using System.Threading;
using NATS.Client;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using backend.drawables;

namespace backend
{
    public class NetworkInstance
    {
        public int id { get; set; }
        public ConcurrentBag<Link> links = new ConcurrentBag<Link>();
        public ConcurrentBag<Node> processedServers = new ConcurrentBag<Node>();
        public HashSet<string> missingServerIds = new HashSet<string>();
        public ConcurrentDictionary<string, Server> idToServer = new ConcurrentDictionary<string, Server>();
        public ConcurrentBag<Server> servers = new ConcurrentBag<Server>();
        public ConcurrentBag<backend.models.Connection> connections = new ConcurrentBag<backend.models.Connection>();
        public ConcurrentBag<Route> routes = new ConcurrentBag<Route>();
        public ConcurrentBag<Gateway> gateways = new ConcurrentBag<Gateway>();
        public ConcurrentBag<Leaf> leafs = new ConcurrentBag<Leaf>();

        public Options options = ConnectionFactory.GetDefaultOptions();

        public NetworkInstance (int id, string url)
        {
            this.id = id;
            options.Url = url;
        }

        private void IncomingMessageHandlerRawJson(object sender, MsgHandlerEventArgs e)
        {
            var json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Message.Data));

            Console.WriteLine(e.Message.ArrivalSubscription.Subject);

            Console.WriteLine(json.ToString());
        }

        private void IncomingMessageHandlerServer(object sender, MsgHandlerEventArgs e)
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

        private void IncomingMessageHandlerConnection(object sender, MsgHandlerEventArgs e)
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



        private void IncomingMessageHandlerRoute(object sender, MsgHandlerEventArgs e)
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
        
        private void IncomingMessageHandlerGateWay(object sender, MsgHandlerEventArgs e)
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

        private void IncomingMessageHandlerLeaf(object sender, MsgHandlerEventArgs e)
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

        public void GetInformation()
        {
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

            ProcessData();
        }


        public void ProcessData() 
        {
            Parallel.ForEach(servers, server => {
                processedServers.Add(new Node {
                    server_id = server.server_id, 
                    server_name = server.server_name, 
                    ntv_error = false 
                });
            });

            // Information about routes are also on server, no request to routez necessary
            // Maybe info on routes is more up-to-date?
            Parallel.ForEach(routes, server => {
                var source = server.server_id;
                Parallel.ForEach(server.routes, route => {
                    var target = route.remote_id;
                    links.Add(new Link(source, target));
                });
            });

            foreach (var link in links) 
            {
                if (!idToServer.ContainsKey(link.target))
                {
                    missingServerIds.Add(link.target);
                    link.ntv_error = true;
                    links.Add(link);
                }
            }

            // Patch for a missing node from varz
            // TODO dynamically handle these types of errors
            foreach(var serverId in missingServerIds)
            {
                processedServers.Add(new Node{server_id = serverId, ntv_error = true});
            }

        }
    }
}