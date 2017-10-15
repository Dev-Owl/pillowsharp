﻿using PillowSharp.BaseObject;
using PillowSharp.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace test
{
    public abstract class BaseTest
    {
        public string TestDB { get; set; }
        public Random Rnd { get; set; }


        public BaseTest(string BaseDBName)
        {
            Rnd = new Random();
            TestDB = BaseDBName + Rnd.Next(Int16.MaxValue);
        }

        public PillowClient GetTestClient(ELoginTypes LoginType = ELoginTypes.TokenLogin)
        {
            return CouchSettings.GetTestClient(TestDB, LoginType);
        }
    }
}
