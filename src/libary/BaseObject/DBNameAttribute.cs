using System;

namespace PillowSharp.BaseObject
{

    public class DBNameAttribute : Attribute
    {
         public string Name { get; set; }     
        

        public DBNameAttribute(string Name):base()
        {
            this.Name=Name;
        }

    }

}