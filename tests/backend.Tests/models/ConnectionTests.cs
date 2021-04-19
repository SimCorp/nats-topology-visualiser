using Xunit;
using Moq;
using backend.models;

namespace backend.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void Test1()
        {
            Connection conn = new Connection();

            Assert.Equal(0, conn.num_connections);
        }
    }
}
