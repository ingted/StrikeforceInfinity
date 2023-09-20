﻿using NebulaSiege.Engine;
using NebulaSiege.Engine.Types.Geometry;
using NebulaSiege.Managers;
using NebulaSiege.Sprites;
using NebulaSiege.TickControllers;

namespace NebulaSiege.Controller
{
    internal class DebugSpriteTickController : _SpriteTickControllerBase<SpriteDebug>
    {
        public DebugSpriteTickController(EngineCore core, EngineSpriteManager manager)
            : base(core, manager)
        {
        }

        public override void ExecuteWorldClockTick(NsPoint displacementVector)
        {
            /*
            if (Core.Player.Sprite != null)
            {
                var anchor = Core.Sprites.Debugs.ByAssetTag("Anchor");
                if (anchor == null)
                {
                    Core.Sprites.Debugs.CreateAtCenterScreen("Anchor");
                    anchor = Core.Sprites.Debugs.ByAssetTag("Anchor");
                }

                var pointer = Core.Sprites.Debugs.ByAssetTag("Pointer");
                if (pointer == null)
                {
                    Core.Sprites.Debugs.CreateAtCenterScreen("Pointer");
                    pointer = Core.Sprites.Debugs.ByAssetTag("Pointer");
                }

                double requiredAngle = Core.Player.Sprite.AngleTo(anchor);
                var offset = HgMath.AngleFromPointAtDistance(new HgAngle(requiredAngle), new HgPoint(200, 200));
                pointer.Velocity.Angle.Degrees = requiredAngle;
                pointer.Location = Core.Player.Sprite.Location + offset;
                anchor.Velocity.Angle.Degrees = anchor.AngleTo(Core.Player.Sprite);
            }
            */

            foreach (var debug in Visible())
            {
                debug.ApplyMotion(displacementVector);
                debug.RenewableResources.RenewAllResources();
            }
        }
    }
}