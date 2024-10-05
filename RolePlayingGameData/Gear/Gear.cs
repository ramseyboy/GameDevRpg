#region File Description

//-----------------------------------------------------------------------------
// Gear.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RolePlayingGameData;

/// <summary>
///     An inventory element - items, equipment, etc.
/// </summary>
#if !XBOX
[DebuggerDisplay("Name = {name}")]
#endif
public abstract class Gear : ContentObject
{
    #region Description Data

    /// <summary>
    ///     The name of this gear.
    /// </summary>
    private string name;

    /// <summary>
    ///     The name of this gear.
    /// </summary>
    public string Name
    {
        get => name;
        set => name = value;
    }


    /// <summary>
    ///     The long description of this gear.
    /// </summary>
    private string description;

    /// <summary>
    ///     The long description of this gear.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }


    /// <summary>
    ///     Builds and returns a string describing the power of this gear.
    /// </summary>
    public virtual string GetPowerText()
    {
        return string.Empty;
    }

    #endregion


    #region Value Data

    /// <summary>
    ///     The value of this gear.
    /// </summary>
    /// <remarks>If the value is less than zero, it cannot be sold.</remarks>
    private int goldValue;

    /// <summary>
    ///     The value of this gear.
    /// </summary>
    /// <remarks>If the value is less than zero, it cannot be sold.</remarks>
    public int GoldValue
    {
        get => goldValue;
        set => goldValue = value;
    }


    /// <summary>
    ///     If true, the gear can be dropped.  If false, it cannot ever be dropped.
    /// </summary>
    private bool isDroppable;

    /// <summary>
    ///     If true, the gear can be dropped.  If false, it cannot ever be dropped.
    /// </summary>
    public bool IsDroppable
    {
        get => isDroppable;
        set => isDroppable = value;
    }

    #endregion


    #region Restrictions

    /// <summary>
    ///     The minimum character level required to equip or use this gear.
    /// </summary>
    private int minimumCharacterLevel;

    /// <summary>
    ///     The minimum character level required to equip or use this gear.
    /// </summary>
    public int MinimumCharacterLevel
    {
        get => minimumCharacterLevel;
        set => minimumCharacterLevel = value;
    }


    /// <summary>
    ///     The list of the names of all supported classes.
    /// </summary>
    /// <remarks>Class names are compared case-insensitive.</remarks>
    private readonly List<string> supportedClasses = new();

    /// <summary>
    ///     The list of the names of all supported classes.
    /// </summary>
    /// <remarks>Class names are compared case-insensitive.</remarks>
    public List<string> SupportedClasses => supportedClasses;


    /// <summary>
    ///     Check the restrictions on this object against the provided character.
    /// </summary>
    /// <returns>True if the gear could be used, false otherwise.</returns>
    public virtual bool CheckRestrictions(FightingCharacter fightingCharacter)
    {
        if (fightingCharacter == null)
        {
            throw new ArgumentNullException("fightingCharacter");
        }

        return fightingCharacter.CharacterLevel >= MinimumCharacterLevel &&
               (SupportedClasses.Count <= 0 ||
                SupportedClasses.Contains(fightingCharacter.CharacterClass.Name));
    }


    /// <summary>
    ///     Builds a string describing the restrictions on this piece of gear.
    /// </summary>
    public virtual string GetRestrictionsText()
    {
        var sb = new StringBuilder();

        // add the minimum character level, if any
        if (MinimumCharacterLevel > 0)
        {
            sb.Append("Level - ");
            sb.Append(MinimumCharacterLevel.ToString());
            sb.Append("; ");
        }

        // add the classes
        if (SupportedClasses.Count > 0)
        {
            sb.Append("Class - ");
            var firstClass = true;
            foreach (var className in SupportedClasses)
            {
                if (firstClass)
                {
                    firstClass = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append(className);
            }
        }

        return sb.ToString();
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The content path and name of the icon for this gear.
    /// </summary>
    private string iconTextureName;

    /// <summary>
    ///     The content path and name of the icon for this gear.
    /// </summary>
    public string IconTextureName
    {
        get => iconTextureName;
        set => iconTextureName = value;
    }


    /// <summary>
    ///     The icon texture for this gear.
    /// </summary>
    private Texture2D iconTexture;

    /// <summary>
    ///     The icon texture for this gear.
    /// </summary>
    [ContentSerializerIgnore]
    public Texture2D IconTexture
    {
        get => iconTexture;
        set => iconTexture = value;
    }

    #endregion


    #region Drawing Methods

    /// <summary>
    ///     Draw the icon for this gear.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch object to use when drawing.</param>
    /// <param name="position">The position of the icon on the screen.</param>
    public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
    {
        // check the parameters
        if (spriteBatch == null)
        {
            throw new ArgumentNullException("spriteBatch");
        }

        // draw the icon, if we there is a texture for it
        if (iconTexture != null)
        {
            spriteBatch.Draw(iconTexture, position, Color.White);
        }
    }


    /// <summary>
    ///     Draw the description for this gear in the space provided.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch object to use when drawing.</param>
    /// <param name="spriteFont">The font that the text is drawn with.</param>
    /// <param name="color">The color of the text.</param>
    /// <param name="position">The position of the text on the screen.</param>
    /// <param name="maximumCharactersPerLine">
    ///     The maximum length of a single line of text.
    /// </param>
    /// <param name="maximumLines">The maximum number of lines to draw.</param>
    public virtual void DrawDescription(SpriteBatch spriteBatch,
        SpriteFont spriteFont,
        Color color,
        Vector2 position,
        int maximumCharactersPerLine,
        int maximumLines)
    {
        // check the parameters
        if (spriteBatch == null)
        {
            throw new ArgumentNullException("spriteBatch");
        }

        if (spriteFont == null)
        {
            throw new ArgumentNullException("spriteFont");
        }

        if (maximumLines <= 0)
        {
            throw new ArgumentOutOfRangeException("maximumLines");
        }

        if (maximumCharactersPerLine <= 0)
        {
            throw new ArgumentOutOfRangeException("maximumCharactersPerLine");
        }

        // if the string is trivial, then this is really easy
        if (string.IsNullOrEmpty(description))
        {
            return;
        }

        // if the text is short enough to fit on one line, then this is still easy
        if (description.Length < maximumCharactersPerLine)
        {
            spriteBatch.DrawString(spriteFont, description, position, color);
            return;
        }

        // construct a new string with carriage returns
        var stringBuilder = new StringBuilder(description);
        var currentLine = 0;
        var newLineIndex = 0;
        while (description.Length - newLineIndex > maximumCharactersPerLine &&
               currentLine < maximumLines)
        {
            description.IndexOf(' ', 0);
            var nextIndex = newLineIndex;
            while (nextIndex < maximumCharactersPerLine)
            {
                newLineIndex = nextIndex;
                nextIndex = description.IndexOf(' ', newLineIndex + 1);
            }

            stringBuilder.Replace(' ', '\n', newLineIndex, 1);
            currentLine++;
        }

        // draw the string
        spriteBatch.DrawString(spriteFont,
            stringBuilder.ToString(),
            position,
            color);
    }

    #endregion
}
