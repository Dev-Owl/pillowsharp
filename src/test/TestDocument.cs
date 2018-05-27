using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middleware.Default;
using System.Threading.Tasks;
using PillowSharp.CouchType;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
namespace test
{
    public class TestDocument : CouchDocument
    {
        public int IntProp { get; set; }
        public string StringProp { get; set; }

        public List<string> ListProp { get; set; }

        public TestDocumentSmall SubObj { get; set; }

        public List<TestDocumentSmall> SubObjList { get; set; }

        [JsonProperty()]
        private Dictionary<string, string> DicTest { get; set; }

        public static TestDocument GenerateTestObject()
        {
            return new TestDocument()
            {
                IntProp = new Random().Next(1, 23456789),
                 StringProp = "This are not the droids you are loOking for",
            ListProp = new List<string>() { "a", "b", "1337" },
            SubObj =  TestDocumentSmall.GenerateTestDocumentSmall(),
            SubObjList = new List<TestDocumentSmall>() {  TestDocumentSmall.GenerateTestDocumentSmall(), TestDocumentSmall.GenerateTestDocumentSmall() },
            DicTest = new Dictionary<string, string>() { {"a","b" },{"c","d" } }
            };
          
        }

        public bool AllSet()
        {
            return IntProp > 0
                   && !string.IsNullOrEmpty(StringProp)
                   && ListProp.Count > 0
                   && SubObj != null
                   && SubObjList.Count > 0
                   && !SubObjList.Any(o => o == null)
                   && DicTest != null
                   && DicTest.ContainsKey("a")
                   && DicTest.ContainsKey("c");

        }

        public override bool Equals(object obj)
        {
            if (obj is TestDocument)
            {
                var doc = obj as TestDocument;
                return doc.ID == this.ID && doc.Rev == this.Rev;
            }
            return false;
        }
        //Override to remove warning
        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class TestDocumentSmall
    {
        public string TestProp { get; set; }

        public TestDocumentSmall()
        {
        }

        public static TestDocumentSmall GenerateTestDocumentSmall()
        {
            return new TestDocumentSmall()
            {
                TestProp = "FuBar"
            };
        }
    }
}