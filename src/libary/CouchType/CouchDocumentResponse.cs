using System;
using System.Collections.Generic;
using PillowSharp.Middelware;

namespace PillowSharp.CouchType
{
    public class CouchDocumentResponse<T> where T : new()
    {
        public int? total_rows { get; set; }

        public int? offset { get; set; }

        public int? update_seq  { get; set; }

        public List<T> rows { get; set; }             
    }
}