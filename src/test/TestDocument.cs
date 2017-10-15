using System;
using Xunit;
using PillowSharp;
using PillowSharp.Client;
using PillowSharp.BaseObject;
using PillowSharp.Middelware.Default;
using System.Threading.Tasks;
using PillowSharp.CouchType;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace test
{
        public class TestDocument : CouchDocument{
            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public List<string> ListProp { get; set; }

            public TestDocumentSmall SubObj { get; set; }

            public List<TestDocumentSmall> SubObjList { get; set; }

            public TestDocument()
            {   
                IntProp = new Random().Next(1,23456789);
                StringProp ="This are not the droids you are loOking for";
                ListProp = new  List<string>(){"a","b","1337"};
                SubObj = new TestDocumentSmall();
                SubObjList = new List<TestDocumentSmall>() {new TestDocumentSmall(),new TestDocumentSmall()};
            }

            public bool AllSet()
            {
                return IntProp > 0 && !string.IsNullOrEmpty(StringProp) && ListProp.Count > 0 && SubObj != null && SubObjList.Count >0 && !SubObjList.Any(o => o == null);

            }

            public override bool Equals(object obj){
                if(obj is TestDocument){
                    var doc = obj as TestDocument;
                     return  doc.ID == this.ID && doc.Rev == doc.Rev;
                }
                return false;
            }
            //Override to remove warning
            public override int GetHashCode(){
                return 0;
            }
        }

        public class TestDocumentSmall{
            public string TestProp { get; set; }

            public TestDocumentSmall()
            {
                TestProp ="FuBar";
            }
        }
}