#region File Description

//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using RolePlayingGame.Combat;
using RolePlayingGame.MenuScreens;
using RolePlayingGame.ScreenManager;
using RolePlayingGame.Session;
using RolePlayingGameData;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     This screen implements the actual game logic. It is just a
///     placeholder to get the idea across: you'll probably want to
///     put some more interesting gameplay in here!
/// </summary>
internal class GameplayScreen : GameScreen
{
    #region Initialization

    private readonly GameStartDescription gameStartDescription;
    private readonly SaveGameDescription saveGameDescription;


    /// <summary>
    ///     Create a new GameplayScreen object.
    /// </summary>
    private GameplayScreen()
    {
        CombatEngine.ClearCombat();
        Exiting += GameplayScreen_Exiting;
    }


    /// <summary>
    ///     Create a new GameplayScreen object from a new-game description.
    /// </summary>
    public GameplayScreen(GameStartDescription gameStartDescription)
        : this()
    {
        this.gameStartDescription = gameStartDescription;
        saveGameDescription = null;
    }


    /// <summary>
    ///     Create a new GameplayScreen object from a saved-game description.
    /// </summary>
    public GameplayScreen(SaveGameDescription saveGameDescription)
        : this()
    {
        gameStartDescription = null;
        this.saveGameDescription = saveGameDescription;
    }


    /// <summary>
    ///     Handle the closing of this screen.
    /// </summary>
    private void GameplayScreen_Exiting(object sender, EventArgs e)
    {
        // make sure the session is ending
        // -- EndSession must be re-entrant safe, as the EndSession may be
        //    making this screen close itself
        Session.Session.EndSession();
    }


    /// <summary>
    ///     Load graphics content for the game.
    /// </summary>
    public override void LoadContent()
    {
        if (gameStartDescription != null)
        {
            Session.Session.StartNewSession(gameStartDescription, ScreenManager, this);
        }
        else if (saveGameDescription != null)
        {
            Session.Session.LoadSession(saveGameDescription, ScreenManager, this);
        }

        // once the load has finished, we use ResetElapsedTime to tell the game's
        // timing mechanism that we have just finished a very long frame, and that
        // it should not try to catch up.
        ScreenManager.Game.ResetElapsedTime();
    }

    #endregion


    #region Update and Draw

    /// <summary>
    ///     Updates the state of the game. This method checks the GameScreen.IsActive
    ///     property, so the game will stop updating when the pause menu is active,
    ///     or if you tab away to a different application.
    /// </summary>
    public override void Update(GameTime gameTime,
        bool otherScreenHasFocus,
        bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (IsActive && !coveredByOtherScreen)
        {
            Session.Session.Update(gameTime);
        }
    }


    /// <summary>
    ///     Lets the game respond to player input. Unlike the Update method,
    ///     this will only be called when the gameplay screen is active.
    /// </summary>
    public override void HandleInput()
    {
        if (InputManager.IsActionTriggered(InputManager.Action.MainMenu))
        {
            ScreenManager.AddScreen(new MainMenuScreen());
            return;
        }

        if (InputManager.IsActionTriggered(InputManager.Action.ExitGame))
        {
            // add a confirmation message box
            const string message =
                "Are you sure you want to exit?  All unsaved progress will be lost.";
            var confirmExitMessageBox = new MessageBoxScreen(message);
            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;
            ScreenManager.AddScreen(confirmExitMessageBox);
            return;
        }

        if (!CombatEngine.IsActive &&
            InputManager.IsActionTriggered(InputManager.Action.CharacterManagement))
        {
            ScreenManager.AddScreen(new StatisticsScreen(Session.Session.Party.Players[0]));
        }
    }


    /// <summary>
    ///     Event handler for when the user selects Yes
    ///     on the "Are you sure?" message box.
    /// </summary>
    private void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
    {
        ScreenManager.Game.Exit();
    }


    /// <summary>
    ///     Draws the gameplay screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        Session.Session.Draw(gameTime);
    }

    #endregion
}
