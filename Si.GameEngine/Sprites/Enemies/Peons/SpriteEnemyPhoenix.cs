﻿using Si.GameEngine.AI.Logistics;
using Si.GameEngine.Core;
using Si.GameEngine.Loudouts;
using Si.GameEngine.Sprites.Enemies.Peons._Superclass;
using Si.GameEngine.Sprites.Weapons;
using Si.Library;
using Si.Library.Types.Geometry;
using System;
using System.Linq;
using static Si.Library.SiConstants;

namespace Si.GameEngine.Sprites.Enemies.Peons
{
    internal class SpriteEnemyPhoenix : SpriteEnemyPeonBase
    {
        public const int hullHealth = 10;
        public const int bountyMultiplier = 15;

        public SpriteEnemyPhoenix(GameEngineCore gameEngine)
            : base(gameEngine, hullHealth, bountyMultiplier)
        {
            ShipClass = SiEnemyClass.Phoenix;
            SetImage(@$"Graphics\Enemy\Peons\{ShipClass}\Hull.png");

            if (IsDrone)
            {
                //If this is a multiplayer drone then we need to skip most of the initilization. This is becuase
                //  the reaminder of the ctor is for adding weapons and initializing AI, none of which we need.
                return;
            }

            //Load the loadout from file or create a new one if it does not exist.
            EnemyShipLoadout loadout = LoadLoadoutFromFile(ShipClass);
            if (loadout == null)
            {
                loadout = new EnemyShipLoadout(ShipClass)
                {
                    Description = "→ Phoenix ←\n"
                       + "TODO: Add a description\n",
                    MaxSpeed = 3.5,
                    MaxBoost = 1.5,
                    HullHealth = 20,
                    ShieldHealth = 10,
                };

                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponVulcanCannon), 5000));
                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponDualVulcanCannon), 2500));
                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponFragMissile), 42));
                loadout.Weapons.Add(new ShipLoadoutWeapon(typeof(WeaponThunderstrikeMissile), 16));

                SaveLoadoutToFile(loadout);
            }

            ResetLoadout(loadout);

            AddAIController(new HostileEngagement(_gameEngine, this, _gameEngine.Player.Sprite));
            AddAIController(new Taunt(_gameEngine, this, _gameEngine.Player.Sprite));
            //AddAIController(new Meander(_gameEngine, this, _gameEngine.Player.Sprite));

            SetCurrentAIController(AIControllers[typeof(Taunt)]);

            _behaviorChangeThresholdMiliseconds = SiRandom.Between(2000, 10000);
        }

        #region Artificial Intelligence.

        private DateTime _lastBehaviorChangeTime = DateTime.UtcNow;
        private double _behaviorChangeThresholdMiliseconds = 0;

        public override void ApplyIntelligence(SiPoint displacementVector)
        {
            if (IsDrone)
            {
                //If this is a multiplayer drone then we need to skip most of the initilization. This is becuase
                //  the reaminder of the ctor is for adding weapons and initializing AI, none of which we need.
                return;
            }

            base.ApplyIntelligence(displacementVector);

            if ((DateTime.UtcNow - _lastBehaviorChangeTime).TotalMilliseconds > _behaviorChangeThresholdMiliseconds)
            {
                _lastBehaviorChangeTime = DateTime.UtcNow;
                _behaviorChangeThresholdMiliseconds = SiRandom.Between(2000, 10000);

                /*
                if (SiRandom.PercentChance(1))
                {
                    SetCurrentAIController(AIControllers[typeof(Taunt)]);
                }
                */

                if (SiRandom.PercentChance(5))
                {
                    SetCurrentAIController(AIControllers[typeof(HostileEngagement)]);
                }
            }

            if (IsHostile)
            {
                var playersIAmPointingAt = GetPointingAtOf(_gameEngine.Sprites.AllVisiblePlayers, 2.0);
                if (playersIAmPointingAt.Any())
                {
                    var closestDistance = ClosestDistanceOf(playersIAmPointingAt);

                    if (closestDistance < 1000)
                    {
                        if (closestDistance > 500 && HasWeaponAndAmmo<WeaponVulcanCannon>())
                        {
                            FireWeapon<WeaponVulcanCannon>();
                        }
                        else if (closestDistance > 0 && HasWeaponAndAmmo<WeaponDualVulcanCannon>())
                        {
                            FireWeapon<WeaponDualVulcanCannon>();
                        }
                    }
                }
            }

            CurrentAIController?.ApplyIntelligence(displacementVector);
        }

        #endregion
    }
}
