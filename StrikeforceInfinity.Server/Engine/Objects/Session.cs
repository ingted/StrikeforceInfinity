﻿using System.Net;

namespace StrikeforceInfinity.Server.Engine.Objects
{
    internal class Session
    {
        public IPEndPoint? Endpoint { get; private set; }
        public DateTime LastSeenDatetime { get; set; }
        public Guid ConnectionId { get; private set; }
        public IPAddress IpAdress { get; private set; }

        /// <summary>
        /// The lobby that the connection is registered for, if any.
        /// </summary>
        public Guid CurrentLobbyUID { get; set; }

        public Session(Guid sessionId, IPAddress ipAdress)
        {
            ConnectionId = sessionId;
            LastSeenDatetime = DateTime.UtcNow;
            IpAdress = ipAdress;
        }

        public void SetRemoteEndpointPort(int port)
        {
            Endpoint = new IPEndPoint(IpAdress, port);
        }

        /// <summary>
        /// Keep track of which lobby the connection was last associated with so that we can easily deregister.
        /// </summary>
        /// <param name="gameLobbyUID"></param>
        public void SetCurrentLobby(Guid lobbyUID)
        {
            CurrentLobbyUID = lobbyUID;
        }
    }
}
