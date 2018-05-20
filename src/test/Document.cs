using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middelware.Default;
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

        public DocumentTests():base("pillowtest_doc")
        {
            GetTestClient().CreateNewDatabase(this.TestDB).Wait();
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
            var result = await client.CreateANewDocument(testDoc,DatabaseToCreateDocumentIn:Database); 
            //Ensure all needed parts are set by the client
            Assert.True(result.Ok,"Error during document creation");
            Assert.False(string.IsNullOrEmpty( testDoc.ID ),"ID was not set for new document");
            Assert.False(string.IsNullOrEmpty(testDoc.Rev), "Rev was not set for new document");
            return testDoc;
        }

        

        private async Task _CreateDocument(){
           LastDocument =await CreateTestDocument(this.TestDB);
        }

        [Fact]
        public void GetDocument(){
            _CreateDocument().Wait(); // ensure document exists
            _GetDocument().Wait();
        }

        private async Task _GetDocument()
        {
            //get all
            PillowClient client = GetTestClient();
            var result = await client.GetAllDocuments(DatabaseToUse: this.TestDB);
            Assert.True(result !=null,"No result from couch db");
            Assert.True(result.TotalRows > 0,"No documents returned, expected >=1");
            Assert.True(result.Rows?.Count == result.TotalRows,"Total rows where not equal returned rows!");
            Assert.True(result.Rows.FirstOrDefault() != null,"Null object returned for first row");
            var firstDocument = result.Rows.FirstOrDefault();
            Assert.False(string.IsNullOrEmpty( firstDocument.ID),"Document id was null or empty");
            Assert.False(string.IsNullOrEmpty( firstDocument.Value.Revision),"Document rev was null or empty");
            //Get single docu, latest rev
            var singleDoc = await client.GetDocument<TestDocument>(firstDocument.ID);
            Assert.True(singleDoc.AllSet(),"Missing values in created document!");
            //Get with rev number
            var singleDocRev = await client.GetDocument<TestDocument>(singleDoc.ID,singleDoc.Rev);
            Assert.True(singleDoc.Equals(singleDocRev),"Documents are differnt but should be the same");
        }

        

        [Fact]
        public void DeleteDocument(){
            _CreateDocument().Wait();
            Deletedocument().Wait();
        }

        private async Task Deletedocument(){
            var client = GetTestClient();
            var prevRevision = LastDocument.Rev;
            var result = await client.DeleteDocument<TestDocument>(LastDocument);
            Assert.NotNull(result);
            Assert.True(result.Ok,"Delete document returned false");
            Assert.True(LastDocument.Deleted,"Delete flag is not true");
            Assert.True(prevRevision != LastDocument.Rev,"Revision was not updated");
        }
        [Fact]
        public void UpdateDocument(){
             _CreateDocument().Wait();
            _UpdateDocument().Wait();
        }

        private async Task _UpdateDocument(){
            var client = GetTestClient();
            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument.Rev;
            LastDocument.StringProp ="Luke, I'm your father!";
            var result = await client.UpdateDocument<TestDocument>(LastDocument);
            Assert.True(result.Ok,"Update document returned false");
            Assert.True(result.ID == LastDocument.ID,"ID of updated document where not the same!");
            Assert.True(result.Rev == LastDocument.Rev,"Revision number of result is not the same as for the test document");
            Assert.False(lastRev == LastDocument.Rev,"Revision number was not changed");
            var dbObj = await client.GetDocument<TestDocument>(LastDocument.ID,LastDocument.Rev);
            Assert.False(dbObj.StringProp == prevText,"document wasnt updated as expected!");
        }
        [Fact]
        public void UpdateDocuments(){
             _CreateDocument().Wait();
             _UpdateDocuments().Wait();
        }

         private async Task _UpdateDocuments(){
            var client = GetTestClient();

            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument.Rev;
            LastDocument.StringProp ="Luke, I'm your father!";
            
            var documentList = new List<TestDocument>(){ TestDocument.GenerateTestObject(), TestDocument.GenerateTestObject(), LastDocument};
            var result = await client.UpdateDocuments(documentList);
            Assert.True(result.Count() == documentList.Count,"Result list was not the same length as document list");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d.ID)),"Not all IDs where set as expected");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d.Rev)),"Not all revs where set as expected");
            Assert.True(result.All(r => r.Ok),"Update document returned false for at least one document");
            var changedDocResult = result.FirstOrDefault(r => r.ID == LastDocument.ID);
            Assert.NotNull(changedDocResult);
            Assert.True( changedDocResult.Rev == LastDocument.Rev,"Revision number missmatch");
            var dbObj = await client.GetDocument<TestDocument>(LastDocument.ID,LastDocument.Rev);
            Assert.False(dbObj.StringProp == prevText,"document wasnt updated as expected!");
        }


        [Fact]
        public void AddAttachment(){
            //Generate test file with random contens as txt
            _CreateDocument().Wait();            
            var file = RandomTextFile();
            Assert.True( File.Exists(file));
            _AddAttachment(file).Wait();
        }
        private async Task _AddAttachment(string File){
            var client = GetTestClient();
            var result =  await client.AddAttachment(LastDocument,File,"test.txt");
            Assert.True(result.Ok,"Add attachment failed");
            Assert.True(result.Rev == LastDocument.Rev,"Revision was not updated");
        }
        private string RandomTextFile(){
            var testFileName = "test.txt";
            if(File.Exists(testFileName))
            {
                File.Delete(testFileName);
            }
            Random random = new Random();
            File.WriteAllText(testFileName,new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", random.Next(100,1337))
                            .Select(s => s[random.Next(s.Length)]).ToArray()));
            return testFileName;

        }
        [Fact]
        public void GetAttachment()
        {
             _CreateDocument().Wait();            
            var file = RandomTextFile();
            Assert.True( File.Exists(file));
            _AddAttachment(file).Wait();
            _GetAttachment(file).Wait();
        }

        private async Task _GetAttachment(string FilePath)
        {
            var expectedSize = File.ReadAllBytes(FilePath);
            var client = GetTestClient();
            var result =  await client.GetAttachement(LastDocument,"test.txt");
            Assert.True(expectedSize.Count() == result.Count(),"File size is different");
        }

        [Fact]
        public void DeleteAttachement()
        {
            try  
            {
                 _CreateDocument().Wait();            
            }
            catch(Exception ex){
                throw new Exception($"Error in _CreateDocument with {ex.ToString()}",ex);
            }
            string file =null;
            try
            {
                file = RandomTextFile();
                Assert.True( File.Exists(file));
            }
            catch(Exception ex){
                throw new Exception($"Error in RandomTextFile with {ex.ToString()}",ex);
            }
            try{
                _AddAttachment(file).Wait();
            }
            catch(Exception ex){
                throw new Exception($"Error in _AddAttachment with {ex.ToString()}",ex);
            }
            try{
                _DeleteAttachment().Wait();
            }
            catch(Exception ex){
                throw new Exception($"Error in _DeleteAttachment with {ex.ToString()}",ex);
            }            
        }

        private async Task _DeleteAttachment()
        {
             var client = GetTestClient();
            var result = await client.DeleteAttachment(LastDocument,"test.txt");
             Assert.True(result.Ok,"Returned result was not true");
             Assert.True(result.Rev == LastDocument.Rev,"Revision was not updated as expected");
        }

        public void Dispose()
        {
            GetTestClient().DeleteDatbase(this.TestDB).Wait();
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

        private async Task CheckBasicProperties()
        {
            var client = GetTestClient();
            var documentFromDB = await client.GetDocument<TestDocument>(LastDocument.ID);
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