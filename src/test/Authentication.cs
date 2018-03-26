using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middelware.Default;
using System.Threading.Tasks;

namespace test
{
    public class AuthenticationDBCreationTests : BaseTest,IDisposable
    {
        public AuthenticationDBCreationTests() : base("authentication_test")
        {
        }

        [Theory]
        [InlineData(ELoginTypes.BasicAuth)]
        [InlineData(ELoginTypes.TokenLogin)]
        public void CreateDBBothAuth(ELoginTypes Type)
        {
           
            if (!CouchSettings.SkipAuthTests)
                LoginCreateDB(Type).Wait();
        }

        public void Dispose()
        {
            if(!CouchSettings.SkipAuthTests)
            {
                var client = GetTestClient();
                client.DeleteDatbase(this.TestDB).Wait();
            }
        }

        private async Task LoginCreateDB(ELoginTypes Type){
            var client = GetTestClient(Type);
            var result = await client.CreateNewDatabase(this.TestDB);
            Assert.True(result,$"Unable to create the db, with {Type.ToString("G")}");
        }   

        
    }
}
