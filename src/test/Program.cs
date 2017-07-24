using System;
using System.Threading.Tasks;
using PillowSharp.BaseObject;
using PillowSharp.Client;
using PillowSharp.Middelware.Default;
using System.Linq;
using PillowSharp.CouchType;
using Newtonsoft.Json;
using System.IO;

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

        public static async Task Work(){
            Console.WriteLine("Hello World, its Pillow#!");
            client = new PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData("admin","admin"),ELoginTypes.TokenLogin));
            try
            {
                await ListDB();
                await GetUUIDS();
                await CreateDB();
                await AddDocuments();
                await UploadFileToDocument();
                await GetFileFromDocument();
                await DeleteFileFromDocument();
                await GetDocuments();
                await DeleteDocument();
            }
            catch(PillowException pEx){
                Console.WriteLine($"Error in pillow#:{pEx.Message}");
            }
            catch(CouchException cEx){
                Console.WriteLine($"Error in couchdb:{cEx.ToString()}");
            }
            catch(Exception ex){
                Console.WriteLine($"Generic error: {ex.Message}");
            }
            
        }
        public static async Task ListDB(){
            Console.WriteLine("DB on server:");
            var dbs = await client.AllDatabase();
            dbs.ForEach(db => Console.WriteLine($"Found DB {db}"));
        }

        public static async Task GetUUIDS(){
            Console.WriteLine("Lets get 10 UUIDS from couch server");
            var uuidResponse = await client.GetUUID(Count:10);
            foreach(var id in uuidResponse.uuids){
                Console.WriteLine(id);
            }
            Console.WriteLine("Easy as cake");
        }
        public static async Task CreateDB()
        {
            Console.WriteLine("Creating DB pillow");
            if(!await client.DbExists("pillow"))
            {
                if( await client.CreateDatabase("pillow"))
                    Console.WriteLine("Database pillow created");
            }
            else
                Console.WriteLine("Database pillow exists already");
            
            client.Database = "pillow"; //Set the db for requests
        }

        public static async Task AddDocuments(){
            Console.WriteLine("Adding document to pillow");
            person = new Person(){Name="Christian",LastName="Muehle",Role="Developer" };
            var result = await client.CreateDocument(person);
            if(result.ok)
            {
                Console.WriteLine($"Document created with id:{result.id} and rev:{result.rev}");
                person.CouchDocument = result.ToCouchDocument();
            }
        }
        private static async Task UploadFileToDocument()
        {
            Console.WriteLine("Uploading sleepy owl");
            var result =  await client.AddAttachment(person.CouchDocument,"fav.image","sleepy_owl.JPG");
            if(result.ok){
                Console.WriteLine("Attachment added");
                person.CouchDocument = result.ToCouchDocument();
            }
        }

        public static async Task GetFileFromDocument()
        {
             Console.WriteLine("Downloading sleepy owl");
            var result =  await client.GetAttachement(person.CouchDocument,"fav.image");
            Console.WriteLine($"Got the file back, file has {result.Count()} bytes");
        }

        public static async Task DeleteFileFromDocument()
        {
            var result =  await client.DeleteAttachment(person.CouchDocument,"fav.image");
            if(result.ok){
                 Console.WriteLine($"The file is now deleted");
            }
        }

        public static async Task GetDocuments(){
            Console.WriteLine("Get documents from pillow");
            var result = await client.GetDocument<Person>(person.CouchDocument._id);
            Console.WriteLine(result.ToString());
        }
        
        public static async Task DeleteDocument(){
            Console.WriteLine("Deleting document from pillow");
            var result = await client.DeleteDocument(person.CouchDocument);
            if(result.ok){
                Console.WriteLine("Document deleted");
            }
        }

        public static void DeleteDatabase(){
            Console.WriteLine("Delete database pillow");
        }

        public class Person{
            public string Name { get; set; }

            public string LastName { get; set; }

            public string Role { get; set; }
            [JsonIgnore]
            public CouchDocument CouchDocument { get; set; }

            public new string ToString(){
                return $"Hello my name is {Name} {LastName} and Im working as {Role}";
            }
        }
    }
}

