using System;
using PillowSharp.BaseObject;
using RestSharp;

namespace PillowSharp.Middelware
{
    public interface IJSONHelper{
        string ToJSON(object Data);
        T FromJSON<T>(string JSON)where T:new();
        T FromJSON<T>(IRestResponse Response)where T:new();
    }
}