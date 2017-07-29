using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middelware.Default;
using System.Threading.Tasks;

namespace test
{
    public class AuthenticationDBCreationTests : IDisposable
    {
        public const string DBName = "pillowtest_auth";


        [Theory]
        [InlineData(ELoginTypes.BasicAuth)]
        [InlineData(ELoginTypes.TokenLogin)]
        public void CreateDBBothAuth(ELoginTypes Type)
        {
            LoginCreateDB("admin","admin",Type).Wait();
        }

        public void Dispose()
        {
            var client = CouchSettings.GetTestClient();
            client.DeleteDatbase(DBName).Wait();
        }

        private async Task LoginCreateDB(string User,string Password,ELoginTypes Type){
            var client = new PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData(User,Password),Type));
            var result = await client.CreateDatabase(DBName);
            Assert.True(result,$"Unable to create the db, with {Type.ToString("G")}");
        }   

        
    }
}
