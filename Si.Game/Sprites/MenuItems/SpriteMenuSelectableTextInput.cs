﻿using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Si.Game.Engine;
using Si.Game.Engine.Types;
using Si.Game.Menus.BasesAndInterfaces;
using static Si.Shared.SiConstants;
using Si.Shared.Types.Geometry;
using Si.Shared.Types;

namespace Si.Game.Sprites.MenuItems
{
    /// <summary>
    /// Menu item that accepts user text input.
    /// </summary>
    internal class SpriteMenuSelectableTextInput : SpriteMenuItem
    {
        public int CharacterLimit { get; set; }

        public SpriteMenuSelectableTextInput(EngineCore gameCore, MenuBase menu, TextFormat format, SolidColorBrush color, SiPoint location, int characterLimit = 100)
            : base(gameCore, menu, format, color, location)
        {
            ItemType = SiMenuItemType.SelectableTextInput;
            Visable = true;
            Velocity = new SiVelocity();
            CharacterLimit = characterLimit;
        }

        public void Backspace()
        {
            if (Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
        }

        public void Append(string text)
        {
            var totalString = Text + text;

            if (totalString.Length > CharacterLimit)
            {
                Text = totalString.Substring(0, CharacterLimit);
            }
            else
            {
                Text = totalString;
            }
        }

    }
}
