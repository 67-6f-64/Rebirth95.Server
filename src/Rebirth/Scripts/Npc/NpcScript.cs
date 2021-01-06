using log4net;
using Rebirth.Client;
using Rebirth.Common.Types;
using Rebirth.Network;
using Rebirth.Tools;
using System;
using System.Collections.Generic;

namespace Rebirth.Scripts.Npc
{
	public class NpcScript : ScriptBase
	{
		private static ILog Log = LogManager.GetLogger(typeof(NpcScript));

		public int SpeakerTemplateID { get; }
		public NpcScriptContext Context { get; }

		public ScriptMsgType? LastMsgType { get; private set; }

		public List<NpcSayMessage> PrevMsgs { get; }
		public int PrevIdx { get; set; }

		private AwaitValue<object> m_response;

		//------------------

		public NpcScript(int npcId, string sScriptName, string sScriptContents, WvsGameClient client)
			: base(sScriptName, sScriptContents, client)
		{
			SpeakerTemplateID = npcId;
			Context = new NpcScriptContext(this);
			LastMsgType = null;

			PrevMsgs = new List<NpcSayMessage>();
			PrevIdx = 0;
		}

		public override void InitLocals()
		{
			Engine.Set("ctx", Context);
			Engine.Set("script", this);
		}

		public T SendMessage<T>(ScriptMsgType nType, Action<COutPacket> pEncode)
		{
			return SendMessage<T>(nType, pEncode, ScriptFlagType.NPCReplacedByNPC, 0, SpeakerTemplateID);
		}

		private T SendMessage<T>(ScriptMsgType nType, Action<COutPacket> pEncode, ScriptFlagType nFlag, byte bParam, int nSpeakerTemplateID)
		{
			SendDialog(nType, pEncode, nFlag, bParam, nSpeakerTemplateID);

			m_response = new AwaitValue<object>();

			var task = m_response.Get();
			task.Wait();

			var result = task.Result;

			if (result is Exception ex)
			{
				throw ex;
			}
			else
			{
				return (T)result;
			}

			//TODO: Find a way to not block, async await doesnt work when called by python ( as below )
			//{
			//var resp = await Response.Get();
			//return (T)resp;
			//}
		}

		private void SendDialog(ScriptMsgType nType, Action<COutPacket> pEncode, ScriptFlagType nFlag, byte bParam, int nSpeakerTemplateID)
		{
			var outPacket = new COutPacket(SendOps.LP_ScriptMessage);
			outPacket.Encode1((byte)nFlag);
			outPacket.Encode4(SpeakerTemplateID);
			outPacket.Encode1((byte)nType);
			outPacket.Encode1(bParam);

			pEncode?.Invoke(outPacket);

			LastMsgType = nType;

			Parent.SendPacket(outPacket);
		}

		private void SendDialogSay(NpcSayMessage pState)
		{
			SendDialog(ScriptMsgType.Say, pState.Encode, ScriptFlagType.NPCReplacedByNPC, 0, SpeakerTemplateID);
		}

		public void PushResult(object item)
		{
			var resp = m_response;

			if (resp != null)
			{
				resp.Set(item);
			}
			else
			{
				Log.Error("ScriptResponse object not initialized");
			}
		}

		public void ProceedBack()
		{
			if (PrevIdx == 0)
			{
				// Hacking
				return;
			}

			PrevIdx--;

			var pState = PrevMsgs[PrevIdx];
			SendDialogSay(pState);
		}

		public void ProceedNext(object pResponse)
		{
			PrevIdx++;

			if (PrevIdx < PrevMsgs.Count)
			{
				// Usage of "next" button after the "back" button
				var pState = PrevMsgs[PrevIdx];
				SendDialogSay(pState);
			}
			else
			{
				PushResult(pResponse);
			}
		}

		public void EndChat()
		{
			PushResult(new NpcScriptException());
		}

		//------------------

		public override void Dispose()
		{
			Parent.ClearScripts(true);
			PrevMsgs.Clear();

			base.Dispose();
		}
	}
}
