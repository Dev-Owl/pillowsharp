using System;
using System.Collections.Generic;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchDocumentChange : CouchConfirm
    {
          public string id { get; set; }                   

          public string rev {get; set;}

          public CouchDocument ToCouchDocument(){
              return new CouchDocument(){_id = this.id,_rev = this.rev,_deleted=false};
          }
    }
}