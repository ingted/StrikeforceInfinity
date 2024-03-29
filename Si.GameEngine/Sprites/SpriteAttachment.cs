﻿using Si.GameEngine.Core;
using Si.GameEngine.Sprites._Superclass;
using Si.GameEngine.Sprites.Weapons.Munitions._Superclass;
using Si.Library.Types;
using Si.Library.Types.Geometry;
using static Si.Library.SiConstants;

namespace Si.GameEngine.Sprites
{
    public class SpriteAttachment : SpriteShipBase
    {
        public bool TakesDamage { get; set; }

        public SpriteAttachment(GameEngineCore gameEngine, string imagePath)
            : base(gameEngine)
        {
            Initialize(imagePath);

            X = 0;
            Y = 0;
            Velocity = new SiVelocity();
        }

        public override bool TryMunitionHit(MunitionBase munition, SiPoint hitTestPosition)
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
