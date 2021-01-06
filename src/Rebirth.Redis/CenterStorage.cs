using log4net;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace Rebirth.Redis
{
    //Thanks WvsBeta <3
    public class CenterStorage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CenterStorage));

        private static readonly TimeSpan MigrateExpiry = TimeSpan.FromSeconds(20);
        private readonly IConnectionMultiplexer m_connectionMultiplexer;
        private readonly IDatabase m_db;

        public CenterStorage(IConfiguration config)
        {
            var redisConfig = config.GetValue<string>("redis");

            m_connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig); // if an error is thrown here it means that we are unable to connect to redis
            m_db = m_connectionMultiplexer.GetDatabase();
        }

        /**
         * Used to get the ISubscriber
         */
        public IConnectionMultiplexer Multiplexer()
        {
            return m_connectionMultiplexer;
        }

        public bool IsAccountOnline(int id)
        {
            var key = CenterKeys.GetAccountKey(id);
            return m_db.KeyExists(key);
        }

        public void AddAccountOnline(int id)
        {
            var key = CenterKeys.GetAccountKey(id);

            if (IsAccountOnline(id))
                return;

            m_db.StringSet(
                    key,
                    "",
                    when: When.Always,
                    flags: CommandFlags.FireAndForget
                );
        }

        public void RemoveAccountOnline(int id)
        {
            var key = CenterKeys.GetAccountKey(id);
            m_db.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public bool IsCharacterOnline(int id)
        {
            var key = CenterKeys.GetCharacterKey(id);
            return m_db.KeyExists(key);
        }

        public void AddCharacterOnline(int id)
        {
            if (IsCharacterOnline(id))
                return;

            var key = CenterKeys.GetCharacterKey(id);
            m_db.StringSet(
                    key,
                    "",
                    when: When.Always,
                    flags: CommandFlags.FireAndForget
                );
        }

        public void RemoveCharacterOnline(int id)
        {
            var key = CenterKeys.GetCharacterKey(id);
            m_db.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public bool IsCharacterMigrate(int id)
        {
            var key = CenterKeys.GetMigrateKey(id);
            return m_db.KeyExists(key);
        }

        public void AddCharacterMigrate(int id)
        {
            if (IsCharacterMigrate(id))
                return;

            var key = CenterKeys.GetMigrateKey(id);
            m_db.StringSet(
                    key,
                    "",
                    expiry: MigrateExpiry,
                    when: When.Always,
                    flags: CommandFlags.FireAndForget
                );
        }

        public void RemoveCharacterMigrate(int id)
        {
            var key = CenterKeys.GetMigrateKey(id);
            m_db.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public void AddCharacterCSITC(int id)
        {
            if (IsCharacterCSITC(id))
                return;

            var key = CenterKeys.GetCSITCKey(id);
            m_db.StringSet(
                    key,
                    "",
                    expiry: MigrateExpiry,
                    when: When.Always,
                    flags: CommandFlags.FireAndForget
                );
        }

        public void RemoveCharacterCSITC(int id)
        {
            var key = CenterKeys.GetCSITCKey(id);
            m_db.KeyDelete(key, CommandFlags.FireAndForget);
        }

        public bool IsCharacterCSITC(int id)
        {
            var key = CenterKeys.GetCSITCKey(id);
            return m_db.KeyExists(key);
        }
    }
}
