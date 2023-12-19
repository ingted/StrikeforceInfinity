﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using StrikeforceInfinity.Game.Sprites;
using StrikeforceInfinity.Game.Weapons.BaseClasses;

namespace StrikeforceInfinity.Game.Weapons.Munitions
{
    internal class MunitionPulseMeson : EnergyMunitionBase
    {
        private const string imagePath = @"Graphics\Weapon\PulseMeson.png";

        public MunitionPulseMeson(EngineCore core, WeaponBase weapon, SpriteBase firedFrom, SiPoint xyOffset = null)
            : base(core, weapon, firedFrom, imagePath, xyOffset)
        {
            Initialize(imagePath);
        }
    }
}