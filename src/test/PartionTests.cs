using System;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.CouchType;
using Xunit;

namespace PillowSharp.Tests
{
    public class PartionTests : BaseTest, IDisposable
    {
        public PartionTests() : base("partion_tests")
        {
        }
        [Fact]
        public async Task GetPartionInfo()
        {
            var client = GetTestClient(BaseObject.ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB, Partitioned: true);
            var testDoc = TestDocument.GenerateTestObject();
            testDoc.ID = "test_partion:1234";
            var result = await client.CreateANewDocumentAsync(testDoc, TestDB);
            Assert.True(result.Ok);
            var partitionInfo = await client.GetPartitionInfoAsync("test_partion");
            Assert.NotNull(partitionInfo);
            Assert.Equal(TestDB, partitionInfo.DbName);
            Assert.NotNull(partitionInfo.Sizes);
            Assert.True(partitionInfo.Sizes.Active >= 0);
            Assert.True(partitionInfo.Sizes.External >= 0);
            Assert.True(partitionInfo.DocCount > 0);
            Assert.True(partitionInfo.DocDelCount >= 0);
        }

        [Fact]
        public async Task GetPartionInfo_OfNonExistingPartion()
        {
            // Arrange
            var client = GetTestClient(BaseObject.ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB, Partitioned: true);
            // Act & Assert
            var result = await client.GetPartitionInfoAsync("non_existent_partion");
            // check that the result has zero doccount
            Assert.NotNull(result);
            Assert.Equal(0, result.DocCount);
        }

        [Fact]
        public async Task GetAllDocumentsOfPartition()
        {
            // Arrange
            var client = GetTestClient(BaseObject.ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB, Partitioned: true);

            var testDoc1 = TestDocument.GenerateTestObject();
            testDoc1.ID = "test_partion:1234";
            var result1 = await client.CreateANewDocumentAsync(testDoc1, TestDB);
            Assert.True(result1.Ok);

            var testDoc2 = TestDocument.GenerateTestObject();
            testDoc2.ID = "test_partion:5678";
            var result2 = await client.CreateANewDocumentAsync(testDoc2, TestDB);
            Assert.True(result2.Ok);

            // Act
            var documents = await client.GetAllDocumentsAsync(DatabaseToUse: TestDB, PartitionToUse: "test_partion");

            // Assert
            Assert.NotNull(documents);
            Assert.Equal(2, documents.Rows.Count);
            Assert.Contains(documents.Rows, doc => doc.ID == "test_partion:1234");
            Assert.Contains(documents.Rows, doc => doc.ID == "test_partion:5678");
        }

        [Fact]
        public async Task GetViewAsync_WithPartition()
        {
            // Arrange
            var client = GetTestClient(BaseObject.ELoginTypes.BasicAuth);
            await client.CreateNewDatabaseAsync(TestDB, Partitioned: true);

            var testDoc1 = TestDocument.GenerateTestObject();
            testDoc1.ID = "test_partition:1234";
            testDoc1.StringProp = "test";
            var result1 = await client.CreateANewDocumentAsync(testDoc1, TestDB);
            Assert.True(result1.Ok);

            var testDoc2 = TestDocument.GenerateTestObject();
            testDoc2.ID = "test_partition:5678";
            testDoc2.StringProp = "test";
            var result2 = await client.CreateANewDocumentAsync(testDoc2, TestDB);
            Assert.True(result2.Ok);

            var designDoc = new CouchDesignDocument()
            {
                ID = "_design/test_design",
            };
            designDoc.TogglePartitionedState();
            designDoc.AddView("test_view", "function(doc) { if (doc.StringProp === 'test') { emit(doc._id, null); } }");
            var designDocResult = await client.UpsertDesignDocumentAsync(designDoc, TestDB);
            Assert.True(designDocResult.Ok);
            // Retrieve the design document
            var retrievedDesignDoc = await client.GetDesignDocumentAsync("_design/test_design", TestDB);

            // Assert that the design document is partitioned
            Assert.NotNull(retrievedDesignDoc);
            Assert.True(retrievedDesignDoc.Options.ContainsKey("partitioned"));
            Assert.True((bool)retrievedDesignDoc.Options["partitioned"]);

            // Act
            var viewResult = await client.GetViewAsync<CouchViewResponse<object>>("test_design", "test_view", partition: "test_partition");

            // Assert
            Assert.NotNull(viewResult);
            Assert.Equal(2, viewResult.Rows.Count);
            Assert.Contains(viewResult.Rows, row => row.ID == "test_partition:1234");
            Assert.Contains(viewResult.Rows, row => row.ID == "test_partition:5678");
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