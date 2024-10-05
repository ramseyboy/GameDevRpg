#region File Description

//-----------------------------------------------------------------------------
// SpellbookScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using RolePlayingGame.Combat;
using RolePlayingGameData;
using RolePlayingGameData.Characters;
using RolePlayingGameData.Data;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Lists the spells available to the character.
/// </summary>
internal class SpellbookScreen : ListScreen<Spell>
{
    #region Initialization

    /// <summary>
    ///     Creates a new SpellbookScreen object for the given player and statistics.
    /// </summary>
    public SpellbookScreen(FightingCharacter fightingCharacter,
        StatisticsValue statistics)
    {
        // check the parameter
        if (fightingCharacter == null)
        {
            throw new ArgumentNullException("fightingCharacter");
        }

        this.fightingCharacter = fightingCharacter;
        this.statistics = statistics;

        // sort the player's spell
        this.fightingCharacter.Spells.Sort(
            delegate(Spell spell1, Spell spell2)
            {
                // handle null values
                if (spell1 == null)
                {
                    return spell2 == null ? 0 : 1;
                }

                if (spell2 == null)
                {
                    return -1;
                }

                // sort by name
                return spell1.Name.CompareTo(spell2.Name);
            });

        // configure the menu text
        titleText = "Spell Book";
        selectButtonText = "Cast";
        backButtonText = "Back";
        xButtonText = string.Empty;
        yButtonText = string.Empty;
        leftTriggerText = string.Empty;
        rightTriggerText = string.Empty;
    }

    #endregion

    #region Graphics Data

    private readonly Vector2 spellDescriptionPosition = new(200, 550);
    private readonly Vector2 warningMessagePosition = new(200, 580);

    #endregion


    #region Columns

    private readonly string nameColumnText = "Name";
    private const int nameColumnInterval = 80;

    private readonly string levelColumnText = "Level";
    private const int levelColumnInterval = 240;

    private readonly string powerColumnText = "Power (min, max)";
    private const int powerColumnInterval = 110;

    private readonly string magicCostColumnText = "MP";
    private const int magicCostColumnInterval = 380;

    #endregion


    #region Data Access

    /// <summary>
    ///     The FightingCharacter object whose spells are displayed.
    /// </summary>
    private readonly FightingCharacter fightingCharacter;


    /// <summary>
    ///     The statistics of the character, for calculating the eligibility of spells.
    /// </summary>
    /// <remarks>
    ///     Needed because combat statistics override character statistics.
    /// </remarks>
    private readonly StatisticsValue statistics;


    /// <summary>
    ///     Get the list that this screen displays.
    /// </summary>
    public override ReadOnlyCollection<Spell> GetDataList()
    {
        return fightingCharacter.Spells.AsReadOnly();
    }

    #endregion


    #region Input Handling

    /// <summary>
    ///     Delegate for spell-selection events.
    /// </summary>
    public delegate void SpellSelectedHandler(Spell spell);


    /// <summary>
    ///     Responds when an spell is selected by this menu.
    /// </summary>
    /// <remarks>
    ///     Typically used by the calling menu, like the combat HUD menu,
    ///     to respond to selection.
    /// </remarks>
    public event SpellSelectedHandler SpellSelected;


    /// <summary>
    ///     Respond to the triggering of the Select action (and related key).
    /// </summary>
    protected override void SelectTriggered(Spell entry)
    {
        // check the parameter
        if (entry == null)
        {
            return;
        }

        // make sure the spell can be selected
        if (!CanSelectEntry(entry))
        {
            return;
        }

        // if the event is valid, fire it and exit this screen
        if (SpellSelected != null)
        {
            SpellSelected(entry);
            ExitScreen();
        }
    }


    /// <summary>
    ///     Returns true if the specified spell can be selected.
    /// </summary>
    private bool CanSelectEntry(Spell entry)
    {
        if (entry == null)
        {
            return false;
        }

        return statistics.MagicPoints >= entry.MagicPointCost &&
               (!entry.IsOffensive || CombatEngine.IsActive);
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the spell at the given position in the list.
    /// </summary>
    /// <param name="contentEntry">The spell to draw.</param>
    /// <param name="position">The position to draw the entry at.</param>
    /// <param name="isSelected">If true, this item is selected.</param>
    protected override void DrawEntry(Spell entry,
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
        var color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;

        // draw the icon
        spriteBatch.Draw(entry.IconTexture, drawPosition + iconOffset, Color.White);

        // draw the name
        drawPosition.Y += listLineSpacing / 4;
        drawPosition.X += nameColumnInterval;
        spriteBatch.DrawString(Fonts.GearInfoFont, entry.Name, drawPosition, color);

        // draw the level
        drawPosition.X += levelColumnInterval;
        spriteBatch.DrawString(Fonts.GearInfoFont,
            entry.Level.ToString(),
            drawPosition,
            color);

        // draw the power
        drawPosition.X += powerColumnInterval;
        var powerText = entry.GetPowerText();
        var powerTextSize = Fonts.GearInfoFont.MeasureString(powerText);
        var powerPosition = drawPosition;
        powerPosition.Y -= (float) Math.Ceiling((powerTextSize.Y - 30f) / 2);
        spriteBatch.DrawString(Fonts.GearInfoFont,
            powerText,
            powerPosition,
            color);

        // draw the quantity
        drawPosition.X += magicCostColumnInterval;
        spriteBatch.DrawString(Fonts.GearInfoFont,
            entry.MagicPointCost.ToString(),
            drawPosition,
            color);

        // draw the cast button if needed
        if (isSelected)
        {
            selectButtonText = CanSelectEntry(entry) && SpellSelected != null ? "Cast" : string.Empty;
        }
    }


    /// <summary>
    ///     Draw the description of the selected item.
    /// </summary>
    protected override void DrawSelectedDescription(Spell entry)
    {
        // check the parameter
        if (entry == null)
        {
            throw new ArgumentNullException("entry");
        }

        var spriteBatch = ScreenManager.SpriteBatch;
        var position = descriptionTextPosition;

        // draw the insufficient-mp warning
        if (CombatEngine.IsActive && entry.MagicPointCost > statistics.MagicPoints)
        {
            // draw the insufficient-mp warning
            spriteBatch.DrawString(Fonts.MainFont,
                "Not enough MP to Cast Spell",
                warningMessagePosition,
                Color.Red);
        }

        // draw the description
        spriteBatch.DrawString(Fonts.MainFont,
            Fonts.BreakTextIntoLines(entry.Description, 90, 3),
            spellDescriptionPosition,
            Fonts.DescriptionColor);
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

        position.X += levelColumnInterval;
        if (!string.IsNullOrEmpty(levelColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                levelColumnText,
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

        position.X += magicCostColumnInterval;
        if (!string.IsNullOrEmpty(magicCostColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                magicCostColumnText,
                position,
                Fonts.CaptionColor);
        }
    }

    #endregion
}
