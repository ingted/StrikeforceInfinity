﻿using Si.Shared;
using Si.Game.Engine;
using Si.Shared.Types.Geometry;
using Si.Game.Menus.BasesAndInterfaces;
using Si.Game.Situations.BasesAndInterfaces;
using Si.Game.Sprites.MenuItems;
using Si.Game.Utility;
using Si.Menus.SinglePlayer;
using System.Linq;

namespace Si.Menus.MultiPlayer.Host
{
    /// <summary>
    /// The menu that is displayed at game start to allow the player to select a situation.
    /// </summary>
    internal class MpMenuHostSituationSelect : MenuBase
    {
        private readonly SpriteMenuItem _situationBlurb;

        public MpMenuHostSituationSelect(EngineCore gameCore)
            : base(gameCore)
        {
            var currentScaledScreenBounds = _gameCore.Display.GetCurrentScaledScreenBounds();

            double offsetX = currentScaledScreenBounds.X + 40;
            double offsetY = currentScaledScreenBounds.Y + 100;

            var itemTitle = CreateAndAddTitleItem(new SiPoint(offsetX, offsetY), "Whats the Situation?");
            itemTitle.X = offsetX + 200;
            itemTitle.Y = offsetY - itemTitle.Size.Height;

            offsetY += itemTitle.Height;

            _situationBlurb = CreateAndAddTextblock(new SiPoint(offsetX, offsetY), "");
            _situationBlurb.X = offsetX + 300;
            _situationBlurb.Y = offsetY - _situationBlurb.Size.Height;

            //Use reflection to get a list of possible player types.
            var situationTypes = SiReflection.GetSubClassesOf<SituationBase>().OrderBy(o => o.Name).ToList();

            //Move the debug player to the top of the list.
            var situations = situationTypes.Where(o => o.Name.Contains("Debug")).FirstOrDefault();
            if (situations != null)
            {
                situationTypes.Remove(situations);
                situationTypes.Insert(0, situations);
            }

            foreach (var situationType in situationTypes)
            {
                var situationInstance = SiReflection.CreateInstanceFromType<SituationBase>(situationType, new object[] { gameCore, });

                var menuItem = CreateAndAddSelectableItem(new SiPoint(offsetX + 25, offsetY), situationInstance.Name, $"> {situationInstance.Name}");

                menuItem.UserData = situationInstance;

                menuItem.Y -= menuItem.Size.Height / 2;
                offsetY += 50;
            }

            OnSelectionChanged += SituationSelectMenu_OnSelectionChanged;
            OnExecuteSelection += SituationSelectMenu_OnExecuteSelection;
            OnEscape += MpMenuHostSituationSelect_OnEscape;

            VisibleSelectableItems().First().Selected = true;
        }

        private bool MpMenuHostSituationSelect_OnEscape()
        {
            //Create the game host on the server.
            var lobbyUID = _gameCore.Multiplay.State.LobbyUID;
            _gameCore.Multiplay.DeregisterLobbyUID();
            _gameCore.Multiplay.DeleteLobby(lobbyUID);

            _gameCore.Menus.Insert(new MpMenuHostCreateLobby(_gameCore));
            return true;
        }

        private bool SituationSelectMenu_OnExecuteSelection(SpriteMenuItem item)
        {
            if (item.UserData is SituationBase situation)
            {
                _gameCore.ResetGame();
                _gameCore.Situations.Select(situation.GetType().Name);
                _gameCore.Menus.Insert(new MpMenuHostSelectLoadout(_gameCore));
            }
            return true;
        }

        private void SituationSelectMenu_OnSelectionChanged(SpriteMenuItem item)
        {
            if (item.UserData is SituationBase situation)
            {
                _situationBlurb.Text = situation.Description;
            }
        }
    }
}
