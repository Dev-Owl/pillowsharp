using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using Xunit;

namespace PillowSharp.Tests
{
    public class DatabaseTest : BaseTest, IDisposable
    {
        public List<string> CleanUpList { get; set; } = new List<string>();

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

        [Fact]
        public async Task TestGetSchedulerJobsEmpty()
        {
            // Arrange
            var client = GetTestClient(ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB);
            // Act
            var response = await client.GetSchedulerJobsAsync();

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Jobs);
            Assert.Empty(response.Jobs);
            Assert.Equal(0, response.Offset);
            Assert.Equal(0, response.TotalRows);
        }

        [Fact]
        public async Task TestGetSchedulerJobsNotEmpty()
        {
            // Arrange
            var client = GetTestClient(ELoginTypes.BasicAuth);
            // Source
            await client.CreateNewDatabaseAsync(TestDB);
            const string targetDbName = "test_scheduler_jobs";
            CleanUpList.Add(targetDbName);
            // Target
            await client.CreateNewDatabaseAsync(targetDbName);


            var uriBuilder = new UriBuilder(client.ServerConfiguration.GetServerURL())
            {
                UserName = client.ServerConfiguration.GetLoginData().UserName,
                Password = client.ServerConfiguration.GetLoginData().Password,
                Path = "/" + TestDB
            };
            var source = uriBuilder.Uri.ToString();
            var target = uriBuilder.Uri.ToString().Replace(TestDB, targetDbName);

            // Create a replication job between TestDB and targetDbName
            var replicationRequest = new ReplicationRequest
            {
                Source = source,
                Target = target,
                Continuous = true
            };
            await client.CreateReplicationAsync(replicationRequest);


            // Act
            var response = await client.GetSchedulerJobsAsync();

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Jobs);
            Assert.NotEmpty(response.Jobs);
            Assert.Equal(0, response.Offset);
            Assert.Equal(1, response.TotalRows);
            uriBuilder.UserName = null;
            uriBuilder.Password = null;
            var sourceFromJob = response.Jobs[0].Source;
            if (sourceFromJob.EndsWith("/"))
            {
                sourceFromJob = sourceFromJob.Substring(0, sourceFromJob.Length - 1);
            }
            var targetFromJob = response.Jobs[0].Target;
            if (targetFromJob.EndsWith("/"))
            {
                targetFromJob = targetFromJob.Substring(0, targetFromJob.Length - 1);
            }
            Assert.Equal(uriBuilder.Uri.ToString(), sourceFromJob);
            Assert.Equal(uriBuilder.Uri.ToString().Replace(TestDB, targetDbName), targetFromJob);
        }

        public void Dispose()
        {
            if (!CouchSettings.SkipAuthTests)
            {
                var client = GetTestClient();
                client.DeleteDatabaseAsync(TestDB).Wait();
                foreach (var dbName in CleanUpList)
                {
                    client.DeleteDatabaseAsync(dbName).Wait();
                }
            }
        }


    }
}