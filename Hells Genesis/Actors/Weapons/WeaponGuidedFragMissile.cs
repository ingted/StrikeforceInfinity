﻿using HG.Actors.BaseClasses;
using HG.Actors.Weapons.BaseClasses;
using HG.Actors.Weapons.Bullets;
using HG.Actors.Weapons.Bullets.BaseClasses;
using HG.Engine;
using HG.Types;

namespace HG.Actors.Weapons
{
    internal class WeaponGuidedFragMissile : WeaponBase
    {
        private const string soundPath = @"Sounds\Weapons\WeaponGuidedFragMissile.wav";
        private const float soundVolumne = 0.4f;

        private bool _toggle = false;

        public WeaponGuidedFragMissile(Core core)
            : base(core, "Guided Frag Missile", soundPath, soundVolumne)
        {
            RoundQuantity = 500;
            Damage = 10;
            FireDelayMilliseconds = 1000;
            Speed = 12;

            CanLockOn = true;
            MinLockDistance = 100;
            MaxLockDistance = 600;
            MaxLocks = 2;
            MaxLockOnAngle = 40;
            ExplodesOnImpact = true;
        }

        public override BulletBase CreateBullet(ActorBase lockedTarget, HgPoint<double> xyOffset = null)
        {
            return new BulletGuidedFragMissile(_core, this, _owner, lockedTarget, xyOffset);
        }

        public override bool Fire()
        {
            if (CanFire)
            {
                _fireSound.Play();
                RoundQuantity--;

                if (LockedOnObjects == null || LockedOnObjects.Count == 0)
                {
                    if (_toggle)
                    {
                        var pointRight = HgMath.AngleFromPointAtDistance(_owner.Velocity.Angle + 90, new HgPoint<double>(10, 10));
                        _core.Actors.Bullets.Create(this, _owner, pointRight);
                    }
                    else
                    {
                        var pointLeft = HgMath.AngleFromPointAtDistance(_owner.Velocity.Angle - 90, new HgPoint<double>(10, 10));
                        _core.Actors.Bullets.Create(this, _owner, pointLeft);
                    }

                    _toggle = !_toggle;
                }
                else
                {
                    foreach (var lockedOn in LockedOnObjects)
                    {
                        if (_toggle)
                        {
                            var pointRight = HgMath.AngleFromPointAtDistance(_owner.Velocity.Angle + 90, new HgPoint<double>(10, 10));
                            _core.Actors.Bullets.CreateLocked(this, _owner, lockedOn, pointRight);
                        }
                        else
                        {
                            var pointLeft = HgMath.AngleFromPointAtDistance(_owner.Velocity.Angle - 90, new HgPoint<double>(10, 10));
                            _core.Actors.Bullets.CreateLocked(this, _owner, lockedOn, pointLeft);
                        }
                        _toggle = !_toggle;
                    }
                }


                return true;
            }
            return false;

        }
    }
}
