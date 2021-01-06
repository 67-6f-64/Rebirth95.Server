using System.Diagnostics;
using Rebirth.Scripts.Npc;
using Rebirth.Characters;
using System.Net.Sockets;
using Rebirth.Entities;
using Rebirth.Scripts.Quest;
using Rebirth.Scripts;
using Rebirth.Entities.Shop;
using Rebirth.Server.Center;
using Rebirth.Server.Game;

namespace Rebirth.Client
{
    public class WvsGameClient : ClientBase
    {       
        public int dwCharId { get; private set; }

        public WvsGame Server { get; }
        public Account Account { get; private set; }
        public Character Character => MasterManager.CharacterPool.Get(dwCharId);
        
        public bool MigratedIn { get; set; }
        
        public NpcScript NpcScript { get; private set; }
        public QuestScript QuestScript { get; private set; }
        public CShop ActiveShop { get; set; }

        public WvsGameClient(WvsGame game, Socket socket) : base(socket)
        {
            Server = game;
            MigratedIn = false;
            Account = null;
            NpcScript = null;
        }

        public void Load(int nCharId)
        {
	        var timer = new Stopwatch();
	        timer.Start();
            dwCharId = nCharId;

            Account = new Account(nCharId);
            Account.Init();

            var pCharacter = MasterManager.CharacterPool.Get(nCharId);

            if (pCharacter is null)
            {
                pCharacter = new Character(nCharId);
                pCharacter.Init(this);

                MasterManager.CharacterPool.Add(pCharacter);
            }
            else
            {
                pCharacter.Init(this);
            }

            LoggedIn = true;
            timer.Stop();
            Log.Debug("Time to load account and character in WvsGameClient: " + timer.ElapsedMilliseconds);
        }


        public void ChangeScript(ScriptBase pScript)
        {
            if (pScript is NpcScript npc)
            {
                NpcScript?.Dispose();
                QuestScript?.Dispose();
                NpcScript = npc;
            }
            else if (pScript is QuestScript quest)
            {
                QuestScript?.Dispose();
                QuestScript = quest;
            }
            else
            {
                throw new ScriptException("Trying to pass a ScriptBase object that isn't handled.");
            }
        }

        public void ClearScripts(bool bBoth)
        {
            QuestScript = null;

            if (bBoth)
                NpcScript = null;
        }

		//public override void Dispose()
		//{
		//	base.Dispose();
		//	Character.Dispose(); // throws NPE cuz char isnt in char pool when func is called
		//}
	}
}
