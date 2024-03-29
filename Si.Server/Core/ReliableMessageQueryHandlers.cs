﻿using NTDLS.ReliableMessaging;
using Si.Library.Messages.Query;
using Si.Library.Payload;

namespace Si.Server.Core
{
    /// <summary>
    /// Recevies and replies to queries from the multiplayer client (Si.MultiplayClient).
    /// </summary>
    internal class ReliableMessageQueryHandlers : IRmMessageHandler
    {
        private readonly ServerEngineCore _serverCore;

        public ReliableMessageQueryHandlers(ServerEngineCore serverCore)
        {
            _serverCore = serverCore;
        }

        public SiGetLobbyInfoReply OnSiGetLobbyInfo(RmContext context, SiGetLobbyInfo param)
        {
            _serverCore.Log.Verbose($"ConnectionId: '{context.ConnectionId}' getting lobby info for '{param.LobbyUID}'.");

            if (!_serverCore.Lobbies.TryGetByLobbyUID(param.LobbyUID, out var lobby))
            {
                throw new Exception($"The lobby was not found '{param.LobbyUID}'.");
            }

            return new SiGetLobbyInfoReply()
            {
                Info = new SiLobbyInfo
                {
                    Name = lobby.Name,
                    IsHeadless = lobby.IsHeadless,
                    RemainingSecondsUntilAutoStart = lobby.RemainingSecondsUntilAutoStart,
                    WaitingCount = lobby.ConnectionsWaitingInLobbyCount()
                }
            };
        }

        public SiCreateLobbyReply OnSiCreateLobby(RmContext context, SiCreateLobby param)
        {
            _serverCore.Log.Verbose($"ConnectionId: '{context.ConnectionId}' creating lobby '{param.Configuration.Name}'.");

            var lobby = _serverCore.Lobbies.Create(context.ConnectionId, param.Configuration);

            return new SiCreateLobbyReply()
            {
                LobbyUID = lobby.UID
            };
        }

        public SiListLobbiesReply OnSiListLobbies(RmContext context, SiListLobbies param)
        {
            _serverCore.Log.Verbose($"ConnectionId: '{context.ConnectionId}' requested lobby list.");

            var lobbies = _serverCore.Lobbies.GetList();

            return new SiListLobbiesReply()
            {
                Collection = lobbies
            };
        }

        public SiConfigureReply OnSiConfigure(RmContext context, SiConfigure param)
        {
            _serverCore.Log.Verbose($"ConnectionId: '{context.ConnectionId}' configuration (v{param.ClientVersion}).");

            if (!_serverCore.Sessions.TryGetByConnectionId(context.ConnectionId, out var session))
            {
                throw new Exception($"Session was not found for: '{context.ConnectionId}'.");
            }

            session.SetRemoteEndpointPort(param.ClientListenUdpPort);
            if (session.Endpoint == null)
            {
                throw new Exception($"Session endpoint can not be null: '{context.ConnectionId}'.");
            }

            _serverCore.ActiveEndpoints.Remove(context.ConnectionId);
            _serverCore.ActiveEndpoints.Add(context.ConnectionId, session.Endpoint);

            return new SiConfigureReply()
            {
                ConnectionId = context.ConnectionId,
                PlayerAbsoluteStateDelayMs = _serverCore.Settings.PlayerAbsoluteStateDelayMs
            };
        }
    }
}
