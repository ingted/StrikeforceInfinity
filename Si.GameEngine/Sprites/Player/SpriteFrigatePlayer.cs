﻿using Si.GameEngine.Core;
using Si.GameEngine.Loudouts;
using Si.GameEngine.Sprites.Player._Superclass;
using Si.GameEngine.Sprites.Weapons;
using static Si.Library.SiConstants;

namespace Si.GameEngine.Sprites.Player
{
    internal class SpriteFrigatePlayer : SpritePlayerBase
    {
        public SpriteFrigatePlayer(GameEngineCore gameEngine)
            : base(gameEngine)
        {
            ShipClass = SiPlayerClass.Frigate;

            string imagePath = @$"Graphics\Player\Ships\{ShipClass}.png";
            Initialize(imagePath);

            //Load the loadout from file or create a new one if it does not exist.
            PlayerShipLoadout loadout = LoadLoadoutFromFile(ShipClass);
            if (loadout == null)
            {
                loadout = new PlayerShipLoadout(ShipClass)
                {
                    Description = "→ Nimble Interceptor ←\n"
                        + "A nimble interceptor, designed for hit-and-run tactics\n"
                        + "and lightning-fast strikes against enemy forces.",
                    MaxSpeed = 4.5,
                    MaxBoost = 1.5,
                    HullHealth = 500,
                    ShieldHealth = 100,
                    PrimaryWeapon = new ShipLoadoutWeapon(typeof(WeaponScattershot), 10000)
                };

                loadout.SecondaryWeapons.Add(new ShipLoadoutWeapon(typeof(WeaponFragMissile), 42));
                loadout.SecondaryWeapons.Add(new ShipLoadoutWeapon(typeof(WeaponThunderstrikeMissile), 16));

                SaveLoadoutToFile(loadout);
            }

            ResetLoadout(loadout);
        }
    }
}
