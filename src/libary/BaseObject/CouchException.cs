using System;
using PillowSharp.CouchType;

namespace PillowSharp.BaseObject
{

    public class CouchException : Exception
    {
        public CouchError Error { get; set; }

        public CouchException(CouchError Error)
        {
            this.Error = Error;
        }

        public override string ToString(){
            if(Error == null)
                return base.ToString();
            
            return $"Couch error {Error.error}, reason {Error.reason} with HTTP status {Error.HTTPCode}";
        }

    }

}