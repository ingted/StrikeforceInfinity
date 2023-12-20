﻿using NTDLS.StreamFraming.Payloads;
using StrikeforceInfinity.Shared.Payload;

namespace StrikeforceInfinity.Shared.Messages.Query
{
    /// <summary>
    /// Gets some basic information about a lobby.
    /// </summary>
    public class SiGetLobbyInfo : IFramePayloadQuery
    {
        public Guid LobyUID { get; set; }
    }

    public class SiGetLobbyInfoReply : IFramePayloadQueryReply
    {
        public SiLobbyInfo Info { get; set; } = new();
    }
}
