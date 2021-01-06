using Rebirth.Characters.Modify;
using Rebirth.Common.Types;
using Rebirth.Entities.Item;
using Rebirth.Server.Center;
using Rebirth.Tools;

namespace Rebirth.Scripts.Npc
{
	public class NpcScriptContext : ScriptContextBase<NpcScript>
	{
		public NpcScriptContext(NpcScript script) : base(script)
		{
			IsNpcScript = true;
		}

		// ------------------ Dialog Stuff

		public int SayOk(string sText) => Say(sText, false, false); // i think this is ok???
		public int SayNext(string sText) => Say(sText, false, true);
		public int SayPrev(string sText) => Say(sText, true, false);
		public int SayNextPrev(string sText) => Say(sText, true, true);

		public int Say(string sText, bool bPrev = false, bool bNext = true)
		{
			var pMsg = new NpcSayMessage()
			{
				sText = sText,
				bPrev = bPrev,
				bNext = bNext
			};

			Script.PrevMsgs.Add(pMsg);

			return Script.SendMessage<byte>(ScriptMsgType.Say, p =>
			{
				p.EncodeString(sText);
				p.Encode1(bPrev);
				p.Encode1(bNext);
			});
		}

		public int SayImage(string[] asImgPaths)
		{
			return Script.SendMessage<byte>(ScriptMsgType.SayImage, p =>
			{
				p.Encode1((byte)asImgPaths.Length);

				foreach (var img in asImgPaths)
				{
					p.EncodeString(img); // CUtilDlgEx::AddImageList(v8, sPath);
				}
			});
		}

		public int AskYesNo(string sText)
		{
			var ret = Script.SendMessage<byte>(ScriptMsgType.AskYesNo, p =>
			{
				p.EncodeString(sText);
			});

			return ret;
		}

		public string AskText(string sMsg, string sMsgDefault = "", short nLenMin = 1, short nLenMax = 50)
		{
			return Script.SendMessage<string>(ScriptMsgType.AskText, p =>
			{
				p.EncodeString(sMsg);
				p.EncodeString(sMsgDefault);
				p.Encode2(nLenMin);
				p.Encode2(nLenMax);
			});
		}

		public int AskNumber(string sMsg, int nDef, int nMin, int nMax)
		{
			return Script.SendMessage<int>(ScriptMsgType.AskNumber, p =>
			{
				p.EncodeString(sMsg);
				p.Encode4(nDef);
				p.Encode4(nMin);
				p.Encode4(nMax);
			});
		}

		public int AskMenu(string sMsg)
		{
			return Script.SendMessage<int>(ScriptMsgType.AskMenu, p =>
			{
				p.EncodeString(sMsg);
			});
		}

		public int AskQuiz(string sMsg, InitialQuizRes nResCode, string sTitle, string sProblemText, string sHintText, int nMinInput, int nMaxInput, int tRemainInitialQuiz)
		{
			return Script.SendMessage<byte>(ScriptMsgType.AskQuiz, p =>
			{
				p.Encode1((byte)nResCode);

				if (nResCode == InitialQuizRes.Request) // fail has no bytes
				{
					p.EncodeString(sTitle);
					p.EncodeString(sProblemText);
					p.EncodeString(sHintText);
					p.Encode4(nMinInput);
					p.Encode4(nMaxInput);
					p.Encode4(tRemainInitialQuiz);
				}
			});
		}

		public int AskSpeedQuiz(string sMsg, InitialQuizRes nResCode, int nType, int dwAnswer, int nCorrect, int nRemain, int tRemainInitialQuiz)
		{
			return Script.SendMessage<byte>(ScriptMsgType.AskSpeedQuiz, p =>
			{
				p.Encode1((byte)nResCode);

				if (nResCode == InitialQuizRes.Request) // fail has no bytes
				{
					p.Encode1((byte)nResCode);

					if (nResCode == InitialQuizRes.Request) // fail has no bytes
					{
						p.Encode4(nType);
						p.Encode4(dwAnswer);
						p.Encode4(nCorrect);
						p.Encode4(nRemain);
						p.Encode4(tRemainInitialQuiz);
					}
				}
			});
		}

		/// <summary>
		/// Return values:
		/// 0: Success
		/// 1: Internal failure
		/// 2: Unable to find item id in inventory
		/// </summary>
		public int MakeRandAvatar(int nItemID, int[] anCanadite)
		{
			if (anCanadite.Length <= 0) return 1; // invalid input

			var itemslot = Character.InventoryCash.FindItemSlot(nItemID);

			if (itemslot <= 0) return 2; // unable to find item

			var retIndex = 0;

			if (retIndex > anCanadite.Length) return 1; // invalid selection

			// TODO consider validating item template existence 

			Character.Modify.Stats(ctx =>
			{
				var selectionItem = anCanadite.Random();
				if (selectionItem / 10000 == 2)
				{
					ctx.Face = selectionItem;
					retIndex = 0;
				}
				else if (selectionItem / 10000 == 3)
				{
					ctx.Hair = selectionItem;
					retIndex = 0;
				}
				else
				{
					retIndex = 1; // invalid item ID
				}
			});

			InventoryManipulator.RemoveFrom(Character, InventoryType.Cash, itemslot, 1);

			return 0;
		}

		/// <summary>
		/// Return values:
		/// 0: Success
		/// 1: Internal failure
		/// 2: Unable to find item id in inventory
		/// </summary>
		public int AskAvatar(string sMsg, int nItemID, int[] anCanadite)
		{
			if (anCanadite.Length <= 0) return 1; // invalid input

			var retIndex = Script.SendMessage<byte>(
				ScriptMsgType.AskAvatar, p =>
				{
					p.EncodeString(sMsg);

					p.Encode1((byte)anCanadite.Length);

					foreach (var nCanadite in anCanadite)
					{
						p.Encode4(nCanadite); //hair id's and such
					}
				});

			var itemslot = Character.InventoryCash.FindItemSlot(nItemID);

			// check item existence after we show them the options
			if (itemslot <= 0) return 2; // unable to find item

			if (retIndex > anCanadite.Length) return 1; // invalid selection

			// TODO consider validating item template existence 

			Character.Modify.Stats(ctx =>
			{
				var selectionItem = anCanadite[retIndex];
				if (selectionItem / 10000 == 2)
				{
					ctx.Face = selectionItem;
					retIndex = 0;
				}
				else if (selectionItem / 10000 == 3)
				{
					ctx.Hair = selectionItem;
					retIndex = 0;
				}
				else if (selectionItem < 10 && selectionItem > 0)
				{
					ctx.Skin = (byte)selectionItem;
					retIndex = 0;
				}
				else
				{
					retIndex = 1; // invalid item ID
				}
			});

			if (retIndex == 0)
			{
				InventoryManipulator.RemoveFrom(Character, InventoryType.Cash, itemslot, 1);
			}

			return retIndex;
		}

		public int AskPet(string sMsg, params GW_ItemSlotPet[] apPet)
		{
			return Script.SendMessage<byte>(ScriptMsgType.AskPet, p =>
			{
				p.EncodeString(sMsg);

				p.Encode1((byte)apPet.Length);

				foreach (var pPet in apPet)
				{
					p.Encode8(pPet.liCashItemSN);
					p.Encode1(1); //Pet Slot
				}
			});
		}

		public int AskPetAll(string sMsg, params GW_ItemSlotPet[] apPet)
		{
			return Script.SendMessage<byte>(ScriptMsgType.AskPetAll, p =>
			{
				p.EncodeString(sMsg);

				p.Encode1((byte)apPet.Length);

				foreach (var pPet in apPet)
				{
					p.Encode8(pPet.liCashItemSN);
					p.Encode1(1); //Pet Slot
				}
			});
		}

		public int AskAccept(string sText)
		{
			return Script.SendMessage<byte>(ScriptMsgType.AskYesNoQuest, p =>
			{
				p.EncodeString(sText);
			});
		}

		public string AskBoxText(string sMsg, string sMsgDefault, short nCol, short nLine)
		{
			return Script.SendMessage<string>(ScriptMsgType.AskBoxText, p =>
			{
				p.EncodeString(sMsg);
				p.EncodeString(sMsgDefault);
				p.Encode2(nCol);
				p.Encode2(nLine);
			});
		}

		public int AskSlideMenu(string sMsg)
		{
			return Script.SendMessage<int>(ScriptMsgType.AskSlideMenu, p =>
			{
				p.Encode4(0);

				//if its zero
				//  CSlideMenuDlgEX::CSlideMenuDlgEX | Korean
				//else
				//   CSlideMenuDlg

				p.Encode4(0); //nIdx
				p.EncodeString(sMsg);
			});
		}

		//------------------ Markdown ----

		public string Selection(int nIdx, string sMsg, bool bNewLine = true)
			=> MapleMarkdownHelper.Selection(nIdx, sMsg) + (bNewLine ? "\r\n" : "");

		public string ItemSelection(int nIdx, int nItemID, bool bNewLine = true)
			=> MapleMarkdownHelper.Selection(nIdx, Markdown.ItemImage, nItemID, Markdown.ItemName, nItemID) + (bNewLine ? "\r\n" : "");

		public string MapSelection(int nIdx, int nItemID, bool bNewLine = true)
			=> MapleMarkdownHelper.Selection(nIdx, Markdown.MapName, nItemID) + (bNewLine ? "\r\n" : "");

		public string RedText(string sMsg, bool bBold = false)
			=> MapleMarkdownHelper.Text(MarkdownText.Red, sMsg, bBold);

		public string GreenText(string sMsg, bool bBold = false)
			=> MapleMarkdownHelper.Text(MarkdownText.Green, sMsg, bBold);

		public string BlueText(string sMsg, bool bBold = false)
			=> MapleMarkdownHelper.Text(MarkdownText.Blue, sMsg, bBold);

		public string PurpleText(string sMsg, bool bBold = false)
			=> MapleMarkdownHelper.Text(MarkdownText.Purple, sMsg, bBold);

		//------------------

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
