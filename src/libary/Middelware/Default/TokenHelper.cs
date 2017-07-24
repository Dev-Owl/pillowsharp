using System;
using System.Collections.Generic;
using PillowSharp.BaseObject;

namespace PillowSharp.Middelware.Default
{
    public class TokenStorage{

        public int LifeTime { get; set; }

        private Dictionary<string,LifeTimeToken> storage = new Dictionary<string, LifeTimeToken>();

        public TokenStorage(){
            LifeTime = 570;//Default config sets lifetime to 10 minutes 9:30 in seconds
        }

        public void Add(string User,string Token ,int? LifeTime=null){
            var _lifeTime = this.LifeTime;
            if(LifeTime.HasValue)
                _lifeTime = LifeTime.GetValueOrDefault();
            
            if(storage.ContainsKey(User)){
                var lifeTime = storage[User];
                lifeTime.LifeTime = DateTime.UtcNow.AddSeconds(_lifeTime);
                lifeTime.Token = Token;
            }
            else{
                storage.Add(User, new LifeTimeToken(){LifeTime=DateTime.UtcNow.AddSeconds(_lifeTime),Token = Token});
            }
        }

        public string Get(string User){
             var token ="";
             if(storage.ContainsKey(User))
             {
                 var lifeTime = storage[User];
                 if(lifeTime.LifeTime > DateTime.UtcNow)
                    token = lifeTime.Token;
                 else
                 {
                     storage.Remove(User);
                 }
             }
             return token;
        }

        private class LifeTimeToken{
            public DateTime LifeTime;
            public string Token;
        }

    }
}