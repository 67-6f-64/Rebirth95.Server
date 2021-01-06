using Rebirth.Characters;
using Rebirth.Game;
using System;
using System.Collections.Generic;
using System.Text;
using Rebirth.Common.Types;

namespace Rebirth.Scripts.Portal
{
    public class PortalScriptContext : ScriptContextBase<PortalScript>
    {
        private Character Player => Script.Parent.Character;

        public PortalScriptContext(PortalScript script) : base(script) { }

        //------------------

        public void DisableMinimap() => Player.SendPacket(CPacket.MiniMapOnOff(false));
        public void EnableMinimap() => Player.SendPacket(CPacket.MiniMapOnOff(true));

        public void ShowInstruction(string hintMessage, short width)
            => Player.SendPacket(CPacket.BalloonMessage(hintMessage, width));
        public void ShowInfo(string hintMessage, short width)
            => ShowInstruction(hintMessage, width);

        public void BlockPortal()
        {

        }

        public void UnblockPortal()
        {

        }

		public void PlayPortalSound() => new UserEffectPacket(UserEffect.PortalSoundEffect).BroadcastEffect(Character, true, false);

        public bool ContainsAreaInfo()
        {
            return true;
        }

        public void UpdateAreaInfo()
        {

        }

        public void Warp(int nFieldID, int nPortalID = 0, bool bSoundEffect = true)
        {
            if (bSoundEffect)
                PlayPortalSound();

            base.Warp(nFieldID, nPortalID);
        }

        public override void Warp(int nFieldID, int nPortalID = 0)
        {
            PlayPortalSound(); 
            base.Warp(nFieldID, nPortalID);
        }

        public override void Warp(int mapId, string sPortalName)
        {
            PlayPortalSound();
            base.Warp(mapId, sPortalName);
        }
    }
}
