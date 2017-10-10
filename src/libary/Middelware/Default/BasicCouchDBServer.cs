using System;
using PillowSharp.BaseObject;

namespace PillowSharp.Middelware.Default
{
    public class BasicCouchDBServer : ICouchdbServer
    {

        private ELoginTypes _loginType = ELoginTypes.None;
        private CouchLoginData _loginData;

        private string _serverURL = null;

        public ELoginTypes LoginType { get { return _loginType; } set { _loginType = value; } }

        public BasicCouchDBServer(string ServerURL,CouchLoginData LoginData = null,ELoginTypes LoginType = ELoginTypes.None)
        {
            _loginType = LoginType;
            _loginData = LoginData;
            _serverURL = ServerURL;
        }

        public CouchLoginData GetLoginData()
        {
            return _loginData;
        }

        public string GetServerURL()
        {
            return _serverURL;
        }
    }
}