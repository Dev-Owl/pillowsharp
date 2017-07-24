using System;
using PillowSharp.BaseObject;

namespace PillowSharp.Middelware
{
    public interface ICouchdbServer{
        string GetServerURL();
        ELoginTypes LoginType {get;}

        CouchLoginData GetLoginData();
    }
}