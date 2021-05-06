using Xunit;
using backend.models;

namespace backend.Tests
{
    public class LeafTests
    {
        [Fact]
        public void Test1()
        {
            Leaf leaf = new Leaf();

            Assert.Equal(0, leaf.leafnodes);
        }
    }
}