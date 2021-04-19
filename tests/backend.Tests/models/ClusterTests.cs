using Xunit;
using backend.models;

namespace backend.Tests
{
    public class ClusterTests
    {
        [Fact]
        public void Test1()
        {
            Cluster cluster = new Cluster();

            Assert.Null(cluster.name);
        }
    }
}
