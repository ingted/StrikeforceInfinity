﻿using NTDLS.StreamFraming.Payloads;
using Si.Library.Payload;

namespace Si.Library.Messages.Query
{
    /// <summary>
    /// Notification from the lobby owner containing the situation layout.
    /// This is then broadcast from the server to each connection.
    /// </summary>
    public class SiSituationLayout : IFramePayloadNotification
    {
        public List<SiSpriteLayout> Sprites { get; set; } = new();
    }
}