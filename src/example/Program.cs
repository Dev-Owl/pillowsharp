using System;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.Client;
using PillowSharp.Middleware.Default;
using System.Linq;
using PillowSharp.CouchType;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace test
{
    class Program
    {

        public static PillowClient client = null;
        public static Person person = null;
        static void Main(string[] args)
        {
            Work().Wait(); // run the test case
            Console.WriteLine("Example completed, press enter to exit");
            Console.ReadLine();
        }

        public static async Task Work()
        {
            Console.WriteLine("Hello World, its Pillow#!");
            client = new PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984", new CouchLoginData("admin", "admin"), ELoginTypes.BasicAuth));
            client.TraceCallback = (traceInfo) =>
            {
                Console.WriteLine("---Pillow trace---");
                Console.WriteLine($"Endpoint: {traceInfo.RequestUrl}");
                Console.WriteLine($"Metho: {traceInfo.RequestMethod}");
                Console.WriteLine($"Duration(ms): {traceInfo.RequestTimeInMs}");
                Console.WriteLine("------------------");
            };
            try
            {
                client.Trace = true;
                try
                {
                    client.DeleteDatbase("pillow");
                }
                catch
                {
                    //Clean up prev test run
                }

                await ListDB();
                await GetUUIDS();
                await CreateDB();
                await AddDocuments();
                await UploadFileToDocument();
                await GetFileFromDocument();
                await DeleteFileFromDocument();
                await GetDocuments();
                await QueryDb();
                await RunListOnView();
                await DeleteDocument();
                await RunForMultipleDbs();


            }
            catch (PillowException pEx)
            {
                Console.WriteLine($"Error in pillow#:{pEx.Message}");
            }
            catch (CouchException cEx)
            {
                Console.WriteLine($"Error in couchdb:{cEx.ToString()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Generic error: {ex.Message}");
            }

        }



        private class QueryPerson
        {
            public string LastName { get; set; }

            public string Role { get; set; }

            public new string ToString()
            {
                return $"Hello my name is {LastName} and Im working as {Role}";
            }
        }

        private static async Task RunForMultipleDbs()
        {
            Console.WriteLine("Get count for all docs in all dbs");
            await client.RunForAllDbsAsyn((db) =>
            {
                Console.WriteLine($"{db}:{client.GetAllDocuments(DatabaseToUse: db).TotalRows ?? 0}");
            });

        }

        private static async Task QueryDb()
        {
            Console.WriteLine("Running MangoQuery against the db");
            var query = new MangoQuery()
            {
                FilteringFields = new List<string>(){
                     "LastName","Role"
                 },
                Selector = new MangoSelector()
                {
                    Operations = new List<MangoSelectorOperator>(){
                         new MangoSelectorOperator("$or"){
                             OperatorValues = new List<MangoSelectorOperator>(){
                                 new MangoSelectorOperator("LastName"){
                                     SimpleOperatorValue = "Muehle"
                                 },
                                 new MangoSelectorOperator("LastName"){
                                     SimpleOperatorValue = "NotFound"
                                 }
                             }
                         }
                     }
                },
                IncludeExecutionStats = true
            };
            await RunQuery(query);
            Console.WriteLine("Generating index for above query");
            var indexResult = await client.CreateMangoIndexAsync(new MangoIndex()
            {
                DesignDocument = "index_collection",
                Name = "example_index_lastname",
                Index = new MangoIndexFields()
                {
                    Fields = new List<string>(){
                         "LastName"
                     }
                },
            }, "pillow");
            Console.WriteLine($"Index result: {indexResult.Result}");
            query.UseIndex = new List<string>(){
                 "index_collection"
             };
            Console.WriteLine($"Running query again");
            await RunQuery(query);
        }

        private static async Task RunListOnView()
        {
            //Add content here to show usage
            var design = new CouchDesignDocument();
            design.ID = "listtesting";
            design.Views.Add("listview", new CouchView()
            {
                Map = "function (doc) {emit(doc._id, doc);}"
            });
            design.Lists.Add("list", "function(head,req){start();var filtered=[];while(row=getRow()){filtered.push(row.value);}send(toJSON(filtered));}");
            await client.UpsertDesignDocumentAsync(design);
            var result = await client.GetListFromViewAsync<List<Person>>("listtesting", "list", "listview", null);

        }

        private static async Task RunQuery(MangoQuery query)
        {
            var queryResult = await client.RunMangoQueryAsync<QueryPerson>(query, "pillow");
            Console.WriteLine("Query results:");
            Console.WriteLine($"Total docs: {queryResult.Docs.Count}");
            Console.WriteLine($"Warning: {queryResult.Warning}");
            Console.WriteLine($"ExecutionTimeMs: {queryResult.ExecutionStats?.ExecutionTimeMs}");
            foreach (var item in queryResult.Docs)
            {
                Console.WriteLine(item.ToString());
            }
        }

        public static async Task ListDB()
        {
            Console.WriteLine("DB on server:");
            var dbs = await client.GetListOfAllDatabasesAsync();
            dbs.ForEach(db => Console.WriteLine($"Found DB {db}"));
        }

        public static async Task GetUUIDS()
        {
            Console.WriteLine("Lets get 10 UUIDS from couch server");
            var uuidResponse = await client.GetManyUUIDsAsync(AmountOfUUIDs: 10);
            foreach (var id in uuidResponse.UUIDS)
            {
                Console.WriteLine(id);
            }
            Console.WriteLine("Easy as cake");
        }
        public static async Task CreateDB()
        {
            Console.WriteLine("Creating DB pillow");
            if (!await client.DbExistsAsync("pillow"))
            {
                if (await client.CreateNewDatabaseAsync("pillow"))
                    Console.WriteLine("Database pillow created");
            }
            else
                Console.WriteLine("Database pillow exists already");

            client.ForcedDatabaseName = "pillow"; //Set the db for requests
        }

        public static async Task AddDocuments()
        {
            Console.WriteLine("Adding document to pillow");
            person = new Person() { Name = "Christian", LastName = "Muehle", Role = "Developer" };
            var result = await client.CreateANewDocumentAsync(person);
            if (result.Ok)
            {
                Console.WriteLine($"Document created with id:{result.ID} and Rev:{result.Rev}");
                person.CouchDocument = result.ToCouchDocument();
            }
        }
        private static async Task UploadFileToDocument()
        {
            Console.WriteLine("Uploading sleepy owl");
            var filePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "sleepy_owl.JPG");
            var result = await client.AddAttachmentAsync(person.CouchDocument, "fav.image", filePath);
            if (result.Ok)
            {
                Console.WriteLine("Attachment added");
                person.CouchDocument = result.ToCouchDocument();
            }
        }

        public static async Task GetFileFromDocument()
        {
            Console.WriteLine("Downloading sleepy owl");
            var result = await client.GetAttachementAsync(person.CouchDocument, "fav.image");
            Console.WriteLine($"Got the file back, file has {result.Count()} bytes");
        }

        public static async Task DeleteFileFromDocument()
        {
            var result = await client.DeleteAttachmentAsync(person.CouchDocument, "fav.image");
            if (result.Ok)
            {
                Console.WriteLine($"The file is now deleted");
            }
        }

        public static async Task GetDocuments()
        {
            Console.WriteLine("Get documents from pillow");
            var result = await client.GetDocumentAsync<Person>(person.CouchDocument.ID);
            Console.WriteLine(result.ToString());
        }

        public static async Task DeleteDocument()
        {
            Console.WriteLine("Deleting document from pillow");
            var result = await client.DeleteDocumentAsync(person.CouchDocument);
            if (result.Ok)
            {
                Console.WriteLine("Document deleted");
            }
        }

        public static void DeleteDatabase()
        {
            Console.WriteLine("Delete database pillow");
        }

        public class Person
        {
            public string Name { get; set; }

            public string LastName { get; set; }

            public string Role { get; set; }
            [JsonIgnore]
            public CouchDocument CouchDocument { get; set; }

            public new string ToString()
            {
                return $"Hello my name is {Name} {LastName} and Im working as {Role}";
            }
        }
    }
}

