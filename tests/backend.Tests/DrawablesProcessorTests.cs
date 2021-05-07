using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Xunit;
using backend;
using backend.models;


namespace backend.Tests
{
    public class DrawablesProcessorTests
    {
        [Fact]
        public void AddServersTest()
        {
            var data = new DataStorage();
            data.servers = new List<Server>(new Server[]
            {
                new Server {server_id = "id0", server_name = "name0"},
                new Server {server_id = "id1", server_name = "name1"},
                new Server {server_id = "id2", server_name = "name2"}
            });
            var dp = new DrawablesProcessor(data, "");
            dp.ProcessServers();
            Assert.Equal(3, data.processedServers.Count);
            Assert.Equal(3, data.foundServers.Count);
        }

        [Fact]
        public void AddConnectionsTest()
        {
            var data = new DataStorage();
            data.servers = new List<Server>(new Server[]
            {
                new Server {server_id = "id0", server_name = "name0"},
                new Server {server_id = "id1", server_name = "name1"},
                new Server {server_id = "id2", server_name = "name2"}
            });
            
            data.connections.Add(new Connection
            {
                server_id = "id0",
                num_connections = 2,
                connections = new ConcurrentBag<ConnectionEntry>(new ConnectionEntry[]
                {
                    new ConnectionEntry() { cid = 0, ip = "0", port = 0},
                    new ConnectionEntry() { cid = 1, ip = "1", port = 1}
                })
            });
            
            var dp = new DrawablesProcessor(data, "");
            dp.ProcessServers();
            var shouldHave2 = data.processedServers.Where(s => s.server_id == "id0").Select(s => s).FirstOrDefault();
            Assert.Equal(2, shouldHave2.clients.Count);
        }

        [Fact]
        public void ProcessLinksTest()
        {
            var data = new DataStorage();
            data.servers = new List<Server>(new Server[]
            {
                new Server {server_id = "id0", server_name = "name0"},
                new Server {server_id = "id1", server_name = "name1"},
                new Server {server_id = "id2", server_name = "name2"}
            });

            foreach (var server in data.servers)
            {
                data.idToServer.TryAdd(server.server_id, server);
            }
            
            data.routes = new ConcurrentBag<Route>(new Route[] {
                new Route {
                    server_id = "id0",
                    routes = new ConcurrentBag<RouteNode>(new RouteNode[] {
                        new RouteNode {
                            remote_id = "id1",
                            ip = "1234"
                        },
                        new RouteNode {
                            remote_id = "bruh",
                            ip = "2345"
                        }
                    })
                },
                new Route {
                    server_id = "id1",
                    routes = new ConcurrentBag<RouteNode>(new RouteNode[] {
                        new RouteNode {
                            remote_id = "id0",
                            ip = "3456"
                        }
                    })
                }
            });
            var dp = new DrawablesProcessor(data);

            Assert.Equal(4, data.processedServers.Count);
            foreach (var server in data.processedServers)
            {
                if (server.server_id == "bruh")
                {
                    Assert.True(server.ntv_error);
                }
                else
                {
                    Assert.False(server.ntv_error);
                }
            }
            Assert.Equal("id1", data.links[0].source);
            Assert.Equal("id0", data.links[0].target);
            Assert.False(data.links[0].ntv_error);
            
            Assert.Equal("id0", data.links[1].source);
            Assert.Equal("bruh", data.links[1].target);
            Assert.True(data.links[1].ntv_error);
            
            Assert.Equal("id0", data.links[2].source);
            Assert.Equal("id1", data.links[2].target);
            Assert.False(data.links[2].ntv_error);
        }

        [Fact]
        public void ConstructSingleLeafConnectionsTest()
        {
            var data = new DataStorage();
            data.servers = new List<Server>(new Server[]
            {
                new Server {server_id = "id0", server_name = "name0"},
                new Server {server_id = "id1", server_name = "name1"},
                new Server {server_id = "id2", server_name = "name2"}
            });

            data.ipToServerId.Add("123", "id0");
            data.ipToServerId.Add("234", "id1");
            data.ipToServerId.Add("345", "id2");

            var leafs = new List<Leaf>(new Leaf[]{
                new Leaf {
                    server_id = "id0",
                    leafs = new List<LeafNode>(new LeafNode[]{
                        new LeafNode {
                            ip = "234",
                            account = "bruh"
                        },
                        new LeafNode {
                            ip = "234",
                            account = "oline"
                        },
                        new LeafNode {
                            ip = "345",
                            account = "lmao"
                        }
                    })
                }
            });

            data.leafs = leafs;

            var dp = new DrawablesProcessor(data, "thisTextDoesNotMatter");
            dp.ConstructSingleLeafConnections();

            Assert.Equal(2, data.leafLinks.Count());
        }
        
        [Fact]
        public void ProcessClustersTest()
        {
            var data = new DataStorage();
            var gateway0 = new Gateway { 
                server_id = "id0", 
                name = "name0",
                inbound_gateways =  new Dictionary<string, List<Gateway.GatewayNodeWrapper>>(),
                outbound_gateways = new Dictionary<string, Gateway.GatewayNodeWrapper>()
            };
            gateway0.inbound_gateways.Add("cluster0", new List<Gateway.GatewayNodeWrapper>());
            gateway0.inbound_gateways["cluster0"].Add(
                new Gateway.GatewayNodeWrapper {
                    connection = new Gateway.GatewayNode {
                        name = "abcd"
                    }
                }
            );

            data.gateways.Add(gateway0);

            var gateway1 = new Gateway { 
                server_id = "id1", 
                name = "name1",
                inbound_gateways = new Dictionary<string, List<Gateway.GatewayNodeWrapper>>(),
                outbound_gateways =  new Dictionary<string, Gateway.GatewayNodeWrapper>()
            };
            gateway1.outbound_gateways.Add(
                "cluster1",
                new Gateway.GatewayNodeWrapper {
                    connection = new Gateway.GatewayNode {
                        name = "efgh"
                    }
                }
            );
            gateway1.inbound_gateways.Add("cluster1", new List<Gateway.GatewayNodeWrapper>());
            gateway1.inbound_gateways["cluster1"].Add(
                new Gateway.GatewayNodeWrapper {
                    connection = new Gateway.GatewayNode {
                        name = "qwer"
                    }
                }
            );

            data.gateways.Add(gateway1);
            
            var gateway2 = new Gateway { 
                server_id = "id2", 
                name = "name2",
                inbound_gateways = new Dictionary<string, List<Gateway.GatewayNodeWrapper>>(),
                outbound_gateways =  new Dictionary<string, Gateway.GatewayNodeWrapper>()
            };
            gateway1.outbound_gateways.Add(
                "cluster2",
                new Gateway.GatewayNodeWrapper {
                    connection = new Gateway.GatewayNode {
                        name = "ijkl"
                    }
                }
            );

            data.gateways.Add(gateway2);

            data.foundServers.Add("ijkl");

            var dp = new DrawablesProcessor(data);
            Assert.Equal(3, data.clusterConnectionErrors.Count);
            Assert.Equal(2, data.errorClusters.Count);
        }
    }
}