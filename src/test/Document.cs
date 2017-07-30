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
    public class DocumentTests : IDisposable
    {
        public const string DBName = "pillowtest_doc";
        public TestDocument LastDocument { get; set; }

        public DocumentTests()
        {
            CouchSettings.GetTestClient().CreateDatabase(DBName).Wait();
        }
        [Fact]
        public void CreateDocument()
        {
          _CreateDocument().Wait();
        }

        private async Task _CreateDocument(){
            
            var testDoc = new TestDocument();
            PillowClient client = CouchSettings.GetTestClient();
            var result = await client.CreateDocument(testDoc); 
            //Ensure all needed parts are set by the client
            Assert.True(result.ok,"Error during document creation");
            Assert.False(string.IsNullOrEmpty( testDoc._id ),"ID was not set for new document");
            Assert.False(string.IsNullOrEmpty( testDoc._rev ),"Rev was not set for new document");
            LastDocument = testDoc;
        }
        [Fact]
        public void GetDocument(){
            _CreateDocument().Wait(); // ensure document exists
            _GetDocument().Wait();
        }

        private async Task _GetDocument()
        {
            //get all
            PillowClient client = CouchSettings.GetTestClient();
            var result = await client.GetAllDocuments(Database:DBName);
            Assert.True(result !=null,"No result from couch db");
            Assert.True(result.total_rows > 0,"No documents returned, expected >=1");
            Assert.True(result.rows?.Count == result.total_rows,"Total rows where not equal returned rows!");
            Assert.True(result.rows.FirstOrDefault() != null,"Null object returned for first row");
            var firstDocument = result.rows.FirstOrDefault();
            Assert.False(string.IsNullOrEmpty( firstDocument.id),"Document id was null or empty");
            Assert.False(string.IsNullOrEmpty( firstDocument.value.Revision),"Document rev was null or empty");
            //Get single docu, latest rev
            var singleDoc = await client.GetDocument<TestDocument>(firstDocument.id);
            Assert.True(singleDoc.AllSet(),"Missing values in created document!");
            //Get with rev number
            var singleDocRev = await client.GetDocument<TestDocument>(singleDoc._id,singleDoc._rev);
            Assert.True(singleDoc.Equals(singleDocRev),"Documents are differnt but should be the same");
        }

        

        [Fact]
        public void DeleteDocument(){
            _CreateDocument().Wait();
            _DeleteDocument().Wait();
        }

        private async Task _DeleteDocument(){
            var client = CouchSettings.GetTestClient();
            var prevRevision = LastDocument._rev;
            var result = await client.DeleteDocument<TestDocument>(LastDocument);
            Assert.NotNull(result);
            Assert.True(result.ok,"Delete document returned false");
            Assert.True(LastDocument._deleted,"Delete flag is not true");
            Assert.True(prevRevision != LastDocument._rev,"Revision was not updated");
        }
        [Fact]
        public void UpdateDocument(){
             _CreateDocument().Wait();
            _UpdateDocument().Wait();
        }

        private async Task _UpdateDocument(){
            var client = CouchSettings.GetTestClient();
            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument._rev;
            LastDocument.StringProp ="Luke, I'm your father!";
            var result = await client.UpdateDocument<TestDocument>(LastDocument);
            Assert.True(result.ok,"Update document returned false");
            Assert.True(result.id == LastDocument._id,"ID of updated document where not the same!");
            Assert.True(result.rev == LastDocument._rev,"Revision number of result is not the same as for the test document");
            Assert.False(lastRev == LastDocument._rev,"Revision number was not changed");
            var dbObj = await client.GetDocument<TestDocument>(LastDocument._id,LastDocument._rev);
            Assert.False(dbObj.StringProp == prevText,"document wasnt updated as expected!");
        }
        [Fact]
        public void UpdateDocuments(){
             _CreateDocument().Wait();
             _UpdateDocuments().Wait();
        }

         private async Task _UpdateDocuments(){
            var client = CouchSettings.GetTestClient();
            
            var prevText = LastDocument.StringProp;
            var lastRev = LastDocument._rev;
            LastDocument.StringProp ="Luke, I'm your father!";
            
            var documentList = new List<TestDocument>(){new TestDocument(),new TestDocument(),LastDocument};
            var result = await client.UpdateDocument(documentList);
            Assert.True(result.Count == documentList.Count,"Result list was not the same length as document list");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d._id)),"Not all IDs where set as expected");
            Assert.True(documentList.All(d => !string.IsNullOrEmpty(d._rev)),"Not all revs where set as expected");
            Assert.True(result.All(r => r.ok),"Update document returned false for at least one document");
            var changedDocResult = result.FirstOrDefault(r => r.id == LastDocument._id);
            Assert.NotNull(changedDocResult);
            Assert.True( changedDocResult.rev == LastDocument._rev,"Revision number missmatch");
            var dbObj = await client.GetDocument<TestDocument>(LastDocument._id,LastDocument._rev);
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
            var client = CouchSettings.GetTestClient();
            var result =  await client.AddAttachment(LastDocument,File,"test.txt");
            Assert.True(result.ok,"Add attachment failed");
            Assert.True(result.rev == LastDocument._rev,"Revision was not updated");
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
            var client = CouchSettings.GetTestClient();
            var result =  await client.GetAttachement(LastDocument,"test.txt");
            Assert.True(expectedSize.Count() == result.Count(),"File size is different");
        }

        [Fact]
        public void DeleteAttachement(){
            _CreateDocument().Wait();            
            var file = RandomTextFile();
            Assert.True( File.Exists(file));
            _AddAttachment(file).Wait();
            _DeleteAttachment().Wait();
        }

        private async Task _DeleteAttachment()
        {
             var client = CouchSettings.GetTestClient();
             var result = await client.DeleteAttachment(LastDocument,"test.txt");
             Assert.True(result.ok,"Returned result was not true");
             Assert.True(result.rev == LastDocument._rev,"Revision was not updated as expected");
        }

        public void Dispose()
        {
            CouchSettings.GetTestClient().DeleteDatbase(DBName).Wait();
        }

        [DBName(DocumentTests.DBName)]
        public class TestDocument : CouchDocument{
            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public List<string> ListProp { get; set; }

            public TestDocumentSmall SubObj { get; set; }

            public List<TestDocumentSmall> SubObjList { get; set; }

            public TestDocument()
            {   
                IntProp = new Random().Next(1,23456789);
                StringProp ="This are not the droids you are looking for";
                ListProp = new  List<string>(){"a","b","1337"};
                SubObj = new TestDocumentSmall();
                SubObjList = new List<TestDocumentSmall>() {new TestDocumentSmall(),new TestDocumentSmall()};
            }

            public bool AllSet()
            {
                return IntProp > 0 && !string.IsNullOrEmpty(StringProp) && ListProp.Count > 0 && SubObj != null && SubObjList.Count >0 && !SubObjList.Any(o => o == null);

            }

            public override bool Equals(object obj){
                if(obj is TestDocument){
                    var doc = obj as TestDocument;
                     return  doc._id == this._id && doc._rev == doc._rev;
                }
                return false;
            }
        }

        public class TestDocumentSmall{
            public string TestProp { get; set; }

            public TestDocumentSmall()
            {
                TestProp ="FuBar";
            }
        }
    }
}