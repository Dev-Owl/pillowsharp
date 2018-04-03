using System;
using pillowsharp.Middelware;
using PillowSharp.BaseObject;

namespace PillowSharp.Middelware
{
    public interface IJSONHelper{
        string ToJSON(object Data);
        T FromJSON<T>(string JSON)where T:new();
        T FromJSON<T>(RestResponse Response)where T:new();
    }
}