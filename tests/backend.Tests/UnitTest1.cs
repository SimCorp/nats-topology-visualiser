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
        public void Test1()
        {
            Assert.True(true);
        }


        //Source: https://stackoverflow.com/questions/9679375/run-an-exe-from-c-sharp-code - (Logan B. Lehman)
        [Fact]
        public void Test2()
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "nats-server";
            startInfo.CreateNoWindow = false;

            // Start the process with the info we specified.
            // Call WaitForExit and then the using statement will close.
            using (Process exeProcess = Process.Start(startInfo))
            {
                var options = ConnectionFactory.GetDefaultOptions();
                options.Url = Nats.OriginalString;

                using (var connection = new ConnectionFactory().CreateConnection(options))
                {
    
                    Assert.True(connection.State.ToString().Equals("CONNECTED"));

                    /*var inbox = connection.NewInbox();

                    using (var subscription = connection.SubscribeAsync(inbox, Program.IncomingMessageHandlerRawJson))
                    {
                        subscription.Start();
                        connection.Publish("$SYS.REQ.SERVER.PING.VARZ", inbox, new byte[0]);
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }*/
                }

                exeProcess.Kill();
            }
        }


        [Fact]
        public void Test3()
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

                using (var connection = new ConnectionFactory().CreateConnection(options))
                {
    
                    var inbox = connection.NewInbox();

                    using (var subscription = connection.SubscribeAsync(inbox, messageHandler.IncomingMessageHandlerRawJson))
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
