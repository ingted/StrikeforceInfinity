﻿using AI2D.AI;
using AI2D.Engine;
using AI2D.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AI2D.Actors.Objects.Enemies
{
    internal class EnemyBase : ActorBase
    {
        public IAIController CurrentAIController { get; set; }
        public Dictionary<Type, IAIController> AIControllers { get; private set; } = new();
        public int CollisionDamage { get; set; } = 25;
        public int ScorePoints { get; private set; } = 25;
        public ActorRadarPositionIndicator RadarPositionIndicator { get; set; }
        public ActorRadarPositionTextBlock RadarPositionText { get; set; }
        public ActorAnimation ThrustAnimation { get; private set; }

        public EnemyBase(Core core, int hitPoints, int scoreMultiplier)
            : base(core)
        {
            Velocity.ThrottlePercentage = 1;
            Initialize();

            RadarDotSize = new Point<int>(4, 4);
            RadarDotColor = Color.FromArgb(200, 100, 100);

            SetHitPoints(hitPoints);
            ScorePoints = HitPoints * scoreMultiplier;

            RadarPositionIndicator = _core.Actors.RadarPositions.Create();
            RadarPositionIndicator.Visable = false;
            RadarPositionText = _core.Actors.TextBlocks.CreateRadarPosition("Consolas", Brushes.Red, 8, new Point<double>());

            var playMode = new ActorAnimation.PlayMode()
            {
                Replay = ActorAnimation.ReplayMode.LoopedPlay,
                DeleteActorAfterPlay = false,
                ReplayDelay = new TimeSpan(0)
            };
            ThrustAnimation = new ActorAnimation(_core, @"..\..\..\Assets\Graphics\Animation\AirThrust32x32.png", new Size(32, 32), 10, playMode);

            ThrustAnimation.Reset();
            _core.Actors.Animations.CreateAt(ThrustAnimation, this);

            var pointRight = Utility.AngleFromPointAtDistance(Velocity.Angle + 180, new Point<double>(20, 20));
            ThrustAnimation.Velocity.Angle.Degrees = Velocity.Angle.Degrees - 180;
            ThrustAnimation.X = X + pointRight.X;
            ThrustAnimation.Y = Y + pointRight.Y;
        }

        public virtual void BeforeCreate()
        {
        }
        public virtual void AfterCreate()
        {
        }

        public override void RotationChanged()
        {
            PositionChanged();
        }

        public override void PositionChanged()
        {
            if (ThrustAnimation != null && ThrustAnimation.Visable)
            {
                var pointRight = Utility.AngleFromPointAtDistance(Velocity.Angle + 180, new Point<double>(20, 20));
                ThrustAnimation.Velocity.Angle.Degrees = Velocity.Angle.Degrees - 180;
                ThrustAnimation.X = X + pointRight.X;
                ThrustAnimation.Y = Y + pointRight.Y;
            }
        }

        public static int GetGenericHP()
        {
            return Utility.Random.Next(Constants.Limits.MinEnemyHealth, Constants.Limits.MaxEnemyHealth);
        }

        public virtual void ApplyMotion(Point<double> frameAppliedOffset)
        {
            if (X < -Constants.Limits.EnemySceneDistanceLimit
                || X >= _core.Display.NatrualScreenSize.Width + Constants.Limits.EnemySceneDistanceLimit
                || Y < -Constants.Limits.EnemySceneDistanceLimit
                || Y >= _core.Display.NatrualScreenSize.Height + Constants.Limits.EnemySceneDistanceLimit)
            {
                QueueForDelete();
                return;
            }

            X += Velocity.Angle.X * (Velocity.MaxSpeed * Velocity.ThrottlePercentage) - frameAppliedOffset.X;
            Y += Velocity.Angle.Y * (Velocity.MaxSpeed * Velocity.ThrottlePercentage) - frameAppliedOffset.Y;

            if (RadarPositionIndicator != null)
            {
                if(_core.Display.CurrentScaledScreenBounds.IntersectsWith(this.Bounds, -50) == false)
                {
                    RadarPositionText.DistanceValue = Math.Abs(DistanceTo(_core.Actors.Player));

                    RadarPositionText.Visable = true;
                    RadarPositionIndicator.Visable = true;

                    double requiredAngle = _core.Actors.Player.AngleTo(this);

                    var offset = Utility.AngleFromPointAtDistance(new Angle<double>(requiredAngle), new Point<double>(200, 200));

                    RadarPositionText.Location = _core.Actors.Player.Location + offset + new Point<double>(25, 25);
                    RadarPositionIndicator.Velocity.Angle.Degrees = requiredAngle;

                    RadarPositionIndicator.Location = _core.Actors.Player.Location + offset;
                    RadarPositionIndicator.Velocity.Angle.Degrees = requiredAngle;
                }
                else
                {
                    RadarPositionText.Visable = false;
                    RadarPositionIndicator.Visable = false;
                }
            }
        }

        public virtual void ApplyIntelligence(Point<double> frameAppliedOffset)
        {
            if (SelectedSecondaryWeapon != null && _core.Actors.Player != null)
            {
                SelectedSecondaryWeapon.ApplyIntelligence(frameAppliedOffset, _core.Actors.Player); //Enemy lock-on to Player. :O
            }
        }

        public override void Cleanup()
        {
            if (RadarPositionIndicator != null)
            {
                RadarPositionIndicator.QueueForDelete();
                RadarPositionText.QueueForDelete();
            }
            if (ThrustAnimation != null)
            {
                ThrustAnimation.QueueForDelete();
            }
            base.Cleanup();
        }

        internal void AddAIController(IAIController controller)
        {
            AIControllers.Add(controller.GetType(), controller);
        }

        internal void SetCurrentAIController(IAIController value)
        {
            CurrentAIController = value;
        }
    }
}
