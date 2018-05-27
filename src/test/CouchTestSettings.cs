
using PillowSharp.BaseObject;
using PillowSharp.Client;
using PillowSharp.Middleware.Default;

public static class CouchSettings{

    public static bool SkipAuthTests { get; set; } = false;

    public static BasicCouchDBServer Server = new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData("admin","admin"),ELoginTypes.TokenLogin);

    public static PillowClient GetTestClient(string Database=null, ELoginTypes LoginType = ELoginTypes.BasicAuth) {
        Server.LoginType = LoginType;
        return new PillowClient(CouchSettings.Server) {ForcedDatabaseName = Database}; 
    }
    
}