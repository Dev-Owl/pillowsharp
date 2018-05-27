using System;
using pillowsharp.Middleware;
using PillowSharp.BaseObject;

namespace PillowSharp.Middleware
{
    public interface IJSONHelper
    {
        string ToJSON(object Data);
        T FromJSON<T>(string JSON)where T:new();
        T FromJSON<T>(RestResponse Response)where T:new();
    }
}