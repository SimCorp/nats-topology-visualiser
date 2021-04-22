using Xunit;
using System.Diagnostics;
using System.Threading;
using System;
using NATS.Client;

namespace backend.Tests
{
    public class IntegrationTests
    {

        private static readonly Uri Nats = new Uri("localhost:4222/");

        [Fact]
        public void Test()
        {
            Assert.True(true);

            DataStorage dataStorage = new DataStorage();
            //MessageHandler messageHandler = new MessageHandler(dataStorage);
            
            
            Uri Nats = new Uri("localhost:4222");

            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = Nats.OriginalString;
            options.User = "admin";
            options.Password = "changeit";
            
            EventParser eventParser = new EventParser(dataStorage, options);

            Console.WriteLine(eventParser.dataStorage.servers.Count);

            Assert.Equal(1, eventParser.dataStorage.servers.Count);
            //Assert.Equal("2.2.0", messageHandler.dataStorage.servers[0].version);
            //Assert.Equal(1, messageHandler.dataStorage.servers[0].proto);

        }

    }
}