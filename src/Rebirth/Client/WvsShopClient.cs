using Rebirth.Characters;
using Rebirth.Entities;
using Rebirth.Entities.PlayerData;
using Rebirth.Server.Center;
using Rebirth.Server.Shop;
using System.Net.Sockets;

namespace Rebirth.Client
{
    public class WvsShopClient : ClientBase
    {
        public WvsShop Server { get; }

        public Character Character => MasterManager.CharacterPool.Get(dwCharId);
        public int dwCharId { get; private set; }
        public Account Account { get; set; }
        public CashLocker CashLocker
            => Account.AccountData.Locker;

        public WvsShopClient(WvsShop login, Socket socket) : base(socket)
        {
            Server = login;
        }

        public void Load(int charId)
        {
            dwCharId = charId;

            Account = new Account(charId);
            Account.Init();

            if (MasterManager.CharacterPool.Get(charId) is null)
            {
                Log.Debug("pChar (shop) null, adding to character pool.");
                MasterManager.CharacterPool.Add(new Character(charId));
            }

            LoggedIn = true;
        }

        public void EnableActions()
            => SendPacket(CPacket.CCashShop.EnableShopActions(this));
    }
}
