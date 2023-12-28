﻿using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Si.GameEngine.Engine;
using Si.GameEngine.Managers;
using Si.GameEngine.Sprites;
using Si.GameEngine.TickControllers.BasesAndInterfaces;
using Si.Shared.Types.Geometry;
using System.Linq;

namespace Si.GameEngine.Controller
{
    public class TextBlocksSpriteTickController : SpriteTickControllerBase<SpriteTextBlock>
    {
        public TextBlocksSpriteTickController(EngineCore gameCore, EngineSpriteManager manager)
            : base(gameCore, manager)
        {
        }

        public override void ExecuteWorldClockTick(SiPoint displacementVector)
        {
            foreach (var textBlock in Visible().Where(o => o.IsFixedPosition == false))
            {
                textBlock.ApplyMotion(displacementVector);
            }
        }

        #region Factories.

        public SpriteRadarPositionTextBlock CreateRadarPosition(TextFormat format, SolidColorBrush color, SiPoint location)
        {
            var obj = new SpriteRadarPositionTextBlock(GameCore, format, color, location);
            SpriteManager.Add(obj);
            return obj;
        }

        public SpriteTextBlock Create(TextFormat format, SolidColorBrush color, SiPoint location, bool isPositionStatic)
        {
            var obj = new SpriteTextBlock(GameCore, format, color, location, isPositionStatic);
            SpriteManager.Add(obj);
            return obj;
        }

        public SpriteTextBlock Create(TextFormat format, SolidColorBrush color, SiPoint location, bool isPositionStatic, string name)
        {
            var obj = new SpriteTextBlock(GameCore, format, color, location, isPositionStatic);
            obj.SpriteTag = name;
            SpriteManager.Add(obj);
            return obj;
        }

        #endregion
    }
}