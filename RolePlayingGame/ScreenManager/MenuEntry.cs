#region File Description

//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RolePlayingGame.ScreenManager;

/// <summary>
///     Helper class represents a single entry in a MenuScreen. By default this
///     just draws the entry text string, but it can be customized to display menu
///     entries in different ways. This also provides an event that will be raised
///     when the menu entry is selected.
/// </summary>
/// <remarks>
///     Similar to a class found in the Game State Management sample on the
///     XNA Creators Club Online website (http://creators.xna.com).
/// </remarks>
internal class MenuEntry
{
    #region Initialization

    /// <summary>
    ///     Constructs a new menu entry with the specified text.
    /// </summary>
    public MenuEntry(string text)
    {
        this.text = text;
    }

    #endregion

    #region Fields

    /// <summary>
    ///     The text rendered for this entry.
    /// </summary>
    private string text;


    /// <summary>
    ///     The font used for this menu item.
    /// </summary>
    private SpriteFont spriteFont;


    /// <summary>
    ///     The position of this menu item on the screen.
    /// </summary>
    private Vector2 position;


    /// <summary>
    ///     A description of the function of the button.
    /// </summary>
    private string description;


    /// <summary>
    ///     An optional texture drawn with the text.
    /// </summary>
    /// <remarks>If present, the text will be centered on the texture.</remarks>
    private Texture2D texture;

    #endregion


    #region Properties

    /// <summary>
    ///     Gets or sets the text of this menu entry.
    /// </summary>
    public string Text
    {
        get => text;
        set => text = value;
    }


    /// <summary>
    ///     Gets or sets the font used to draw this menu entry.
    /// </summary>
    public SpriteFont Font
    {
        get => spriteFont;
        set => spriteFont = value;
    }


    /// <summary>
    ///     Gets or sets the position of this menu entry.
    /// </summary>
    public Vector2 Position
    {
        get => position;
        set => position = value;
    }


    /// <summary>
    ///     A description of the function of the button.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }


    /// <summary>
    ///     An optional texture drawn with the text.
    /// </summary>
    /// <remarks>If present, the text will be centered on the texture.</remarks>
    public Texture2D Texture
    {
        get => texture;
        set => texture = value;
    }

    #endregion


    #region Events

    /// <summary>
    ///     Event raised when the menu entry is selected.
    /// </summary>
    public event EventHandler<EventArgs> Selected;


    /// <summary>
    ///     Method for raising the Selected event.
    /// </summary>
    protected internal virtual void OnSelectEntry()
    {
        if (Selected != null)
        {
            Selected(this, EventArgs.Empty);
        }
    }

    #endregion


    #region Update and Draw

    /// <summary>
    ///     Updates the menu entry.
    /// </summary>
    public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
    }


    /// <summary>
    ///     Draws the menu entry. This can be overridden to customize the appearance.
    /// </summary>
    public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        // Draw the selected entry in yellow, otherwise white.
        var color = isSelected ? Fonts.MenuSelectedColor : Fonts.TitleColor;

        // Draw text, centered on the middle of each line.
        var screenManager = screen.ScreenManager;
        var spriteBatch = screenManager.SpriteBatch;

        if (texture != null)
        {
            spriteBatch.Draw(texture, position, Color.White);
            if (spriteFont != null && !string.IsNullOrEmpty(text))
            {
                var textSize = spriteFont.MeasureString(text);
                var textPosition = position + new Vector2(
                    (float) Math.Floor((texture.Width - textSize.X) / 2),
                    (float) Math.Floor((texture.Height - textSize.Y) / 2));
                spriteBatch.DrawString(spriteFont, text, textPosition, color);
            }
        }
        else if (spriteFont != null && !string.IsNullOrEmpty(text))
        {
            spriteBatch.DrawString(spriteFont, text, position, color);
        }
    }


    /// <summary>
    ///     Queries how much space this menu entry requires.
    /// </summary>
    public virtual int GetHeight(MenuScreen screen)
    {
        return Font.LineSpacing;
    }

    #endregion
}
