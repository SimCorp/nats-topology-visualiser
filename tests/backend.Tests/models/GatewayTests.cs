using Xunit;
using backend.models;

namespace backend.Tests
{
    public class GatewayTests
    {
        [Fact]
        public void Test1()
        {
            Gateway gateway = new Gateway();

            Assert.Null(gateway.name);
        }
    }
}