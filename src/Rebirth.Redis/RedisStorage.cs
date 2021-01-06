using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace Rebirth.Redis
{
    public class RedisStorage
    {
        private IConnectionMultiplexer _connection;

        public IDatabase Database => _connection.GetDatabase();

        public RedisStorage(IConfiguration config)
        {
            var dbcfg = config["redis"];

            _connection = ConnectionMultiplexer.Connect(dbcfg);
        }
               
        //-------------------------------------------

        public bool Exists(string key)
        {
            return Database.KeyExists(key);
        }

        public bool Set(string key, string value, TimeSpan? expiry = null)
        {
            return Database.StringSet(key, value, expiry, When.Always);
        }

        public string Get(string key)
        {
            return Database.StringGet(key);
        }

        public bool Delete(string key)
        {
            return Database.KeyDelete(key);
        }

        public TimeSpan? TimeToLive(string key)
        {
            return Database.KeyTimeToLive(key);
        }

        //-------------------------------------------

        //public async Task InitializeAsync()
        //{
        //    var dbcfg = _config["redis"];
        //    _connection = await ConnectionMultiplexer.ConnectAsync(dbcfg);
        //}

        //public Task<bool> Exists(RedisKey key)
        //{
        //    return Database.KeyExistsAsync(key);
        //}

        //public Task<bool> Set(RedisKey key, string value, TimeSpan? expiry = null)
        //{
        //    return Database.StringSetAsync(key, value, expiry, When.Always);
        //}

        //public async Task<string> Get(RedisKey key)
        //{
        //    return await Database.StringGetAsync(key);
        //}

        //public Task<bool> Delete(RedisKey key)
        //{
        //    return Database.KeyDeleteAsync(key);
        //}

        //public Task<TimeSpan?> TimeToLive(RedisKey key)
        //{
        //    return Database.KeyTimeToLiveAsync(key);
        //}
    }
}
