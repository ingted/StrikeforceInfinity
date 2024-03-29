﻿using NTDLS.ReliableMessaging;
using Si.Library.Payload;

namespace Si.Library.Messages.Query
{
    /// <summary>
    /// When a newcommer enters an existing game, the server will send
    /// this message to the lobby owner. The loby owner should then send
    /// a notification to the server with all and applicable spites to that
    /// the newcommer can come up to speed.
    /// </summary>
    public class SiRequestCurrentSituationLayout : IRmQuery<SiRequestCurrentSituationLayoutReply>
    {
        public SiRequestCurrentSituationLayout()
        {
            throw new NotImplementedException();
        }
    }

    public class SiRequestCurrentSituationLayoutReply : IRmQueryReply
    {
        public List<SiSpriteLayout> Sprites { get; set; } = new();
    }
}
