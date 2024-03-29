﻿using NTDLS.ReliableMessaging;

namespace Si.Library.Messages.Notify
{
    /// <summary>
    /// Tell the server that a connection has selected the lobby. This does not mean
    /// that they have selected a loadout yet. That is denoted with a call to SetWaitingInLobby().
    /// </summary>
    public class SiRegisterToLobby : IRmNotification
    {
        public Guid LobbyUID { get; set; }
        public string PlayerName { get; set; }

        public SiRegisterToLobby(Guid lobbyUID, string playerName)
        {
            LobbyUID = lobbyUID;
            PlayerName = playerName;
        }
    }
}
