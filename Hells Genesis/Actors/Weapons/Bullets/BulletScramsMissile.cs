﻿using HG.Engine;
using HG.Types;

namespace HG.Actors.Weapons.Bullets
{
    internal class BulletScramsMissile : BulletBase
    {
        private const string imagePath = @"Graphics\Weapon\Missiles\BulletScramsMissile.png";

        public BulletScramsMissile(Core core, WeaponBase weapon, ActorBase firedFrom,
             ActorBase lockedTarget = null, HgPoint<double> xyOffset = null)
            : base(core, weapon, firedFrom, imagePath, lockedTarget, xyOffset)
        {
            Initialize(imagePath);
        }

        public override void ApplyIntelligence(HgPoint<double> displacementVector, dynamic testHit)
        {
            if (LockedTarget != null)
            {
                if (LockedTarget.Visable)
                {
                    var deltaAngle = DeltaAngle(LockedTarget);

                    if (deltaAngle >= 180.0) //We might as well turn around clock-wise
                    {
                        Velocity.Angle += 10;
                    }
                    else if (deltaAngle < 180.0) //We might as well turn around counter clock-wise
                    {
                        Velocity.Angle -= 10;
                    }
                }
            }

            base.ApplyIntelligence(displacementVector, testHit as ActorBase);
        }
    }
}
