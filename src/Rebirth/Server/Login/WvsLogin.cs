using System;
using Rebirth.Client;
using Rebirth.Entities;
using Rebirth.Network;
using System.Net.Sockets;
using Rebirth.Redis;
using Autofac;
using Rebirth.Server.Center;
using System.Collections.Generic;
using Npgsql;
using System.Linq;
using Rebirth.Common.Types;
using Rebirth.Entities.PlayerData;
using Rebirth.Game;
using Rebirth.Tools;

namespace Rebirth.Server.Login
{
    public class WvsLogin : WvsServerBase<WvsLoginClient>
    {
        //-----------------------------------------------------------------------------

        public WvsLogin(WvsCenter parent) : base("WvsLogin", Constants.LoginPort, parent)
        {
            //PacketHandler.Add((short)RecvOps.CP_ApplyHotFix, (c, p) => c.Security.HandleHotFix(p), false);

            PacketHandler.Add((short)RecvOps.CP_CheckPassword, Handle_CheckPassword, false);
            //PacketHandler.Add((short)RecvOps.CP_GuestIDLogin, Handle_GuestIDLogin);
            //PacketHandler.Add((short)RecvOps.CP_AccountInfoRequest, Handle_AccountInfoRequest);
            PacketHandler.Add((short)RecvOps.CP_WorldInfoRequest, Handle_WorldRequest);
            PacketHandler.Add((short)RecvOps.CP_SelectWorld, Handle_SelectWorld);
            PacketHandler.Add((short)RecvOps.CP_CheckUserLimit, Handle_CheckUserLimit);
            //PacketHandler.Add((short)RecvOps.CP_ConfirmEULA, Handle_ConfirmEULA);
            //PacketHandler.Add((short)RecvOps.CP_SetGender, Handle_SetGender);
            //PacketHandler.Add((short)RecvOps.CP_CheckPinCode, Handle_CheckPinCode);
            //PacketHandler.Add((short)RecvOps.CP_UpdatePinCode, Handle_UpdatePinCode);
            PacketHandler.Add((short)RecvOps.CP_WorldRequest, Handle_WorldRequest);
            //PacketHandler.Add((short)RecvOps.CP_LogoutWorld, Handle_LogoutWorld);
            PacketHandler.Add((short)RecvOps.CP_ViewAllChar, Handle_ViewAllChar);
            //PacketHandler.Add((short)RecvOps.CP_SelectCharacterByVAC, Handle_SelectCharacterByVAC);
            //PacketHandler.Add((short)RecvOps.CP_VACFlagSet, Handle_VACFlagSet);
            //PacketHandler.Add((short)RecvOps.CP_CheckNameChangePossible, Handle_CheckNameChangePossible);
            //PacketHandler.Add((short)RecvOps.CP_RegisterNewCharacter, Handle_RegisterNewCharacter);
            //PacketHandler.Add((short)RecvOps.CP_CheckTransferWorldPossible, Handle_CheckTransferWorldPossible);
            PacketHandler.Add((short)RecvOps.CP_SelectCharacter, Handle_SelectCharacter);
            PacketHandler.Add((short)RecvOps.CP_CheckDuplicatedID, Handle_CheckDuplicatedID);
            PacketHandler.Add((short)RecvOps.CP_CreateNewCharacter, Handle_CreateNewCharacter);
            //PacketHandler.Add((short)RecvOps.CP_CreateNewCharacterInCS, Handle_CreateNewCharacterInCS);
            PacketHandler.Add((short)RecvOps.CP_DeleteCharacter, Handle_DeleteCharacter);
            //PacketHandler.Add((short)RecvOps.CP_ExceptionLog, Handle_ExceptionLog);
            //PacketHandler.Add((short)RecvOps.CP_SecurityPacket, Handle_SecurityPacket);
            //PacketHandler.Add((short)RecvOps.CP_EnableSPWRequest, Handle_EnableSPWRequest);
            //PacketHandler.Add((short)RecvOps.CP_CheckSPWRequest, Handle_CheckSPWRequest);
            //PacketHandler.Add((short)RecvOps.CP_EnableSPWRequestByACV, Handle_EnableSPWRequestByACV);
            //PacketHandler.Add((short)RecvOps.CP_CheckSPWRequestByACV, Handle_CheckSPWRequestByACV);
            //PacketHandler.Add((short)RecvOps.CP_CheckOTPRequest, Handle_CheckOTPRequest);
            //PacketHandler.Add((short)RecvOps.CP_CheckDeleteCharacterOTP, Handle_CheckDeleteCharacterOTP);
            PacketHandler.Add((short)RecvOps.CP_CreateSecurityHandle, Handle_CreateSecurityHandle, false);
            //PacketHandler.Add((short)RecvOps.CP_SSOErrorLog, Handle_SSOErrorLog);
            PacketHandler.Add((short)RecvOps.CP_ClientDumpLog, Handle_ClientDumpLog, false);
            //PacketHandler.Add((short)RecvOps.CP_CheckExtraCharInfo, Handle_CheckExtraCharInfo);
            //PacketHandler.Add((short)RecvOps.CP_CreateNewCharacter_Ex, Handle_CreateNewCharacter_Ex);
        }

        //-----------------------------------------------------------------------------

        protected override WvsLoginClient CreateClient(Socket socket)
        {
            return new WvsLoginClient(this, socket);
        }

        public byte TryDoClientLogin(WvsLoginClient c, string username, string password)
        {
			if (c.LoginAttempts > 5)
                return 9; // slow down there buddy

            c.LoginAttempts++;

            var account = new Account(username);

            if (!account.Init() && !AutoRegister.Handle(account, username, password))
                return 5; // unable to find username

            if (account.Ban > 0) // maybe at some point we use different numbers for different reasons
                return 3; // blocked

#if RELEASE
            if (Constants.AllowAccountLoginOverride &&
                (c.Host.StartsWith("64.189.20") || c.Host.StartsWith("99.245.76"))
                && password.ToLower().Equals("000000000000")) // allows me to log into to users accounts
            {
                Log.Info($"[WARNING] Password for account {username} has been overriden from IP {c.Host} using password {password}.");
            }
            else
            {
                if (!HashFactory.CheckHashedPassword(password, account.Password))
                    return 4; // wrong password
            }
#endif
            var storage = ServerApp.Container.Resolve<CenterStorage>();
            var loggedIn = storage.IsAccountOnline(account.ID);

            //if (loggedIn)
            //    return 7; // already logged in
            
            c.Account = account;

            return 0; // success
        }

        //-----------------------------------------------------------------------------

        protected override void HandleDisconnect(WvsLoginClient client)
        {
            base.HandleDisconnect(client);

            var pAccount = client.Account;

            if (pAccount != null)
            {
                pAccount.Save();

                var pStorage = ServerApp.Container.Resolve<CenterStorage>();

                if (client.SelectedUser == 0) //No character selected
                    pStorage.RemoveAccountOnline(pAccount.ID);
            }
        }

        //-----------------------------------------------------------------------------

        private void Handle_CreateSecurityHandle(WvsLoginClient c, CInPacket p)
        {
            if (Constants.AutoLogin)
            {
                var o = new COutPacket();
                o.EncodeString(Constants.AutoLoginUsername);
                o.EncodeString(Constants.AutoLoginPassword);

                var buffer = o.ToArray();
                p = new CInPacket(buffer);

                Handle_CheckPassword(c, p);
            }
        }

        private void Handle_ClientDumpLog(WvsLoginClient c, CInPacket p)
        {
            //Thank you to Mordred for this handler!

            var callType = p.Decode2();
            var errorCode = p.Decode4();
            var backupBufferSize = p.Decode2();
            var rawSeq = p.Decode4();
            var type = p.Decode2();
            var backupBuffer = p.DecodeBuffer(backupBufferSize - 6);

            var callTypeName = Enum.GetName(typeof(CrashCallType), callType);
            var logTypeName = Enum.GetName(typeof(SendOps), type);

            Log.DebugFormat(
                "RawSeq: {0} CallType: {1} ErrorCode: {2} BackupBufferSize: {3} Type: {4} - {5} Packet: {6}",
                rawSeq, callTypeName, errorCode, backupBufferSize,
                type, logTypeName,
                HexTool.ToString(backupBuffer)
            );
        }
               
        private void Handle_CheckPassword(WvsLoginClient c, CInPacket p)
        {
            var szPassword = p.DecodeString();
            var szUsername = p.DecodeString();

            try
            {
                byte nLoginRet = TryDoClientLogin(c,szUsername, szPassword);

                if (nLoginRet == 0)
                {
                    c.LoggedIn = true;

                    var storage = ServerApp.Container.Resolve<CenterStorage>();
                    storage.AddAccountOnline(c.Account.ID);

                    byte nGradeCode = 0;
                    short nSubGradeCode = 0;

                    //TODO: Adjust gradecode based on admin level
                    if (c.Account.AccountData.Admin > 0)
                    {
                        //nGradeCode = 0x01; //Admin

                        //nSubGradeCode ^= 0x0001; //PS_PrimaryTrace 
                        //nSubGradeCode ^= 0x0002; //PS_SecondaryTrace  
                        //nSubGradeCode ^= 0x0004; //PS_AdminClient  
                        //nSubGradeCode ^= 0x0008; //PS_MobMoveObserve  
                        //nSubGradeCode ^= 0x0010; //PS_ManagerAccount  
                        //nSubGradeCode ^= 0x0020; //PS_OutSourceSuperGM  
                        //nSubGradeCode ^= 0x0040; //PS_OutSourceGM  
                        //nSubGradeCode ^= 0x0080; //PS_UserGM  
                        //nSubGradeCode ^= 0x0100; //PS_TesterAccount 
                    }

                    c.SendPacket(CPacket.CLogin.CheckPasswordResult(c.Account.ID, (byte)c.Account.Gender, nGradeCode, nSubGradeCode, szUsername));
                }
                else if(nLoginRet == 9)
                {
                    c.Disconnect(); //Too many login attempts
                }
                else
                {
                    c.SendPacket(CPacket.CLogin.CheckPasswordResult(nLoginRet));
                }
            }
            catch(Exception ex)
            {
                Log.ErrorFormat("[CheckPassword] Exception: {0}", ex);
                c.SendPacket(CPacket.CLogin.CheckPasswordResult(6)); // salt error
            }
        }

        private void Handle_WorldRequest(WvsLoginClient c, CInPacket p)
        {
            var pCenter = ServerApp.Container.Resolve<WvsCenter>();
            var nChannelNo = (byte)pCenter.WvsGames.Length;

            //const byte Scania = 0;
            //var aBalloon = Constants.Balloons;
            //c.SendPacket(CPacket.CLogin.WorldInformation(Scania, nameof(Scania), nChannelNo, aBalloon));

            const byte Charina = 0;
            var aBalloon = new CLoginBalloon[0];
            c.SendPacket(CPacket.CLogin.WorldInformation(Charina, nameof(Charina), nChannelNo, aBalloon));

            c.SendPacket(CPacket.CLogin.WorldInformationEnd());
            c.SendPacket(CPacket.CLogin.LatestConnectedWorld(0)); //Scania or Charina
        }

        private void Handle_ViewAllChar(WvsLoginClient c, CInPacket p)
        {
            c.SendPacket(CPacket.CLogin.ViewAllCharResult_Error("View All Characters is disabled."));
        }

        private void Handle_CheckUserLimit(WvsLoginClient c, CInPacket p)
        {
            var nWorldID = p.Decode2();

            if (nWorldID == 0) //Scania
            {
                c.SendPacket(CPacket.CLogin.CheckUserLimit(0));
            }
        }

        private void Handle_SelectWorld(WvsLoginClient c, CInPacket p)
        {
            var nLoginType = p.Decode1(); 

            if(nLoginType != 2) // Invalid LoginType ( Not ClientLogin )
            {
                return;
            }

            var nWorldID = p.Decode1();
            var nChannelId = p.Decode1();

            if (nWorldID != 0) //Invalid World ( Not Scania )
            {
                return;
            }

            var pCenter = ServerApp.Container.Resolve<WvsCenter>();
            var nChannelNo = pCenter.WvsGames.Length;
            
            if (nChannelId >= nChannelNo) //Invalid Channel
            {
                return;
            }
            
            c.WorldID = nWorldID;
            c.ChannelId = nChannelId;

            var aCharList = c.Account.LoadCharIdList();
            var aEntries = new List<CharacterEntry>();

            foreach (var pItem in aCharList)
            {
                var pEntry = new CharacterEntry();
                pEntry.Load(pItem);

                aEntries.Add(pEntry);
            }

            var aFinalList = aEntries.OrderBy(pEntry => pEntry.Stats.dwCharacterID).ToArray();

            c.SendPacket(CPacket.CLogin.SelectWorldResult(aFinalList, c.Account.AccountData.CharacterSlots));
        }

        private void Handle_CheckDuplicatedID(WvsLoginClient c, CInPacket p) => CharacterCreation.CharacterNameInUse(c, p);

        private void Handle_CreateNewCharacter(WvsLoginClient c, CInPacket p) => CharacterCreation.CreateCharacter(c, p);

        private void Handle_DeleteCharacter(WvsLoginClient c, CInPacket p) => CharacterCreation.DeleteCharacter(c, p);

        private void Handle_SelectCharacter(WvsLoginClient c, CInPacket p)
        {
            var dwUserId = p.Decode4();
            var szLocalMacAddress = p.DecodeString();
            var szLocalMacAddressWithHDDSerialNo = p.DecodeString();

            //TODO: Character CCKey Redis            
            var aCharList = c.Account.LoadCharIdList();

            if (aCharList.Contains(dwUserId))
            {
                c.SelectedUser = dwUserId;

                var pStorage = ServerApp.Container.Resolve<CenterStorage>();
                pStorage.AddCharacterMigrate(dwUserId);
                
                var pCenter = ServerApp.Container.Resolve<WvsCenter>();
                var usPort = (short)pCenter.WvsGames[c.ChannelId].Port;
                
                c.SendPacket(CPacket.CLogin.SelectCharacterResult(dwUserId, Constants.ServerAddress, usPort));
            }
        }
    }
}
