﻿using Si.GameEngine.Engine;
using Si.GameEngine.Weapons.BasesAndInterfaces;
using Si.GameEngine.Weapons.Munitions;
using Si.Shared;
using Si.Shared.Types;
using Si.Shared.Types.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Si.GameEngine.Sprites
{
    /// <summary>
    /// The ship base is a ship object that moves, can be hit, explodes and can be the subject of locking weapons.
    /// </summary>
    public class SpriteShipBase : SpriteBase
    {
        public bool IsDrone { get; private set; }

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

        public SpriteShipBase(EngineCore gameCore, string name = "")
            : base(gameCore, name)
        {
            IsDrone = GetType().Name.EndsWith("Drone");

            _gameCore = gameCore;
        }

        public override void Initialize(string imagePath = null, Size? size = null)
        {
            _hitSound = _gameCore.Assets.GetAudio(_assetPathHitSound, 0.5f);
            _shieldHit = _gameCore.Assets.GetAudio(_assetPathshieldHit, 0.5f);

            _selectedExplosionSoundIndex = SiRandom.Generator.Next(0, 1000) % _explosionSoundCount;
            _explodeSound = _gameCore.Assets.GetAudio(Path.Combine(_assetExplosionSoundPath, $"{_selectedExplosionSoundIndex}.wav"), 1.0f);

            _selectedExplosionAnimationIndex = SiRandom.Generator.Next(0, 1000) % _explosionAnimationCount;
            _explosionAnimation = new SpriteAnimation(_gameCore, Path.Combine(_assetPathExplosionAnimation, $"{_selectedExplosionAnimationIndex}.png"), new Size(256, 256));

            _selectedHitExplosionAnimationIndex = SiRandom.Generator.Next(0, 1000) % _hitExplosionAnimationCount;
            _hitExplosionAnimation = new SpriteAnimation(_gameCore, Path.Combine(_assetPathHitExplosionAnimation, $"{_selectedHitExplosionAnimationIndex}.png"), new Size(22, 22));

            _lockedOnImage = _gameCore.Assets.GetBitmap(_assetPathlockedOnImage);
            _lockedOnSoftImage = _gameCore.Assets.GetBitmap(_assetPathlockedOnSoftImage);

            base.Initialize(imagePath, size);
        }

        public bool FireDroneWeapon(string weaponTypeName)
        {
            return GetWeaponByName(weaponTypeName)?.Fire() == true;
        }

        private readonly Dictionary<string, WeaponBase> _droneWeaponsCache = new();

        public WeaponBase GetWeaponByName(string weaponTypeName)
        {
            if (_droneWeaponsCache.TryGetValue(weaponTypeName, out var weapon))
            {
                return weapon;
            }

            var weaponType = SiReflection.GetTypeByName(weaponTypeName); //TODO: Probably need to speed this up.
            weapon = SiReflection.CreateInstanceFromType<WeaponBase>(weaponType, new object[] { _gameCore, this });
            weapon.IsOwnerDrone = true;

            _droneWeaponsCache.Add(weaponTypeName, weapon);

            return weapon;
        }

        public override void Explode()
        {
            _explodeSound?.Play();
            _explosionAnimation?.Reset();
            _gameCore.Sprites.Animations.InsertAt(_explosionAnimation, this);
            base.Explode();
        }

        public void CreateParticlesExplosion()
        {
            _gameCore.Sprites.Particles.CreateRandomShipPartParticlesAt(this, SiRandom.Between(30, 50));
            _gameCore.Audio.PlayRandomExplosion();
        }

        /// <summary>
        /// Allows for the testing of hits from a munition. This is called for each movement along a munitions path.
        /// </summary>
        /// <param name="displacementVector">The background offset vector.</param>
        /// <param name="munition">The munition object that is being tested for.</param>
        /// <param name="hitTestPosition">The position to test for hit.</param>
        /// <returns></returns>
        public virtual bool TryMunitionHit(SiPoint displacementVector, MunitionBase munition, SiPoint hitTestPosition)
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
            return false;
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
