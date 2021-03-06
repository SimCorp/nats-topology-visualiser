using System;
using System.IO;
using Xunit;
using backend.helpers;

namespace backend.Tests
{
    public class HelperTests
    {

        [Fact]
        public void EnvTestWorks()
        {
            FileStream fs = new FileStream("test.txt", FileMode.Append, FileAccess.Write);
            StreamWriter textWriter = new StreamWriter(fs);

            textWriter.WriteLine("TESTVAR=JOHN");
            textWriter.Close();

            DotEnv.Load("test.txt");

            Assert.Equal("JOHN", Environment.GetEnvironmentVariable("TESTVAR"));
        }

        [Fact]
        public void EnvTestDoesntTake()
        {
            FileStream fs = new FileStream("test.txt", FileMode.Append, FileAccess.Write);
            StreamWriter textWriter = new StreamWriter(fs);

            textWriter.WriteLine("TESTVAR2=JO=HN");
            textWriter.Close();

            DotEnv.Load("test.txt");

            Assert.Null(Environment.GetEnvironmentVariable("TESTVAR2"));
        }
    }
}