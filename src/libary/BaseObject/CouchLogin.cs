using System;
using Newtonsoft.Json;
using Newtonsoft;
namespace PillowSharp.BaseObject
{

    public class CouchLoginData
    {
        [JsonProperty("name")]
        public string UserName { get; set; }
        [JsonProperty("password")]     
        public string Password { get; set; }

        public CouchLoginData(string User,string Password)
        {
            UserName=User;
            this.Password=Password;
        }

    }

}