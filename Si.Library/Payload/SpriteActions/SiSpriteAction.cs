﻿namespace Si.Library.Payload.SpriteActions
{
    public class SiSpriteAction
    {
        public Guid MultiplayUID { get; set; }

        public SiSpriteAction(Guid multiplayUID)
        {
            MultiplayUID = multiplayUID;
        }
    }
}
