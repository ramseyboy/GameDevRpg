#region File Description

//-----------------------------------------------------------------------------
// EquipmentScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using RolePlayingGameData.Characters;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Lists the player's equipped gear, and allows the user to unequip them.
/// </summary>
internal class EquipmentScreen : ListScreen<Equipment>
{
    #region Initialization

    /// <summary>
    ///     Creates a new EquipmentScreen object for the given player.
    /// </summary>
    public EquipmentScreen(FightingCharacter fightingCharacter)
    {
        // check the parameter
        if (fightingCharacter == null)
        {
            throw new ArgumentNullException("fightingCharacter");
        }

        this.fightingCharacter = fightingCharacter;

        // sort the player's equipment
        this.fightingCharacter.EquippedEquipment.Sort(
            delegate(Equipment equipment1, Equipment equipment2)
            {
                // handle null values
                if (equipment1 == null)
                {
                    return equipment2 == null ? 0 : 1;
                }

                if (equipment2 == null)
                {
                    return -1;
                }

                // handle weapons - they're always first in the list
                if (equipment1 is Weapon)
                {
                    return equipment2 is Weapon ? equipment1.Name.CompareTo(equipment2.Name) : -1;
                }

                if (equipment2 is Weapon)
                {
                    return 1;
                }

                // compare armor slots
                var armor1 = equipment1 as Armor;
                var armor2 = equipment2 as Armor;
                if (armor1 != null && armor2 != null)
                {
                    return armor1.Slot.CompareTo(armor2.Slot);
                }

                return 0;
            });

        // configure the menu text
        titleText = "Equipped Gear";
        selectButtonText = string.Empty;
        backButtonText = "Back";
        xButtonText = "Unequip";
        yButtonText = string.Empty;
        leftTriggerText = string.Empty;
        rightTriggerText = string.Empty;
    }

    #endregion


    #region Input Handling

    /// <summary>
    ///     Respond to the triggering of the X button (and related key).
    /// </summary>
    protected override void ButtonXPressed(Equipment entry)
    {
        // remove the equipment from the player's equipped list
        fightingCharacter.Unequip(entry);

        // add the equipment back to the party's inventory
        Session.Session.Party.AddToInventory(entry, 1);
    }

    #endregion

    #region Columns

    protected string nameColumnText = "Name";
    private const int nameColumnInterval = 80;

    protected string powerColumnText = "Power (min, max)";
    private const int powerColumnInterval = 270;

    protected string slotColumnText = "Slot";
    private const int slotColumnInterval = 400;

    #endregion


    #region Data Access

    /// <summary>
    ///     The FightingCharacter object whose equipment is displayed.
    /// </summary>
    private readonly FightingCharacter fightingCharacter;


    /// <summary>
    ///     Get the list that this screen displays.
    /// </summary>
    public override ReadOnlyCollection<Equipment> GetDataList()
    {
        return fightingCharacter.EquippedEquipment.AsReadOnly();
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the equipment at the given position in the list.
    /// </summary>
    /// <param name="contentEntry">The equipment to draw.</param>
    /// <param name="position">The position to draw the entry at.</param>
    /// <param name="isSelected">If true, this item is selected.</param>
    protected override void DrawEntry(Equipment entry,
        Vector2 position,
        bool isSelected)
    {
        // check the parameter
        if (entry == null)
        {
            throw new ArgumentNullException("entry");
        }

        var spriteBatch = ScreenManager.SpriteBatch;
        var drawPosition = position;

        // draw the icon
        spriteBatch.Draw(entry.IconTexture, drawPosition + iconOffset, Color.White);

        // draw the name
        var color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;
        drawPosition.Y += listLineSpacing / 4;
        drawPosition.X += nameColumnInterval;
        spriteBatch.DrawString(Fonts.MainFont, entry.Name, drawPosition, color);

        // draw the power
        drawPosition.X += powerColumnInterval;
        var powerText = entry.GetPowerText();
        var powerTextSize = Fonts.MainFont.MeasureString(powerText);
        var powerPosition = drawPosition;
        powerPosition.Y -= (float) Math.Ceiling((powerTextSize.Y - 30f) / 2);
        spriteBatch.DrawString(Fonts.MainFont,
            powerText,
            powerPosition,
            color);

        // draw the slot
        drawPosition.X += slotColumnInterval;
        if (entry is Weapon)
        {
            spriteBatch.DrawString(Fonts.MainFont,
                "Weapon",
                drawPosition,
                color);
        }
        else if (entry is Armor)
        {
            var armor = entry as Armor;
            spriteBatch.DrawString(Fonts.MainFont,
                armor.Slot.ToString(),
                drawPosition,
                color);
        }

        // turn on or off the unequip button
        if (isSelected)
        {
            xButtonText = "Unequip";
        }
    }


    /// <summary>
    ///     Draw the description of the selected item.
    /// </summary>
    protected override void DrawSelectedDescription(Equipment entry)
    {
        // check the parameter
        if (entry == null)
        {
            throw new ArgumentNullException("entry");
        }

        var spriteBatch = ScreenManager.SpriteBatch;
        var position = descriptionTextPosition;

        // draw the description
        // -- it's up to the content owner to fit the description
        var text = entry.Description;
        if (!string.IsNullOrEmpty(text))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                text,
                position,
                Fonts.DescriptionColor);
            position.Y += Fonts.MainFont.LineSpacing;
        }

        // draw the modifiers
        text = entry.OwnerBuffStatistics.GetModifierString();
        if (!string.IsNullOrEmpty(text))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                text,
                position,
                Fonts.DescriptionColor);
            position.Y += Fonts.MainFont.LineSpacing;
        }

        // draw the restrictions
        text = entry.GetRestrictionsText();
        if (!string.IsNullOrEmpty(text))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                text,
                position,
                Fonts.DescriptionColor);
            position.Y += Fonts.MainFont.LineSpacing;
        }
    }


    /// <summary>
    ///     Draw the column headers above the list.
    /// </summary>
    protected override void DrawColumnHeaders()
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var position = listEntryStartPosition;

        position.X += nameColumnInterval;
        if (!string.IsNullOrEmpty(nameColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                nameColumnText,
                position,
                Fonts.CaptionColor);
        }

        position.X += powerColumnInterval;
        if (!string.IsNullOrEmpty(powerColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                powerColumnText,
                position,
                Fonts.CaptionColor);
        }

        position.X += slotColumnInterval;
        if (!string.IsNullOrEmpty(slotColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                slotColumnText,
                position,
                Fonts.CaptionColor);
        }
    }

    #endregion
}
