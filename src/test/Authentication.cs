using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middleware.Default;
using System.Threading.Tasks;

namespace PillowSharp.Tests
{
    public class AuthenticationDBCreationTests : BaseTest, IDisposable
    {
        public AuthenticationDBCreationTests() : base("authentication_test")
        {
        }

        [Theory]
        [InlineData(ELoginTypes.BasicAuth)]
        [InlineData(ELoginTypes.TokenLogin)]
        public async Task CreateDBBothAuth(ELoginTypes Type)
        {

            if (!CouchSettings.SkipAuthTests)
                await LoginCreateDB(Type);
        }

        [Fact]
        public async Task CreatePartitionedDatabase()
        {
            var client = GetTestClient(ELoginTypes.BasicAuth);
            await LoginCreateDB(ELoginTypes.BasicAuth);

        }

        public void Dispose()
        {
            if (!CouchSettings.SkipAuthTests)
            {
                var client = GetTestClient();
                client.DeleteDatabaseAsync(this.TestDB).Wait();
            }
        }

        private async Task LoginCreateDB(ELoginTypes Type, bool partitioned = false)
        {
            var client = GetTestClient(Type);
            var result = await client.CreateNewDatabaseAsync(this.TestDB, partitioned);
            Assert.True(result, $"Unable to create the db, with {Type.ToString("G")}");
        }


    }
}
