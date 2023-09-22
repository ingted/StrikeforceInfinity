﻿using NebulaSiege.Engine.Types.Geometry;
using System.Diagnostics;
using System.Threading;

namespace NebulaSiege.Engine
{
    /// <summary>
    /// The world clock. Moves all objects forward in time, renders all objects and keeps the frame-counter in check.
    /// </summary>
    internal class EngineWorldClock
    {
        private readonly EngineCore _core;
        private bool _shutdown = false;
        private bool _pause = false;
        private readonly Thread _graphicsThread;

        public EngineWorldClock(EngineCore core)
        {
            _core = core;
            _graphicsThread = new Thread(GraphicsThreadProc);
        }

        #region Start / Stop / Pause.

        public void Start()
        {
            _shutdown = false;
            _graphicsThread.Start();
        }

        public void Stop()
        {
            _shutdown = true;
            _graphicsThread.Join();
        }

        public bool IsPaused() => _pause;

        public void TogglePause()
        {
            _pause = !_pause;
        }

        public void Pause()
        {
            _pause = true;
        }

        public void Resume()
        {
            _pause = false;
        }

        #endregion

        private void GraphicsThreadProc()
        {
            #region Add initial stars.

            for (int i = 0; i < _core.Settings.InitialFrameStarCount; i++)
            {
                _core.Sprites.Stars.Create();
            }

            #endregion

            var timer = new Stopwatch();
            var targetFrameDuration = 1000000 / _core.Settings.FrameLimiter; //1000000 / n-frames/second.

            while (_shutdown == false)
            {
                timer.Restart();

                _core.Display.GameLoopCounter.Calculate();

                lock (_core.Menus.Collection)
                    lock (_core.Player.Sprite)
                        lock (_core.Sprites.Collection)
                        {
                            if (_pause == false)
                            {
                                BeforeExecuteWorldClockTick();
                                var displacementVector = ExecuteWorldClockTick();
                                AfterExecuteWorldClockTick(displacementVector);
                            }

                            _core.Debug.ProcessCommand();

                            _core.Render();
                            timer.Stop();
                        }

                var clockTime = (((double)timer.ElapsedTicks) / Stopwatch.Frequency) * 1000000;
                var deltaClockTime = targetFrameDuration - clockTime;
                timer.Restart();

                while (((double)timer.ElapsedTicks) / Stopwatch.Frequency * 1000000 < deltaClockTime)
                {
                    Thread.Yield();
                }

                if (_pause)
                {
                    Thread.Yield();
                }
            }
        }

        private NsPoint ExecuteWorldClockTick()
        {
            _core.Menus.ExecuteWorldClockTick();
            _core.Situations.ExecuteWorldClockTick();
            _core.Events.ExecuteWorldClockTick();

            var displacementVector = _core.Player.ExecuteWorldClockTick();

            _core.Sprites.Enemies.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Particles.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Munitions.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Stars.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Animations.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.TextBlocks.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Powerups.ExecuteWorldClockTick(displacementVector);
            _core.Sprites.Debugs.ExecuteWorldClockTick(displacementVector);

            _core.Sprites.RadarPositions.ExecuteWorldClockTick();

            _core.Sprites.CleanupDeletedObjects();

            return displacementVector;
        }

        private void BeforeExecuteWorldClockTick()
        {
        }

        private void AfterExecuteWorldClockTick(NsPoint displacementVector)
        {
            if (_core.Player.Sprite.Visable == false)
            {
                _core.Player.Sprite.ShipEngineIdleSound.Stop();
                _core.Player.Sprite.ShipEngineRoarSound.Stop();
            }

            string situation = "<peaceful>";

            if (_core.Situations.CurrentSituation != null)
            {
                situation = $"{_core.Situations.CurrentSituation.Name} (Wave {_core.Situations.CurrentSituation.CurrentWave} of {_core.Situations.CurrentSituation.TotalWaves})";
            }

            double boostRebuildPercent = (_core.Player.Sprite.Velocity.AvailableBoost / _core.Settings.PlayerBoostRebuildFloor) * 100.0;

            _core.Sprites.PlayerStatsText.Text =
                  $" Situation: {situation}\r\n"
                + $"      Hull: {_core.Player.Sprite.HullHealth:n0} (Shields: {_core.Player.Sprite.ShieldHealth:n0}) | Bounty: ${_core.Player.Sprite.Bounty}\r\n"
                + $"      Warp: {((_core.Player.Sprite.Velocity.AvailableBoost / _core.Settings.MaxPlayerBoostAmount) * 100.0):n1}%"
                    + (_core.Player.Sprite.Velocity.BoostRebuilding ? $" (RECHARGING: {boostRebuildPercent:n1}%)" : string.Empty) + "\r\n"
                + $"Pri-Weapon: {_core.Player.Sprite.PrimaryWeapon?.Name} x{_core.Player.Sprite.PrimaryWeapon?.RoundQuantity:n0}\r\n"
                + $"Sec-Weapon: {_core.Player.Sprite.SelectedSecondaryWeapon?.Name} x{_core.Player.Sprite.SelectedSecondaryWeapon?.RoundQuantity:n0}\r\n";

            //_core.Sprites.DebugText.Text = "Anything we need to know about?";
        }
    }
}
