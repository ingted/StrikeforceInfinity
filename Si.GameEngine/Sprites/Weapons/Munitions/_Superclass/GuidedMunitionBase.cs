﻿using Si.GameEngine.Core;
using Si.GameEngine.Sprites._Superclass;
using Si.GameEngine.Sprites.Weapons._Superclass;
using Si.Library.ExtensionMethods;
using Si.Library.Types.Geometry;

namespace Si.GameEngine.Sprites.Weapons.Munitions._Superclass
{
    /// <summary>
    /// Guided munitions need to be locked onto a target before they are fired. They will adjust heading within given parameters to hit the locked target.
    /// </summary>
    internal class GuidedMunitionBase : MunitionBase
    {
        public int MaxGuidedObservationAngleDegrees { get; set; } = 90;
        public int GuidedRotationRateInDegrees { get; set; } = 3;
        public SpriteBase LockedTarget { get; private set; }

        public GuidedMunitionBase(GameEngineCore gameEngine, WeaponBase weapon, SpriteBase firedFrom, string imagePath,
             SpriteBase lockedTarget = null, SiPoint xyOffset = null)
            : base(gameEngine, weapon, firedFrom, imagePath, xyOffset)
        {
            LockedTarget = lockedTarget;
        }

        public override void ApplyIntelligence(SiPoint displacementVector)
        {
            if (LockedTarget != null)
            {
                if (LockedTarget.Visable)
                {
                    var deltaAngle = DeltaAngle(LockedTarget);
                    if (deltaAngle.IsBetween(-MaxGuidedObservationAngleDegrees, MaxGuidedObservationAngleDegrees))
                    {

                        if (deltaAngle >= 0) //We might as well turn around clock-wise
                        {
                            Velocity.Angle += GuidedRotationRateInDegrees;
                        }
                        else if (deltaAngle < 0) //We might as well turn around counter clock-wise
                        {
                            Velocity.Angle -= GuidedRotationRateInDegrees;
                        }
                    }
                }
            }

            base.ApplyIntelligence(displacementVector);
        }
    }
}
