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
    public class DesignDocumentTests : BaseTest, IDisposable
    {
       
        public CouchDesignDocument LastDesignDoc {get;set;}

        public DesignDocumentTests():base("pillowtest_designdoc")
        {
            GetTestClient().CreateNewDatabase(this.TestDB).Wait();
        }

        [Fact]
        public void CreateDesign()
        {
            _CreateDesign().Wait();
        }

        private async Task _CreateDesign()
        {
            var client = GetTestClient();
            //Create document with two views
            var designDoc = new CouchDesignDocument();
            designDoc.ID="testDesignDoc";
            designDoc.AddView("test","function (doc) {emit(doc._id, 1);}");
            designDoc.AddView("testReduce","function (doc) {emit(doc._id, 1);}","_count");
            var result = await client.UpsertDesignDocument(designDoc);
            Assert.True(result.Ok);
            Assert.True(!string.IsNullOrEmpty(designDoc.Rev),"Revision was not set during creation");
            Assert.True(designDoc.ID.StartsWith(CouchEntryPoints.DesignDoc),"New created design document doesnt start with _design");
            Assert.False(designDoc.Deleted,"Design document is marked as deleted!");
            LastDesignDoc = designDoc;
        }
        [Fact]
        public void GetDesign()
        {
             _GetDesign().Wait();
        }
        private async Task _GetDesign()
        {
            await _CreateDesign();
            Assert.True(LastDesignDoc != null,"No document created during CreateDesign");
            var client = GetTestClient();
            var dbDocument = await client.GetDesignDocument(LastDesignDoc.ID);
            Assert.True(dbDocument != null,"Unable to get design from DB");
            Assert.True(dbDocument.ID == LastDesignDoc.ID,"ID is different as expected!");
            Assert.True(dbDocument.Rev == LastDesignDoc.Rev,"Revision is different as expected");
            Assert.True(dbDocument.Filters != null,"Filters is null");
            Assert.True(dbDocument.Lists != null,"Lists is null");
            Assert.True(dbDocument.Views != null,"Views is null");
            Assert.True(dbDocument.Shows != null,"Shows is null");
            Assert.True(dbDocument.Updates != null,"Updates is null");
            Assert.True(dbDocument.Views.Count == LastDesignDoc.Views.Count,"View count is different as expected");
        }

        public void DeleteDesign()
        {

        }
        [Fact]
        public void RunViewDesign()
        {
           _RunViewDesign().Wait();
        }
        private async Task _RunViewDesign(){
            await _CreateDesign();
            var testDoc = await DocumentTests.CreateTestDocument(this.TestDB);//create a doc in design test db            
            var client = GetTestClient();
            var result = await client.GetView<dynamic>(LastDesignDoc.ID,"test",new[] {new KeyValuePair<string, object>("reduce","false")});
            Assert.True(result != null,"Result was null, expected data");
            //TODO extend this test
        }

        public void FilterViewDesign()
        {

        }


        public void Dispose()
        {
            GetTestClient().DeleteDatbase(this.TestDB).Wait();
        }
    }
}