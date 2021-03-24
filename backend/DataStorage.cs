using System;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using backend.models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace backend
{
    public class DataStorage
    {

        public ConcurrentDictionary<string, Server> idToServer;
        public List<Server> servers;
        public ConcurrentBag<backend.models.Connection> connections;
        public ConcurrentBag<Route> routes;
        public ConcurrentBag<Gateway> gateways;


        public DataStorage() {

            idToServer = new ConcurrentDictionary<string, Server>();

            servers = new List<Server>();

            connections = new ConcurrentBag<backend.models.Connection>();

            routes = new ConcurrentBag<Route>();

            gateways = new ConcurrentBag<Gateway>();
        }

    }

}