using Xunit;
using System.Diagnostics;
using System.Threading;
using System;
using NATS.Client;

namespace backend.Tests
{
    public class Tests
    {

        private static readonly Uri Nats = new Uri("localhost:4222/");
        
        [Fact]
        public void Test()
        {

            Program program = new Program();
            DataStorage dataStorage = new DataStorage();
            MessageHandler messageHandler = new MessageHandler(dataStorage);


            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = "nats-server";
            //startInfo.Arguments = "-p 4224 -a localhost -c server1.config";
            //startInfo.CreateNoWindow = false;

            //Uri Nats = new Uri("nats://localhost:4222");
            Uri Nats = new Uri("localhost:4222");

            //using (Process exeProcess = Process.Start(startInfo))
            //{
                var options = ConnectionFactory.GetDefaultOptions();
                options.Url = Nats.OriginalString;
                options.User = "admin";
                options.Password = "changeit";

                using (var connection = new ConnectionFactory().CreateConnection(options))
                {
    
                    var inbox = connection.NewInbox();

                    using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerServer))
                    {
                        subscription.Start();
                        connection.Publish("$SYS.REQ.SERVER.PING.VARZ", inbox, new byte[0]);
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }

              //  exeProcess.Kill();
            //}

            Console.WriteLine(messageHandler.dataStorage.servers.Count);

            //Assert.Equal(1, dataStorage.servers.Count);
            Assert.Equal("2.2.0", messageHandler.dataStorage.servers[0].version);
            Assert.Equal(1, messageHandler.dataStorage.servers[0].proto);
            




        }

    }
}
