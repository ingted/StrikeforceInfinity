﻿using Si.Game.Engine;
using Si.Game.Engine.Types;
using Si.Game.Weapons.Munitions;
using System.Drawing;
using static Si.Shared.SiConstants;
using Si.Shared.Types.Geometry;
using Si.Shared.Types;

namespace Si.Game.Sprites
{
    internal class SpriteAttachment : _SpriteShipBase
    {
        public bool TakesDamage { get; set; }

        public SpriteAttachment(EngineCore gameCore, string imagePath, Size? size = null)
            : base(gameCore)
        {
            Initialize(imagePath, size);

            X = 0;
            Y = 0;
            Velocity = new SiVelocity();
        }

        public override bool TryMunitionHit(SiPoint displacementVector, MunitionBase munition, SiPoint hitTestPosition)
        {
            if (munition.FiredFromType == SiFiredFromType.Player)
            {
                if (Intersects(hitTestPosition))
                {
                    Hit(munition);
                    if (HullHealth <= 0)
                    {
                        Explode();
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
