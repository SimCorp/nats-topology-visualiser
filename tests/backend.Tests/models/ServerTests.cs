using Xunit;
using backend.models;

namespace backend.Tests
{
    public class ServerTests
    {
        [Fact]
        public void Test1()
        {
            Server server = new Server();

            Assert.Null(server.connection);
        }
    }
}