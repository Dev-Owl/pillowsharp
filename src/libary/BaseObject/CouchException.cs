using System;
using PillowSharp.CouchType;

namespace PillowSharp.BaseObject
{

    public class CouchException : Exception
    {
        public CouchError Error { get; set; }

        public CouchException(CouchError Error) : base(message:Error?.ToString())
        {
            this.Error = Error;
            if (Error != null)
                Console.WriteLine(this.ToString());
            
        }

        public override string ToString(){
            if(Error == null)
                return base.ToString();
            
            return $"Couch error {Error.Error}, reason {Error.Reason} with HTTP status {Error.HTTPCode}";
        }

    }

}