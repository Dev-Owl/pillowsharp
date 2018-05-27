using System;
using PillowSharp.BaseObject;

namespace PillowSharp.Middleware
{
    public interface ICouchdbServer{
        string GetServerURL();
        ELoginTypes LoginType {get;}

        CouchLoginData GetLoginData();
    }
}