﻿using AI2D.Engine;
using AI2D.Types;
using AI2D.Weapons;

namespace AI2D.GraphicObjects.Bullets
{
    public class BulletVulcanCannon : BaseBullet
    {
        private const string imagePath = @"..\..\Assets\Graphics\Weapon\Vulcan Cannon.png";

        public BulletVulcanCannon(Core core, WeaponBase weapon, BaseGraphicObject firedFrom,
             BaseGraphicObject lockedTarget = null, PointD xyOffset = null)
            : base(core, weapon, firedFrom, imagePath, lockedTarget, xyOffset)
        {
            Initialize(imagePath);
        }
    }
}
