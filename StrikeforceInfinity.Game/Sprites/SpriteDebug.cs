﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using System.Drawing;

namespace StrikeforceInfinity.Game.Sprites
{
    internal class SpriteDebug : _SpriteShipBase
    {
        public SpriteDebug(EngineCore core)
            : base(core)
        {
            Initialize(@"Graphics\Debug.png", new Size(64, 64));
            X = 0;
            Y = 0;
            Velocity = new HgVelocity();
        }

        public SpriteDebug(EngineCore core, double x, double y)
            : base(core)
        {
            Initialize(@"Graphics\Debug.png", new Size(64, 64));
            X = x;
            Y = y;
            Velocity = new HgVelocity();
        }

        public SpriteDebug(EngineCore core, double x, double y, string imagePath)
            : base(core)
        {
            Initialize(imagePath);
            X = x;
            Y = y;
            Velocity = new HgVelocity();
        }

        public override void ApplyMotion(SiPoint displacementVector)
        {
            Velocity.Angle.Degrees = AngleTo360(_core.Player.Sprite);
            base.ApplyMotion(displacementVector);
        }
    }
}