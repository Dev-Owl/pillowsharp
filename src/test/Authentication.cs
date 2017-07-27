using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middelware.Default;
using System.Threading.Tasks;

namespace test
{
    public class Authentication : IDisposable
    {
        PillowClient client = null;

        public Authentication()
        {
            client = new  PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData("",""),ELoginTypes.BasicAuth));
        }
      

        [Theory]
        [InlineData("admin","admin")]
        public void CreateDB(string User,string Password)
        {
            LoginCreateDB(User,Password).Wait();
        }

        private async Task LoginCreateDB(string User,string Password){
            client.ServerHelper = new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData(User,Password),ELoginTypes.BasicAuth); 
            var result = await client.CreateDatabase("pillowtest");
            Assert.True(result,"Unable to create the db");
        }   

        void IDisposable.Dispose()
        {
           client.ServerHelper = new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData("admin","admin"),ELoginTypes.BasicAuth); 
           if(client.DbExists("pillowtest").Result)
            client.DeleteDatbase("pillowtest").Wait();
           client = null;
        }
        
    }
}
