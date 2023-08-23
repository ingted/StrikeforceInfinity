﻿using HG.Actors.Objects;
using HG.Actors.Objects.Enemies;
using HG.Actors.Objects.PowerUp;
using HG.Actors.Objects.Weapons.Bullets;
using HG.Engine.Situations;
using HG.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HG.Engine
{
    internal class GameLoop
    {
        private readonly Core _core;
        private bool _shutdown = false;
        private bool _pause = false;
        private readonly Thread _graphicsThread;

        public GameLoop(Core core)
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
        }

        public bool IsPaused()
        {
            return _pause;
        }

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

            for (int i = 0; i < 60; i++)
            {
                _core.Actors.Stars.Create();
            }

            #endregion

            var timer = new Stopwatch();
            var targetFrameDuration = 1000000 / _core.Settings.FrameLimiter; //1000000 / n-frames/second.

            while (_shutdown == false)
            {
                timer.Restart();

                _core.Display.GameLoopCounter.Calculate();

                if (_pause == false)
                {
                    Monitor.Enter(_core.DrawingSemaphore);

                    lock (_core.Actors.Menus.Collection)
                        lock (_core.Actors.Player)
                            lock (_core.Actors.Collection)
                            {
                                AdvanceWorldClock();
                                timer.Stop();

                            }

                    Monitor.Exit(_core.DrawingSemaphore);
                }

                if (_core.Actors.Menus.Collection.Count > 0)
                {
                    Thread.Sleep(20);
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
                    _core.Display.DrawingSurface.Invalidate();
                    Thread.Sleep(5);
                }
            }
        }

        void AdvanceWorldClock()
        {
            #region Menu Management.

            for (int i = 0; i < _core.Actors.Menus.Collection.Count; i++)
            {
                var menu = _core.Actors.Menus.Collection[i];
                menu.HandleInput();
            }

            #endregion

            #region Situation Advancement.

            if (_core.Situations.CurrentSituation?.State == BaseSituation.ScenarioState.Ended)
            {
                if (_core.Situations.AdvanceSituation() == false)
                {
                    _core.Events.QueueTheDoorIsAjar();
                }
            }

            #endregion

            #region Player Frame Advancement.

            var appliedOffset = new HGPoint<double>();

            if (_core.Actors.Player.Visable)
            {
                if (_core.Input.IsKeyPressed(HGPlayerKey.PrimaryFire))
                {
                    if (_core.Actors.Player.SelectedPrimaryWeapon != null && _core.Actors.Player.SelectedPrimaryWeapon.Fire())
                    {
                        if (_core.Actors.Player.SelectedPrimaryWeapon?.RoundQuantity == 25)
                        {
                            _core.Actors.Player.AmmoLowSound.Play();
                        }
                        if (_core.Actors.Player.SelectedPrimaryWeapon?.RoundQuantity == 0)
                        {
                            _core.Actors.Player.AmmoEmptySound.Play();
                            _core.Actors.Player.SelectFirstAvailableUsablePrimaryWeapon();
                        }
                    }
                }

                if (_core.Input.IsKeyPressed(HGPlayerKey.SecondaryFire))
                {
                    if (_core.Actors.Player.SelectedSecondaryWeapon != null && _core.Actors.Player.SelectedSecondaryWeapon.Fire())
                    {
                        if (_core.Actors.Player.SelectedSecondaryWeapon?.RoundQuantity == 25)
                        {
                            _core.Actors.Player.AmmoLowSound.Play();
                        }
                        if (_core.Actors.Player.SelectedSecondaryWeapon?.RoundQuantity == 0)
                        {
                            _core.Actors.Player.AmmoEmptySound.Play();
                            _core.Actors.Player.SelectFirstAvailableUsableSecondaryWeapon();
                        }
                    }
                }

                //Make player boost "build up" and fade-in.
                if (_core.Input.IsKeyPressed(HGPlayerKey.SpeedBoost) && _core.Input.IsKeyPressed(HGPlayerKey.Forward)
                    && _core.Actors.Player.Velocity.AvailableBoost > 0 && _core.Actors.Player.IsBoostFading == false)
                {
                    if (_core.Actors.Player.Velocity.BoostPercentage < 1.0)
                    {
                        _core.Actors.Player.Velocity.BoostPercentage += _core.Settings.PlayerThrustRampUp;
                    }

                    _core.Actors.Player.Velocity.AvailableBoost -= _core.Actors.Player.Velocity.MaxBoost * _core.Actors.Player.Velocity.BoostPercentage;
                    if (_core.Actors.Player.Velocity.AvailableBoost < 0)
                    {
                        _core.Actors.Player.Velocity.AvailableBoost = 0;
                    }
                }
                else
                {
                    if (_core.Actors.Player.Velocity.AvailableBoost == 0)
                    {
                        //The boost was all used up, now we have to wait on it to cool down.
                        _core.Actors.Player.IsBoostFading = true;
                    }

                    //If no "forward" or "reverse" user input is received... then fade the boost and rebuild available boost.
                    if (_core.Actors.Player.Velocity.BoostPercentage > _core.Settings.MinPlayerThrust)
                    {
                        _core.Actors.Player.Velocity.BoostPercentage -= _core.Settings.PlayerThrustRampDown;
                        if (_core.Actors.Player.Velocity.BoostPercentage < 0)
                        {
                            _core.Actors.Player.Velocity.BoostPercentage = 0;
                        }
                    }

                    if (_core.Actors.Player.Velocity.AvailableBoost < _core.Settings.MaxPlayerBoost)
                    {
                        _core.Actors.Player.Velocity.AvailableBoost += 1 - _core.Actors.Player.Velocity.BoostPercentage;
                    }
                    else
                    {
                        _core.Actors.Player.IsBoostFading = false;
                    }
                }

                if (_core.Actors.Player.BoostAnimation != null)
                {
                    _core.Actors.Player.BoostAnimation.Visable = (_core.Input.IsKeyPressed(HGPlayerKey.SpeedBoost)
                        && _core.Input.IsKeyPressed(HGPlayerKey.Forward)
                        && _core.Actors.Player.Velocity.AvailableBoost > 0
                        && _core.Actors.Player.IsBoostFading == false);
                }

                if (_core.Actors.Player.ThrustAnimation != null)
                {
                    _core.Actors.Player.ThrustAnimation.Visable = _core.Actors.Player.Velocity.ThrottlePercentage > 0;
                }

                //Make player thrust "build up" and fade-in.
                if (_core.Input.IsKeyPressed(HGPlayerKey.Forward))
                {
                    if (_core.Actors.Player.Velocity.ThrottlePercentage < 1.0)
                    {
                        _core.Actors.Player.Velocity.ThrottlePercentage += _core.Settings.PlayerThrustRampUp;
                    }
                }
                else
                {
                    //If no "forward" or "reverse" user input is received... then fade the thrust.
                    if (_core.Actors.Player.Velocity.ThrottlePercentage > _core.Settings.MinPlayerThrust)
                    {
                        _core.Actors.Player.Velocity.ThrottlePercentage -= _core.Settings.PlayerThrustRampDown;
                        if (_core.Actors.Player.Velocity.ThrottlePercentage < 0)
                        {
                            _core.Actors.Player.Velocity.ThrottlePercentage = 0;
                        }
                    }
                }

                if (_core.Actors.Player.Velocity.ThrottlePercentage > 0)
                {
                    double forwardThrust = (_core.Actors.Player.Velocity.MaxSpeed * _core.Actors.Player.Velocity.ThrottlePercentage);

                    if (_core.Actors.Player.Velocity.BoostPercentage > 0)
                    {
                        forwardThrust += _core.Actors.Player.Velocity.MaxBoost * _core.Actors.Player.Velocity.BoostPercentage;
                    }

                    //Close to the right wall and travelling in that direction.
                    if (_core.Actors.Player.X > _core.Display.NatrualScreenBounds.X + _core.Display.NatrualScreenBounds.Width - _core.Settings.InfiniteScrollWallX
                        && _core.Actors.Player.Velocity.Angle.X > 0)
                    {
                        appliedOffset.X = (_core.Actors.Player.Velocity.Angle.X * forwardThrust);
                    }

                    //Close to the bottom wall and travelling in that direction.
                    if (_core.Actors.Player.Y > _core.Display.NatrualScreenBounds.Y + _core.Display.NatrualScreenBounds.Height - _core.Settings.InfiniteScrollWallY
                        && _core.Actors.Player.Velocity.Angle.Y > 0)
                    {
                        appliedOffset.Y = (_core.Actors.Player.Velocity.Angle.Y * forwardThrust);
                    }

                    //Close to the left wall and travelling in that direction.
                    if (_core.Actors.Player.X < _core.Display.NatrualScreenBounds.X + _core.Settings.InfiniteScrollWallX
                        && _core.Actors.Player.Velocity.Angle.X < 0)
                    {
                        appliedOffset.X = (_core.Actors.Player.Velocity.Angle.X * forwardThrust);
                    }

                    //Close to the top wall and travelling in that direction.
                    if (_core.Actors.Player.Y < _core.Display.NatrualScreenBounds.Y + _core.Settings.InfiniteScrollWallY
                        && _core.Actors.Player.Velocity.Angle.Y < 0)
                    {
                        appliedOffset.Y = (_core.Actors.Player.Velocity.Angle.Y * forwardThrust);
                    }

                    _core.Actors.Player.X += (_core.Actors.Player.Velocity.Angle.X * forwardThrust) - appliedOffset.X;
                    _core.Actors.Player.Y += (_core.Actors.Player.Velocity.Angle.Y * forwardThrust) - appliedOffset.Y;
                }

                if (_core.Actors.Player.Velocity.BoostPercentage > 0)
                {
                    _core.Actors.Player.ShipEngineBoostSound.Play();
                }
                else
                {
                    _core.Actors.Player.ShipEngineBoostSound.Fade();
                }

                if (_core.Actors.Player.Velocity.ThrottlePercentage > _core.Settings.MinPlayerThrust)
                {
                    _core.Actors.Player.ShipEngineRoarSound.Play();
                }
                else
                {
                    _core.Actors.Player.ShipEngineRoarSound.Fade();
                }

                //Scroll the background.
                _core.Display.BackgroundOffset.X += appliedOffset.X;
                _core.Display.BackgroundOffset.Y += appliedOffset.Y;

                //We are going to restrict the rotation speed to a percentage of thrust.
                var rotationSpeed = (_core.Actors.Player.Velocity.MaxRotationSpeed * _core.Actors.Player.Velocity.ThrottlePercentage);

                if (_core.Input.IsKeyPressed(HGPlayerKey.RotateCounterClockwise))
                {
                    _core.Actors.Player.Rotate(-(rotationSpeed > 1.0 ? rotationSpeed : 1.0));
                }
                else if (_core.Input.IsKeyPressed(HGPlayerKey.RotateClockwise))
                {
                    _core.Actors.Player.Rotate(rotationSpeed > 1.0 ? rotationSpeed : 1.0);
                }
            }

            _core.Display.CurrentQuadrant = _core.Display.GetQuadrant(
                _core.Actors.Player.X + _core.Display.BackgroundOffset.X,
                _core.Actors.Player.Y + _core.Display.BackgroundOffset.Y);

            #endregion

            #region Engine Event Callbacks.
            _core.Events.Execute();
            #endregion

            #region Enemies Frame Advancement.

            if (_core.Actors.Player.SelectedSecondaryWeapon != null)
            {
                _core.Actors.Player.SelectedSecondaryWeapon.LockedOnObjects.Clear();
            }

            foreach (var enemy in _core.Actors.VisibleOfType<EnemyBase>())
            {
                enemy.SelectedSecondaryWeapon?.LockedOnObjects.Clear();

                if (_core.Actors.Player.Visable)
                {
                    enemy.ApplyIntelligence(appliedOffset);

                    //Player collides with enemy.
                    if (enemy.Intersects(_core.Actors.Player))
                    {
                        if (_core.Actors.Player.Hit(enemy.CollisionDamage, true, false))
                        {
                            _core.Actors.Player.HitExplosion();
                            //enemy.Hit(enemy.CollisionDamage);
                        }
                    }

                    if (_core.Actors.Player.SelectedSecondaryWeapon != null)
                    {
                        _core.Actors.Player.SelectedSecondaryWeapon.ApplyIntelligence(appliedOffset, enemy); //Player lock-on to enemy. :D
                    }
                }

                enemy.ApplyMotion(appliedOffset);

                if (enemy.ThrustAnimation != null)
                {
                    enemy.ThrustAnimation.Visable = enemy.Velocity.ThrottlePercentage > 0;
                }
            }

            #endregion

            #region Radar Indicator Logic.

            var overlappingIndicators = new Func<List<List<ActorRadarPositionTextBlock>>>(() =>
            {
                var accountedFor = new HashSet<ActorRadarPositionTextBlock>();
                var groups = new List<List<ActorRadarPositionTextBlock>>();
                var radarTexts = _core.Actors.VisibleOfType<ActorRadarPositionTextBlock>();

                foreach (var parent in radarTexts)
                {
                    if (accountedFor.Contains(parent) == false)
                    {
                        var group = new List<ActorRadarPositionTextBlock>();
                        foreach (var child in radarTexts)
                        {
                            if (accountedFor.Contains(child) == false)
                            {
                                if (parent != child && parent.Intersects(child, new HGPoint<double>(100, 100)))
                                {
                                    group.Add(child);
                                    accountedFor.Add(child);
                                }
                            }
                        }
                        if (group.Count > 0)
                        {
                            group.Add(parent);
                            accountedFor.Add(parent);
                            groups.Add(group);
                        }
                    }
                }
                return groups;
            })();

            if (overlappingIndicators.Count > 0)
            {
                foreach (var group in overlappingIndicators)
                {
                    var min = group.Min(o => o.DistanceValue);
                    var max = group.Min(o => o.DistanceValue);

                    foreach (var member in group)
                    {
                        member.Visable = false;
                    }

                    group[0].Text = min.ToString("#,#") + "-" + max.ToString("#,#");
                    group[0].Visable = true;
                }
            }

            #endregion

            #region Bullet Frame Advancement.

            foreach (var bullet in _core.Actors.VisibleOfType<BulletBase>())
            {
                bullet.ApplyMotion(appliedOffset);

                //Check to see if the bullet hit the player:
                bullet.ApplyIntelligence(appliedOffset, _core.Actors.Player);

                //Check to see if the bullet hit an enemy.
                foreach (var enemy in _core.Actors.VisibleOfType<EnemyBase>())
                {
                    bullet.ApplyIntelligence(appliedOffset, enemy);
                }

                foreach (var enemy in _core.Actors.VisibleOfType<ActorAttachment>())
                {
                    bullet.ApplyIntelligence(appliedOffset, enemy);
                }
            }

            #endregion

            #region Stars Frame Advancement.

            if (appliedOffset.X != 0 || appliedOffset.Y != 0)
            {
                if (_core.Actors.VisibleOfType<ActorStar>().Count < 100) //Never wan't more than n stars.
                {
                    if (appliedOffset.X > 0)
                    {
                        for (int i = 0; i < 100; i++) //n chances to create a star.
                        {
                            if (HGRandom.ChanceIn(1000)) //1 in n chance to create a star.
                            {
                                int x = HGRandom.Random.Next(_core.Display.TotalCanvasSize.Width - (int)appliedOffset.X, _core.Display.TotalCanvasSize.Width);
                                int y = HGRandom.Random.Next(0, _core.Display.TotalCanvasSize.Height);
                                _core.Actors.Stars.Create(x, y);
                            }
                        }
                    }
                    else if (appliedOffset.X < 0)
                    {
                        for (int i = 0; i < 100; i++) //n chances to create a star.
                        {
                            if (HGRandom.ChanceIn(1000)) //1 in n chance to create a star.
                            {
                                int x = HGRandom.Random.Next(0, (int)-appliedOffset.X);
                                int y = HGRandom.Random.Next(0, _core.Display.TotalCanvasSize.Height);
                                _core.Actors.Stars.Create(x, y);
                            }
                        }
                    }
                    if (appliedOffset.Y > 0)
                    {
                        for (int i = 0; i < 100; i++) //n chances to create a star.
                        {
                            if (HGRandom.ChanceIn(1000)) //1 in n chance to create a star.
                            {
                                int x = HGRandom.Random.Next(0, _core.Display.TotalCanvasSize.Width);
                                int y = HGRandom.Random.Next(_core.Display.TotalCanvasSize.Height - (int)appliedOffset.Y, _core.Display.TotalCanvasSize.Height);
                                _core.Actors.Stars.Create(x, y);
                            }
                        }
                    }
                    else if (appliedOffset.Y < 0)
                    {
                        for (int i = 0; i < 100; i++) //n chances to create a star.
                        {
                            if (HGRandom.ChanceIn(1000)) //1 in n chance to create a star.
                            {
                                int x = HGRandom.Random.Next(0, _core.Display.TotalCanvasSize.Width);
                                int y = HGRandom.Random.Next(0, (int)-appliedOffset.Y);
                                _core.Actors.Stars.Create(x, y);
                            }
                        }
                    }

                }

                foreach (var star in _core.Actors.VisibleOfType<ActorStar>())
                {
                    if (_core.Display.TotalScreenBounds.IntersectsWith(star.Bounds) == false) //Remove off-screen stars.
                    {
                        star.QueueForDelete();
                    }

                    star.X -= appliedOffset.X * star.Velocity.ThrottlePercentage;
                    star.Y -= appliedOffset.Y * star.Velocity.ThrottlePercentage;
                }
            }

            #endregion

            #region Animation Frame Advancement.

            foreach (var animation in _core.Actors.VisibleOfType<ActorAnimation>())
            {
                animation.X += (animation.Velocity.Angle.X * (animation.Velocity.MaxSpeed * animation.Velocity.ThrottlePercentage)) - appliedOffset.X;
                animation.Y += (animation.Velocity.Angle.Y * (animation.Velocity.MaxSpeed * animation.Velocity.ThrottlePercentage)) - appliedOffset.Y;
                animation.AdvanceImage();
            }

            #endregion

            #region Text Block Frame Advancement.

            foreach (var textBlock in _core.Actors.VisibleOfType<ActorTextBlock>().Where(o => o.IsPositionStatic == false))
            {
                textBlock.X += (textBlock.Velocity.Angle.X * (textBlock.Velocity.MaxSpeed * textBlock.Velocity.ThrottlePercentage)) - appliedOffset.X;
                textBlock.Y += (textBlock.Velocity.Angle.Y * (textBlock.Velocity.MaxSpeed * textBlock.Velocity.ThrottlePercentage)) - appliedOffset.Y;
            }

            #endregion

            #region Power-Up Frame Advancement.

            foreach (var powerUp in _core.Actors.VisibleOfType<PowerUpBase>())
            {
                HGConversion.DynamicCast(powerUp, powerUp.GetType()).ApplyIntelligence(appliedOffset);

                powerUp.X += (powerUp.Velocity.Angle.X * (powerUp.Velocity.MaxSpeed * powerUp.Velocity.ThrottlePercentage)) - appliedOffset.X;
                powerUp.Y += (powerUp.Velocity.Angle.Y * (powerUp.Velocity.MaxSpeed * powerUp.Velocity.ThrottlePercentage)) - appliedOffset.Y;
            }

            #endregion

            if (_core.Actors.Player.Visable == false)
            {
                _core.Actors.Player.ShipEngineIdleSound.Stop();
                _core.Actors.Player.ShipEngineRoarSound.Stop();
            }

            _core.Actors.CleanupDeletedObjects();

            string situation = "<peaceful>";

            if (_core.Situations.CurrentSituation != null)
            {
                situation = $"{_core.Situations.CurrentSituation.Name} (Wave {_core.Situations.CurrentSituation.CurrentWave} of {_core.Situations.CurrentSituation.TotalWaves})";
            }

            _core.Actors.PlayerStatsText.Text =
                  $" Situation: {situation}\r\n"
                + $"      Hull: {_core.Actors.Player.HitPoints} (Shields: {_core.Actors.Player.ShieldPoints})\r\n"
                + $"     Boost: {_core.Actors.Player.Velocity.AvailableBoost.ToString("#,0")}\r\n"
                + $"Pri-Weapon: {_core.Actors.Player.SelectedPrimaryWeapon?.Name} x{_core.Actors.Player.SelectedPrimaryWeapon?.RoundQuantity}\r\n"
                + $"Sec-Weapon: {_core.Actors.Player.SelectedSecondaryWeapon?.Name} x{_core.Actors.Player.SelectedSecondaryWeapon?.RoundQuantity}\r\n";

            if (_core.ShowDebug)
            {
                _core.Actors.DebugText.Text =
                      $"       Frame Rate: Avg: {_core.Display.GameLoopCounter.AverageFrameRate.ToString("0.0")}, "
                                        + $"Min: {_core.Display.GameLoopCounter.FrameRateMin.ToString("0.0")}, "
                                        + $"Max: {_core.Display.GameLoopCounter.FrameRateMax.ToString("0.0")}\r\n"
                    + $"Player Display XY: {_core.Actors.Player.X:#0.00}x, {_core.Actors.Player.Y:#0.00}y\r\n"
                    + $"     Player Angle: {_core.Actors.Player.Velocity.Angle.X:#0.00}x, {_core.Actors.Player.Velocity.Angle.Y:#0.00}y, "
                                        + $"{_core.Actors.Player.Velocity.Angle.Degrees:#0.00}deg, "
                                        + $" {_core.Actors.Player.Velocity.Angle.Radians:#0.00}rad, "
                                        + $" {_core.Actors.Player.Velocity.Angle.RadiansUnadjusted:#0.00}rad unadjusted\r\n"
                    + $"Player Virtual XY: {_core.Actors.Player.X + _core.Display.BackgroundOffset.X:#0.00}x,"
                                        + $" {_core.Actors.Player.Y + _core.Display.BackgroundOffset.Y:#0.00}y\r\n"
                    + $"        BG Offset: {_core.Display.BackgroundOffset.X:#0.00}x, {_core.Display.BackgroundOffset.Y:#0.00}y\r\n"
                    + $"  Delta BG Offset: {appliedOffset.X:#0.00}x, {appliedOffset.Y:#0.00}y\r\n"
                    + $"           Thrust: {(_core.Actors.Player.Velocity.ThrottlePercentage * 100):#0.00}\r\n"
                    + $"            Boost: {(_core.Actors.Player.Velocity.BoostPercentage * 100):#0.00}\r\n"
                    + $"         Quadrant: {_core.Display.CurrentQuadrant.Key.X}:{_core.Display.CurrentQuadrant.Key.Y}\r\n"
                    + $"            Score: {_core.Actors.Player.Score.ToString("#,0")}";
            }
            else
            {
                if (_core.Actors.DebugText.Text != string.Empty)
                {
                    _core.Actors.DebugText.Text = string.Empty;
                }
            }
        }
    }
}