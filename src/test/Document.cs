using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middleware.Default;
using System.Threading.Tasks;
using PillowSharp.CouchType;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace test
{
    public class DocumentTests : BaseTest, IDisposable
    {

        public TestDocument LastDocument { get; set; }

        public DocumentTests() : base("pillowtest_doc")
        {
            GetTestClient().CreateNewDatabaseAsync(this.TestDB).Wait();
        }

        [Fact]
        public void CreateDocument()
        {
            _CreateDocument().Wait();
        }

        public static async Task<TestDocument> CreateTestDocument(string Database)
        {
            var testDoc = TestDocument.GenerateTestObject();
            var client = CouchSettings.GetTestClient(Database);
            var result = await client.CreateANewDocumentAsync(testDoc, DatabaseToCreateDocumentIn: Database);
            //Ensure all needed parts are set by the client
            Assert.True(result.Ok, "Error during document creation");
            Assert.False(string.IsNullOrEmpty(testDoc.ID), "ID was not set for new document");
            Assert.False(string.IsNullOrEmpty(testDoc.Rev), "Rev was not set for new document");
            return testDoc;
        }

        [Fact]
        public void PurgeDocuments()
        {

            var testDocTask = CreateTestDocument(this.TestDB);
            testDocTask.Wait();
            var testDoc = testDocTask.Result;
            var client = CouchSettings.GetTestClient(this.TestDB);
            var result = client.PurgeDocumentsInDatabase(new PurgeRequest()
            {
                DocumentsToPurge = new List<PurgeRequestDocument>{
                    new PurgeRequestDocument(testDoc.ID, new List<string>{ testDoc.Rev})
                }
            });
            Assert.NotNull(result);
            Assert.True(result.Purged.ContainsKey(testDoc.ID));
            //Get document from db -> must fail
            var allDocs = client.GetAllDocuments();
            Assert.Empty(allDocs.Rows);
            //Purge multi revsions
            testDocTask = CreateTestDocument(this.TestDB);
            testDoc = testDocTask.Result;
            testDoc.StringProp = "Cat's everywhere";
            var firstRev = testDoc.Rev;
            client.UpdateDocument(testDoc);
            result = client.PurgeDocumentsInDatabase(new PurgeRequest()
            {
                DocumentsToPurge = new List<PurgeRequestDocument>{
                    new PurgeRequestDocument(testDoc.ID, new List<string>{testDoc.Rev,firstRev})
                }
            });
            Assert.NotNull(result);
            Assert.True(result.Purged.ContainsKey(testDoc.ID));
            allDocs = client.GetAllDocuments();
            Assert.Empty(allDocs.Rows);
        }



        private async Task _CreateDocument()
        {
            LastDocument = await CreateTestDocument(this.TestDB);
        }

        [Fact]
        public void TestQueryForAll()
        {
            _CreateDocument().Wait();
            var client = GetTestClient();
            client.RunForAllDbs((db) =>
            {
                Assert.True(client.GetAllDocuments(DatabaseToUse:db).TotalRows > 0);
            }, (db) => db == TestDB);
        }

        [Fact]
        public void GetDocument()
        {
            _CreateDocument().Wait(); // ensure document exists
            _GetDocument().Wait();
        }

        private async Task _GetDocument()
        {
            //get all
            PillowClient client = GetTestClient();
            var result = await client.GetAllDocumentsAsync(DatabaseToUse: this.TestDB);
            Assert.True(result != null, "No result from couch db");
            Assert.True(result.TotalRows > 0, "No documents returned, expected >=1");
            Assert.True(result.Rows?.Count == result.TotalRows, "Total rows where not equal returned rows!");
            Assert.True(result.Rows.FirstOrDefault() != null, "Null object returned for first row");
            var firstDocument = result.Rows.FirstOrDefault();
            Assert.False(string.IsNullOrEmpty(firstDocument.ID), "Document id was null or empty");
            Assert.False(string.IsNullOrEmpty(firstDocument.Value.Revision), "Document rev was null or empty");
            //Get single docu, latest rev
            var singleDoc = await client.GetDocumentAsync<TestDocument>(firstDocument.ID);
            Assert.True(singleDoc.AllSet(), "Missing values in created document!");
            //Get with rev number
            var singleDocRev = await client.GetDocumentAsync<TestDocument>(singleDoc.ID, singleDoc.Rev);
            Assert.True(singleDoc.Equals(singleDocRev), "Documents are differnt but should be the same");
        }



        [Fact]
        public void DeleteDocument()
        {
            _CreateDocument().Wait();
            Deletedocument().Wait();
        }

        private async Task Deletedocument()
        {
            var client = GetTestClient();
            var prevRevision = LastDocument.Rev;
            var result = await client.DeleteDocumentAsync<TestDocument>(LastDocument);
            Assert.NotNull(result);
            Assert.True(result.Ok, "Delete document returned false");
            Assert.True(LastDocument.Deleted, "Delete flag is not true");
            Assert.True(prevRevision != LastDocument.Rev, "Revision was not updated");
        }
        [Fact]
        public void UpdateDocument()
        {
            _CreateDocument().Wait();
            _UpdateDocument().Wait();
        }

        private async Task _UpdateDocument()
        {
            var client = GetTestClient();
            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument.Rev;
            LastDocument.StringProp = "Luke, I'm your father!";
            var result = await client.UpdateDocumentAsync<TestDocument>(LastDocument);
            Assert.True(result.Ok, "Update document returned false");
            Assert.True(result.ID == LastDocument.ID, "ID of updated document where not the same!");
            Assert.True(result.Rev == LastDocument.Rev, "Revision number of result is not the same as for the test document");
            Assert.False(lastRev == LastDocument.Rev, "Revision number was not changed");
            var dbObj = await client.GetDocumentAsync<TestDocument>(LastDocument.ID, LastDocument.Rev);
            Assert.False(dbObj.StringProp == prevText, "document wasnt updated as expected!");
        }
        [Fact]
        public void UpdateDocuments()
        {
            _CreateDocument().Wait();
            _UpdateDocuments().Wait();
        }

        private async Task _UpdateDocuments()
        {
            var client = GetTestClient();

            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument.Rev;
            LastDocument.StringProp = "Luke, I'm your father!";

            var documentList = new List<TestDocument>() { TestDocument.GenerateTestObject(), TestDocument.GenerateTestObject(), LastDocument };
            var result = await client.UpdateDocumentsAsync(documentList);
            Assert.True(result.Count() == documentList.Count, "Result list was not the same length as document list");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d.ID)), "Not all IDs where set as expected");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d.Rev)), "Not all revs where set as expected");
            Assert.True(result.All(r => r.Ok), "Update document returned false for at least one document");
            var changedDocResult = result.FirstOrDefault(r => r.ID == LastDocument.ID);
            Assert.NotNull(changedDocResult);
            Assert.True(changedDocResult.Rev == LastDocument.Rev, "Revision number missmatch");
            var dbObj = await client.GetDocumentAsync<TestDocument>(LastDocument.ID, LastDocument.Rev);
            Assert.False(dbObj.StringProp == prevText, "document wasnt updated as expected!");
        }


        [Fact]
        public void AddAttachment()
        {
            //Generate test file with random contens as txt
            _CreateDocument().Wait();
            var file = RandomTextFile();
            Assert.True(File.Exists(file));
            _AddAttachment(file).Wait();
        }
        private async Task _AddAttachment(string File)
        {
            var client = GetTestClient();
            var result = await client.AddAttachmentAsync(LastDocument, File, "test.txt");
            Assert.True(result.Ok, "Add attachment failed");
            Assert.True(result.Rev == LastDocument.Rev, "Revision was not updated");
        }
        private string RandomTextFile()
        {
            var testFileName = "test.txt";
            if (File.Exists(testFileName))
            {
                File.Delete(testFileName);
            }
            Random random = new Random();
            File.WriteAllText(testFileName, new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", random.Next(100, 1337))
                            .Select(s => s[random.Next(s.Length)]).ToArray()));
            return testFileName;

        }
        [Fact]
        public void GetAttachment()
        {
            _CreateDocument().Wait();
            var file = RandomTextFile();
            Assert.True(File.Exists(file));
            _AddAttachment(file).Wait();
            _GetAttachment(file).Wait();
        }

        private async Task _GetAttachment(string FilePath)
        {
            var expectedSize = File.ReadAllBytes(FilePath);
            var client = GetTestClient();
            var result = await client.GetAttachementAsync(LastDocument, "test.txt");
            Assert.True(expectedSize.Count() == result.Count(), "File size is different");
        }

        [Fact]
        public void DeleteAttachement()
        {
            try
            {
                _CreateDocument().Wait();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in _CreateDocument with {ex.ToString()}", ex);
            }
            string file = null;
            try
            {
                file = RandomTextFile();
                Assert.True(File.Exists(file));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in RandomTextFile with {ex.ToString()}", ex);
            }
            try
            {
                _AddAttachment(file).Wait();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in _AddAttachment with {ex.ToString()}", ex);
            }
            try
            {
                _DeleteAttachment().Wait();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in _DeleteAttachment with {ex.ToString()}", ex);
            }
        }

        private async Task _DeleteAttachment()
        {
            var client = GetTestClient();
            var result = await client.DeleteAttachmentAsync(LastDocument, "test.txt");
            Assert.True(result.Ok, "Returned result was not true");
            Assert.True(result.Rev == LastDocument.Rev, "Revision was not updated as expected");
        }

        public void Dispose()
        {
            GetTestClient().DeleteDatbaseAsync(this.TestDB).Wait();
        }
        [Fact]
        public void TestBaseDocumentProperties()
        {
            _CreateDocument().Wait();
            var file = RandomTextFile();
            Assert.True(File.Exists(file));
            _AddAttachment(file).Wait();
            CheckBasicProperties().Wait();
        }

        [Fact]
        public void TestDocumentHead()
        {
            _CreateDocument().Wait();
            _TestDocumentHead().Wait();
        }

        private async Task _TestDocumentHead()
        {
            var client = GetTestClient();
            var result = await client.GetCurrentDocumentRevisionAsync(LastDocument.ID, LastDocument.GetType());
            Assert.NotNull(result);
            Assert.Equal(LastDocument.Rev, result);
        }

        private async Task CheckBasicProperties()
        {
            var client = GetTestClient();
            var documentFromDB = await client.GetDocumentAsync<TestDocument>(LastDocument.ID);
            Assert.NotNull(documentFromDB.ID);
            Assert.NotNull(documentFromDB.Rev);
            Assert.False(documentFromDB.Deleted);
            Assert.NotNull(documentFromDB.Attachments);
            Assert.True(documentFromDB.Attachments.Count == 1);
            var firstAttachment = documentFromDB.Attachments.First();
            Assert.True(firstAttachment.Key == "test.txt");
            Assert.NotNull(firstAttachment.Value.ContentType);
            Assert.True(firstAttachment.Value.Length > 0);

        }

    }
}