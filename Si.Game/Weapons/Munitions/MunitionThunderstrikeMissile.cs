﻿using Si.Shared;
using Si.Game.Engine;
using Si.Game.Sprites;
using Si.Game.Utility;
using Si.Game.Weapons.BasesAndInterfaces;
using System.Drawing;
using System.IO;
using Si.Shared.Types.Geometry;

namespace Si.Game.Weapons.Munitions
{
    internal class MunitionThunderstrikeMissile : SeekingMunitionBase
    {
        private const string imagePath = @"Graphics\Weapon\ThunderstrikeMissile.png";

        private const string _assetPathHitExplosionAnimation = @"Graphics\Animation\Explode\Hit Explosion 66x66\";
        private readonly int _hitExplosionAnimationCount = 2;
        private int _selectedHitExplosionAnimationIndex = 0;

        public MunitionThunderstrikeMissile(EngineCore gameCore, WeaponBase weapon, SpriteBase firedFrom, SiPoint xyOffset = null)
            : base(gameCore, weapon, firedFrom, imagePath, xyOffset)
        {
            MaxSeekingObservationDistance = 1000;
            MaxSeekingObservationAngleDegrees = 20;
            SeekingRotationRateInDegrees = 4;

            _selectedHitExplosionAnimationIndex = SiRandom.Generator.Next(0, 1000) % _hitExplosionAnimationCount;
            _hitExplosionAnimation = new SpriteAnimation(_gameCore, Path.Combine(_assetPathHitExplosionAnimation, $"{_selectedHitExplosionAnimationIndex}.png"), new Size(66, 66));
        }
    }
}
