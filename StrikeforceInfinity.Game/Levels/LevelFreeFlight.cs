﻿using StrikeforceInfinity.Game.Engine;
using StrikeforceInfinity.Game.Engine.Types;
using StrikeforceInfinity.Game.Levels.BaseClasses;
using System.Collections.Generic;

namespace StrikeforceInfinity.Game.Levels
{
    /// <summary>
    /// Levels are contained inside Situations. Each level contains a set of waves that are progressed. 
    /// This level is just a peaceful free flight.
    /// </summary>
    internal class LevelFreeFlight : LevelBase
    {
        public LevelFreeFlight(EngineCore core)
            : base(core,
                  "Free Flight",
                  "Theres nothing in this quadrant or the next that will threaten us.")
        {
            TotalWaves = 5;
        }

        readonly List<SiEngineCallbackEvent> events = new List<SiEngineCallbackEvent>();

        public override void Begin()
        {
            base.Begin();

            AddSingleFireEvent(new System.TimeSpan(0, 0, 0, 0, 500), FirstShowPlayerCallback);
        }

        private void FirstShowPlayerCallback(EngineCore core, SiEngineCallbackEvent sender, object refObj)
        {
            _core.Player.ResetAndShow();
        }
    }
}