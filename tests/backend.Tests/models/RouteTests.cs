using Xunit;
using backend.models;

namespace backend.Tests
{
    public class RouteTests
    {
        [Fact]
        public void Test1()
        {
            Route route = new Route();

            Assert.Equal(0, route.num_routes);
        }
    }
}