﻿using Si.Game.Engine;
using Si.Game.Sprites;
using Si.Game.Utility;
using Si.Game.Weapons.BasesAndInterfaces;
using Si.Game.Weapons.Munitions;
using Si.Shared.Types.Geometry;

namespace Si.Game.Weapons
{
    internal class WeaponThunderstrikeMissile : WeaponBase
    {
        static new string Name { get; } = "Thunderstrike Missile";
        private const string soundPath = @"Sounds\Weapons\ThunderstrikeMissile.wav";
        private const float soundVolumne = 0.4f;

        private bool _toggle = false;

        public WeaponThunderstrikeMissile(EngineCore gameCore, _SpriteShipBase owner)
            : base(gameCore, owner, Name, soundPath, soundVolumne) => InitializeWeapon();

        public WeaponThunderstrikeMissile(EngineCore gameCore)
            : base(gameCore, Name, soundPath, soundVolumne) => InitializeWeapon();

        private void InitializeWeapon()
        {
            Damage = 20;
            FireDelayMilliseconds = 250;
            Speed = 15;
            SpeedVariancePercent = 0.10;

            CanLockOn = false;
            ExplodesOnImpact = true;
        }

        public override MunitionBase CreateMunition(SiPoint xyOffset, SpriteBase targetOfLock = null)
        {
            return new MunitionFragMissile(_gameCore, this, _owner, xyOffset);
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
                        var pointRight = SiMath.PointFromAngleAtDistance360(_owner.Velocity.Angle + 90, new SiPoint(10, 10));
                        _gameCore.Sprites.Munitions.Create(this, pointRight);
                    }
                    else
                    {
                        var pointLeft = SiMath.PointFromAngleAtDistance360(_owner.Velocity.Angle - 90, new SiPoint(10, 10));
                        _gameCore.Sprites.Munitions.Create(this, pointLeft);
                    }

                    _toggle = !_toggle;
                }

                return true;
            }

            return false;

        }
    }
}
