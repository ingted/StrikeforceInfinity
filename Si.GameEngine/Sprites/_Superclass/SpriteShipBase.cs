﻿using Si.GameEngine.Core;
using Si.GameEngine.Sprites.Weapons._Superclass;
using Si.GameEngine.Utility;
using Si.Library;
using Si.Library.ExtensionMethods;
using Si.Library.Types;
using Si.Library.Types.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Si.GameEngine.Sprites._Superclass
{
    /// <summary>
    /// The ship base is a ship object that moves, can be hit, explodes and can be the subject of locking weapons.
    /// </summary>
    public class SpriteShipBase : SpriteBase
    {
        private readonly Dictionary<string, WeaponBase> _droneWeaponsCache = new();

        public SpriteRadarPositionIndicator RadarPositionIndicator { get; protected set; }
        public SpriteRadarPositionTextBlock RadarPositionText { get; protected set; }
        public SiTimeRenewableResources RenewableResources { get; set; } = new();

        private readonly string _assetPathlockedOnImage = @"Graphics\Weapon\Locked On.png";
        private readonly string _assetPathlockedOnSoftImage = @"Graphics\Weapon\Locked Soft.png";
        private readonly string _assetPathHitSound = @"Sounds\Ship\Object Hit.wav";
        private readonly string _assetPathshieldHit = @"Sounds\Ship\Shield Hit.wav";

        private const string _assetPathExplosionAnimation = @"Graphics\Animation\Explode\Explosion 256x256\";
        private readonly int _explosionAnimationCount = 6;
        private int _selectedExplosionAnimationIndex = 0;

        private const string _assetPathHitExplosionAnimation = @"Graphics\Animation\Explode\Hit Explosion 22x22\";
        private readonly int _hitExplosionAnimationCount = 2;
        private int _selectedHitExplosionAnimationIndex = 0;

        private const string _assetExplosionSoundPath = @"Sounds\Explode\";
        private readonly int _explosionSoundCount = 4;
        private int _selectedExplosionSoundIndex = 0;

        public SpriteShipBase(GameEngineCore gameEngine, string name = "")
            : base(gameEngine, name)
        {
            if (IsDrone)
            {
                BuildDroneWeaponsCache();
            }

            _gameEngine = gameEngine;
        }

        public override void Initialize(string imagePath = null)
        {
            _hitSound = _gameEngine.Assets.GetAudio(_assetPathHitSound, 0.5f);
            _shieldHit = _gameEngine.Assets.GetAudio(_assetPathshieldHit, 0.5f);

            _selectedExplosionSoundIndex = SiRandom.Generator.Next(0, 1000) % _explosionSoundCount;
            _explodeSound = _gameEngine.Assets.GetAudio(Path.Combine(_assetExplosionSoundPath, $"{_selectedExplosionSoundIndex}.wav"), 1.0f);

            _selectedExplosionAnimationIndex = SiRandom.Generator.Next(0, 1000) % _explosionAnimationCount;
            _explosionAnimation = new SpriteAnimation(_gameEngine, Path.Combine(_assetPathExplosionAnimation, $"{_selectedExplosionAnimationIndex}.png"), new Size(256, 256));

            _selectedHitExplosionAnimationIndex = SiRandom.Generator.Next(0, 1000) % _hitExplosionAnimationCount;
            _hitExplosionAnimation = new SpriteAnimation(_gameEngine, Path.Combine(_assetPathHitExplosionAnimation, $"{_selectedHitExplosionAnimationIndex}.png"), new Size(22, 22));

            _lockedOnImage = _gameEngine.Assets.GetBitmap(_assetPathlockedOnImage);
            _lockedOnSoftImage = _gameEngine.Assets.GetBitmap(_assetPathlockedOnSoftImage);

            base.Initialize(imagePath);
        }

        /// <summary>
        /// Fires a drone weapon (a weapon without ammo limits).
        /// </summary>
        /// <param name="weaponTypeName"></param>
        /// <returns></returns>
        public bool FireDroneWeapon(string weaponTypeName)
        {
            return GetDroneWeaponByTypeName(weaponTypeName)?.Fire() == true;
        }

        /// <summary>
        /// Builds the cache of all weapons so the drone can fire quickly.
        /// </summary>
        private void BuildDroneWeaponsCache()
        {
            var allWeapons = SiReflection.GetSubClassesOf<WeaponBase>();

            foreach (var weapon in allWeapons)
            {
                _ = GetDroneWeaponByTypeName(weapon.Name);
            }
        }

        /// <summary>
        /// Gets a cached drone weapon (a weapon without ammo limits).
        /// </summary>
        /// <param name="weaponTypeName"></param>
        /// <returns></returns>
        private WeaponBase GetDroneWeaponByTypeName(string weaponTypeName)
        {
            if (_droneWeaponsCache.TryGetValue(weaponTypeName, out var weapon))
            {
                return weapon;
            }

            var weaponType = SiReflection.GetTypeByName(weaponTypeName);
            weapon = SiReflection.CreateInstanceFromType<WeaponBase>(weaponType, new object[] { _gameEngine, this });

            _droneWeaponsCache.Add(weaponTypeName, weapon);

            return weapon;
        }

        public override void Explode()
        {
            _explodeSound?.Play();
            _explosionAnimation?.Reset();
            _gameEngine.Sprites.Animations.AddAt(_explosionAnimation, this);
            base.Explode();
        }

        public void CreateParticlesExplosion()
        {
            _gameEngine.Sprites.Particles.CreateRandomShipPartParticlesAt(this, SiRandom.Between(30, 50));
            _gameEngine.Audio.PlayRandomExplosion();
        }


        public void FixRadarPositionIndicator()
        {
            if (RadarPositionIndicator != null)
            {
                if (_gameEngine.Display.GetCurrentScaledScreenBounds().IntersectsWith(RenderBounds, -50) == false)
                {
                    RadarPositionText.DistanceValue = Math.Abs(DistanceTo(_gameEngine.Player.Sprite));

                    RadarPositionText.Visable = true;
                    RadarPositionText.IsFixedPosition = true;
                    RadarPositionIndicator.Visable = true;
                    RadarPositionIndicator.IsFixedPosition = true;

                    double requiredAngle = _gameEngine.Player.Sprite.AngleTo360(this);

                    RadarPositionIndicator.Location = _gameEngine.Display.CenterScreen
                        + SiMath.PointFromAngleAtDistance360(new SiAngle(requiredAngle), new SiPoint(200, 200));
                    RadarPositionIndicator.Velocity.Angle.Degrees = requiredAngle;

                    RadarPositionText.Location = _gameEngine.Display.CenterScreen
                        - SiMath.PointFromAngleAtDistance360(new SiAngle(requiredAngle) - 180, new SiPoint(120, 120));
                    RadarPositionIndicator.Velocity.Angle.Degrees = requiredAngle;
                }
                else
                {
                    RadarPositionText.Visable = false;
                    RadarPositionIndicator.Visable = false;
                }
            }
        }

        public override void Cleanup()
        {
            if (RadarPositionIndicator != null)
            {
                RadarPositionIndicator.QueueForDelete();
                RadarPositionText.QueueForDelete();
            }

            base.Cleanup();
        }
    }
}
