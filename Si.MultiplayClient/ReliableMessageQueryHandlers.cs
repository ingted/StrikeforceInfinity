﻿using NTDLS.ReliableMessaging;
using Si.Library.Messages.Query;

namespace Si.MultiplayClient
{
    /// <summary>
    /// Recevies and replies to queries from the multiplayer host.
    /// </summary>
    internal class ReliableMessageQueryHandlers : IRmMessageHandler
    {
        private readonly EngineMultiplayManager _manager;

        public ReliableMessageQueryHandlers(EngineMultiplayManager manager)
        {
            _manager = manager;
        }

        public SiPingReply OnSiPing(RmContext context, SiPing param)
        {
            return new SiPingReply(param);
        }
    }
}
