using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PillowSharp.BaseObject;

public class PurgeRequest
{
    public List<PurgeRequestDocument> DocumentsToPurge { get; set; }
    
    public string WriteJosn(){
        if(DocumentsToPurge != null && DocumentsToPurge.Count > 0){
            if(DocumentsToPurge.TrueForAll(d => !string.IsNullOrEmpty( d.DocumentID))){
                  var currentObject = new JObject();
                  DocumentsToPurge.ForEach((doc) => {
                      var jArray = new JArray();
                      doc.DocumentRevisions?.ForEach((rev)=> {
                          jArray.Add(rev);
                      });
                      currentObject.Add(doc.DocumentID,jArray);
                  });
                  return currentObject.ToString();
            }
            else{
                throw new PillowException("The pruge document list has to contian the documents id, one ore more are missing");
            }

        }
        return "";
    }
}

public class PurgeRequestDocument
{
    public string DocumentID { get; set; }

    public List<string> DocumentRevisions { get; set; }

    public PurgeRequestDocument(string documentID, List<string> documentReviosions)
    {
        DocumentID = documentID;
        DocumentRevisions = documentReviosions;
    }
}