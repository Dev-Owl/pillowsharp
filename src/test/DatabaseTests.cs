using System;
using System.Formats.Asn1;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using Xunit;

namespace PillowSharp.Tests
{
    public class DatabaseTest : BaseTest, IDisposable
    {
        public DatabaseTest() : base("database_checks")
        {
        }

        [Fact]
        public async Task TestGetDatabaseInfo()
        {
            var client = GetTestClient(ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB);
            var databaseInfo = await client.GetDatabaseInfoAsync(TestDB);
            Assert.NotNull(databaseInfo);
            Assert.Equal(TestDB, databaseInfo.DbName);
            Assert.Equal(0, databaseInfo.DocCount);
            Assert.NotNull(databaseInfo.Cluster);
            Assert.NotNull(databaseInfo.Sizes);
            Assert.Empty(databaseInfo.Props);
            await client.DeleteDatabaseAsync(TestDB);
            await client.CreateNewDatabaseAsync(TestDB, Partitioned: true);
            databaseInfo = await client.GetDatabaseInfoAsync(TestDB);
            Assert.NotEmpty(databaseInfo.Props);
            Assert.True(databaseInfo.Props.ContainsKey(CouchDbInfo.DbPropsPartitioned));
            Assert.IsType<bool>(databaseInfo.Props[CouchDbInfo.DbPropsPartitioned]);
            Assert.True((bool)databaseInfo.Props[CouchDbInfo.DbPropsPartitioned]);
        }

        public void Dispose()
        {
            if (!CouchSettings.SkipAuthTests)
            {
                var client = GetTestClient();
                client.DeleteDatabaseAsync(TestDB).Wait();
            }
        }
    }
}