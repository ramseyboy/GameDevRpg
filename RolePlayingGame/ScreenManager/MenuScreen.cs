#region File Description

//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace RolePlayingGame.ScreenManager;

/// <summary>
///     Base class for screens that contain a menu of options. The user can
///     move up and down to select an entry, or cancel to back out of the screen.
/// </summary>
/// <remarks>
///     Similar to a class found in the Game State Management sample on the
///     XNA Creators Club Online website (http://creators.xna.com).
/// </remarks>
internal abstract class MenuScreen : GameScreen
{
    #region Initialization

    /// <summary>
    ///     Constructor.
    /// </summary>
    public MenuScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    #endregion

    #region Fields

    private readonly List<MenuEntry> menuEntries = new();
    protected int selectedEntry;

    #endregion


    #region Properties

    /// <summary>
    ///     Gets the list of menu entries, so derived classes can add
    ///     or change the menu contents.
    /// </summary>
    protected IList<MenuEntry> MenuEntries => menuEntries;


    protected MenuEntry SelectedMenuEntry
    {
        get
        {
            if (selectedEntry < 0 || selectedEntry >= menuEntries.Count)
            {
                return null;
            }

            return menuEntries[selectedEntry];
        }
    }

    #endregion


    #region Handle Input

    /// <summary>
    ///     Responds to user input, changing the selected entry and accepting
    ///     or cancelling the menu.
    /// </summary>
    public override void HandleInput()
    {
        var oldSelectedEntry = selectedEntry;

        // Move to the previous menu entry?
        if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
        {
            selectedEntry--;
            if (selectedEntry < 0)
            {
                selectedEntry = menuEntries.Count - 1;
            }
        }

        // Move to the next menu entry?
        if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
        {
            selectedEntry++;
            if (selectedEntry >= menuEntries.Count)
            {
                selectedEntry = 0;
            }
        }

        // Accept or cancel the menu?
        if (InputManager.IsActionTriggered(InputManager.Action.Ok))
        {
            AudioManager.PlayCue("Continue");
            OnSelectEntry(selectedEntry);
        }
        else if (InputManager.IsActionTriggered(InputManager.Action.Back) ||
                 InputManager.IsActionTriggered(InputManager.Action.ExitGame))
        {
            OnCancel();
        }
        else if (selectedEntry != oldSelectedEntry)
        {
            AudioManager.PlayCue("MenuMove");
        }
    }


    /// <summary>
    ///     Handler for when the user has chosen a menu entry.
    /// </summary>
    protected virtual void OnSelectEntry(int entryIndex)
    {
        menuEntries[selectedEntry].OnSelectEntry();
    }


    /// <summary>
    ///     Handler for when the user has cancelled the menu.
    /// </summary>
    protected virtual void OnCancel()
    {
        ExitScreen();
    }


    /// <summary>
    ///     Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
    /// </summary>
    protected void OnCancel(object sender, EventArgs e)
    {
        OnCancel();
    }

    #endregion


    #region Update and Draw

    /// <summary>
    ///     Updates the menu.
    /// </summary>
    public override void Update(GameTime gameTime,
        bool otherScreenHasFocus,
        bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Update each nested MenuEntry object.
        for (var i = 0; i < menuEntries.Count; i++)
        {
            var isSelected = IsActive && i == selectedEntry;

            menuEntries[i].Update(this, isSelected, gameTime);
        }
    }


    /// <summary>
    ///     Draws the menu.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin();

        // Draw each menu entry in turn.
        for (var i = 0; i < menuEntries.Count; i++)
        {
            var menuEntry = menuEntries[i];

            var isSelected = IsActive && i == selectedEntry;

            menuEntry.Draw(this, isSelected, gameTime);
        }

        spriteBatch.End();
    }

    #endregion
}
