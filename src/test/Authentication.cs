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
            if(!CouchSettings.SkipAuthTests)
                LoginCreateDB(Type).Wait();
        }

        public void Dispose()
        {
            if(!CouchSettings.SkipAuthTests)
            {
                var client = CouchSettings.GetTestClient();
                client.DeleteDatbase(DBName).Wait();
            }
        }

        private async Task LoginCreateDB(ELoginTypes Type){
            var client =  CouchSettings.GetTestClient(LoginType: Type);
            var result = await client.CreateDatabase(DBName);
            Assert.True(result,$"Unable to create the db, with {Type.ToString("G")}");
        }   

        
    }
}
