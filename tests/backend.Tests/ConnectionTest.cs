using Xunit;
using Moq;
using backend.models;

namespace backend.Tests
{
    public class ConnectionTest
    {
        [Fact]
        public void Test1()
        {
            Connection conn = new Connection();

            Assert.Equal(0, conn.num_connections);
        }
    }
}
