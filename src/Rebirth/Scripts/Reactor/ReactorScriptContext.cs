using Rebirth.Field;
using Rebirth.Field.FieldObjects;
using Rebirth.Server.Center;

namespace Rebirth.Scripts.Reactor
{
	public class ReactorScriptContext : ScriptContextBase<ReactorScript>
    {
        public CReactor Reactor { get; }
        public bool WasHit { get; set; }

        public ReactorScriptContext(ReactorScript script, CReactor reactor, bool bHit) : base(script)
        {
            Reactor = reactor;
            WasHit = bHit;
        }

        public void DropItemOnGroundBelow(int nItemID, bool bMeso = false)
        {
	        if (!bMeso)
	        {
		        var item = MasterManager.CreateItem(nItemID);
		        CDropFactory.CreateDropItem(Reactor.Field, Reactor.Position.CurrentXY, 0, item);
            }
	        else
	        {
				CDropFactory.CreateDropMeso(Reactor.Field, Reactor.Position.CurrentXY, 0, nItemID);
	        }
        }
    }
}
