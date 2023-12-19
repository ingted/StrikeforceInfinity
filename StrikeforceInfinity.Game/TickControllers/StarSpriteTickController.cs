﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types.Geometry;
using StrikeforceInfinity.Game.Managers;
using StrikeforceInfinity.Game.Sprites;
using StrikeforceInfinity.Game.TickControllers.BaseClasses;
using StrikeforceInfinity.Game.Utility;

namespace StrikeforceInfinity.Game.Controller
{
    internal class StarSpriteTickController : SpriteTickControllerBase<SpriteStar>
    {
        public StarSpriteTickController(EngineCore core, EngineSpriteManager manager)
            : base(core, manager)
        {
        }

        public override void ExecuteWorldClockTick(SiPoint displacementVector)
        {
            if (displacementVector.X != 0 || displacementVector.Y != 0)
            {
                #region Add new stars...

                if (SpriteManager.VisibleOfType<SpriteStar>().Count < Core.Settings.DeltaFrameTargetStarCount) //Never wan't more than n stars.
                {
                    if (displacementVector.X > 0)
                    {
                        if (HgRandom.PercentChance(20))
                        {
                            int x = HgRandom.Generator.Next(Core.Display.TotalCanvasSize.Width - (int)displacementVector.X, Core.Display.TotalCanvasSize.Width);
                            int y = HgRandom.Generator.Next(0, Core.Display.TotalCanvasSize.Height);
                            SpriteManager.Stars.Create(x, y);
                        }

                    }
                    else if (displacementVector.X < 0)
                    {
                        if (HgRandom.PercentChance(20))
                        {
                            int x = HgRandom.Generator.Next(0, (int)-displacementVector.X);
                            int y = HgRandom.Generator.Next(0, Core.Display.TotalCanvasSize.Height);
                            SpriteManager.Stars.Create(x, y);
                        }

                    }
                    if (displacementVector.Y > 0)
                    {
                        if (HgRandom.PercentChance(20))
                        {
                            int x = HgRandom.Generator.Next(0, Core.Display.TotalCanvasSize.Width);
                            int y = HgRandom.Generator.Next(Core.Display.TotalCanvasSize.Height - (int)displacementVector.Y, Core.Display.TotalCanvasSize.Height);
                            SpriteManager.Stars.Create(x, y);
                        }

                    }
                    else if (displacementVector.Y < 0)
                    {

                        if (HgRandom.PercentChance(20))
                        {
                            int x = HgRandom.Generator.Next(0, Core.Display.TotalCanvasSize.Width);
                            int y = HgRandom.Generator.Next(0, (int)-displacementVector.Y);
                            SpriteManager.Stars.Create(x, y);
                        }

                    }
                }

                #endregion

                foreach (var star in All())
                {
                    star.ApplyMotion(displacementVector);

                    if (Core.Display.TotalCanvasBounds.IntersectsWith(star.Bounds) == false) //Remove off-screen stars.
                    {
                        star.QueueForDelete();
                    }
                }
            }
        }
    }
}