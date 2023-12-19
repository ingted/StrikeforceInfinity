﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using StrikeforceInfinity.Game.Managers;
using StrikeforceInfinity.Game.Sprites.PowerUp.BaseClasses;
using StrikeforceInfinity.Game.TickControllers.BaseClasses;
using System;

namespace StrikeforceInfinity.Game.Controller
{
    internal class PowerupSpriteTickController : SpriteTickControllerBase<SpritePowerUpBase>
    {
        public PowerupSpriteTickController(EngineCore core, EngineSpriteManager manager)
            : base(core, manager)
        {
        }

        public override void ExecuteWorldClockTick(SiPoint displacementVector)
        {
            foreach (var sprite in Visible())
            {
                sprite.ApplyIntelligence(displacementVector);
                sprite.ApplyMotion(displacementVector);
            }
        }

        public T Create<T>(double x, double y) where T : SpritePowerUpBase
        {
            lock (SpriteManager.Collection)
            {
                object[] param = { Core };
                var obj = (SpritePowerUpBase)Activator.CreateInstance(typeof(T), param);
                obj.Location = new SiPoint(x, y);
                SpriteManager.Collection.Add(obj);
                return (T)obj;
            }
        }
    }
}