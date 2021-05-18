using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Xunit;
using Moq;
using backend;
using backend.drawables;
using backend.models;

namespace backend.Tests
{
    public class ControllerTest
    {

        [Fact]
        public void GetNodesTest()
        {
            
            ServerNode testNode = new ServerNode();
            
            var mockStorage = new DataStorage()
            {
                processedServers = { testNode }
            };
            
            Controller testController = new Controller(mockStorage);

            Assert.Contains(testNode, testController.GetNodes().Value);
        }

        [Fact]
        public void GetGatewayErrorsTest()
        {
            var testList = new List<string>();

            var mockStorage = new DataStorage()
            {
                clusterConnectionErrors = {{"test", testList}}
            };
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetGatewayErrors().Value;
            
            Assert.Equal(testList, result["test"]);
        }

        [Fact]
        public void GetClustersTest()
        {
            var testCluster = new ClusterNode()
            {
                name = "test",
                servers = new ConcurrentBag<ServerNode>()
            };
            
            var mockStorage = new DataStorage()
            {
                processedClusters = {testCluster}
            };
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetClusters().Value;

            Assert.Contains(testCluster, result);
        }

        [Fact]
        public void GetIpToServerIdTest()
        {
            var mockStorage = new DataStorage()
            {
                ipToServerId = {{"test", "1337"}}
            };
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetIpToServerId().Value;
            
            Assert.Equal("1337", result["test"]);
        }
        
        [Fact]
        public void GetLeafLinksTest()
        {
            var leafLink = new LeafLink("origin", "target");
            
            var mockStorage = new DataStorage()
            {
                leafLinks = {leafLink}
            };            

            Controller testController = new Controller(mockStorage);

            var result = testController.GetLeafLinks().Value;

            Assert.Contains(leafLink, result);
        }
        
        [Fact]
        public void GetGatewayLinksTest()
        {
            var testGatewayLink = new GatewayLink("test", "test", false);
            var mockStorage = new DataStorage()
            {
                gatewayLinks = {testGatewayLink}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetGatewayLinks().Value;

            Assert.Contains(testGatewayLink, result);
        }
        
        [Fact]
        public void GetVarzTest()
        {
            var testVar = new Server();
            
            var mockStorage = new DataStorage()
            {
                servers = {testVar}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetVarz().Value;

            Assert.Contains(testVar, result);    
        }
        
        [Fact]
        public void GetVarzSpecificTest()
        {
            var testVar = new Server()
            {
                server_id = "1337"
            };
            
            var mockStorage = new DataStorage()
            {
                servers = {testVar}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetVarz("1337").Value;

            Assert.Equal(testVar, result);
        }
        
        [Fact]
        public void GetVarzSpecificFailsTest()
        {
            var testVar = new Server()
            {
                server_id = "1337"
            };
            
            var mockStorage = new DataStorage()
            {
                servers = {testVar}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetVarz("1336").Value;

            Assert.Null(result);
        }
        
        [Fact]
        public void GetConnzTest()
        {
            var testConn = new Connection();
            
            var mockStorage = new DataStorage()
            {
                connections = {testConn}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetConnz().Value;

            Assert.Contains(testConn, result);
        }
        
        [Fact]
        public void GetLinksTest()
        {
            var expected = new Link("test", "test", false);
            
            var mockStorage = new DataStorage()
            {
                links = {expected}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetLinks().Value;

            Assert.Contains(expected, result);
        }
        
        [Fact]
        public void GetRoutezTest()
        {
            var expected = new Route();
            
            var mockStorage = new DataStorage()
            {
                routes = {expected}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetRoutez().Value;

            Assert.Contains(expected, result);
        }
        
        [Fact]
        public void GetGatewayzTest()
        {
            var expected = new Gateway();
            
            var mockStorage = new DataStorage()
            {
                gateways = {expected}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetGatewayz().Value;

            Assert.Contains(expected, result);
        }
        
        [Fact]
        public void GetLeafzTest()
        {
            var expected = new Leaf();
            
            var mockStorage = new DataStorage()
            {
                leafs = {expected}
            };            
            
            Controller testController = new Controller(mockStorage);

            var result = testController.GetLeafz().Value;

            Assert.Contains(expected, result);
        }
        
    }
}