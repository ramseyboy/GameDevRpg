#region File Description

//-----------------------------------------------------------------------------
// GameOverScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGame.MenuScreens;
using RolePlayingGame.ScreenManager;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Displays the game-over screen, after the player has lost.
/// </summary>
internal class GameOverScreen : GameScreen
{
    #region Updating

    /// <summary>
    ///     Handles user input.
    /// </summary>
    public override void HandleInput()
    {
        if (InputManager.IsActionTriggered(InputManager.Action.Ok) ||
            InputManager.IsActionTriggered(InputManager.Action.Back))
        {
            ExitScreen();
            ScreenManager.AddScreen(new MainMenuScreen());
        }
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draws the screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        spriteBatch.Begin();

        // Draw fading screen
        spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1280, 720), Color.White);

        // Draw popup texture
        spriteBatch.Draw(backTexture, backgroundPosition, Color.White);

        // Draw title
        spriteBatch.DrawString(Fonts.MainFont,
            titleString,
            titlePosition,
            Fonts.TitleColor);

        // Draw Gameover text
        spriteBatch.DrawString(Fonts.MainFont,
            gameOverString,
            gameOverPosition,
            Fonts.CountColor);

        // Draw select button
        spriteBatch.DrawString(Fonts.MainFont,
            selectString,
            selectPosition,
            Color.White);
        spriteBatch.Draw(selectIconTexture, selectIconPosition, Color.White);

        spriteBatch.End();
    }

    #endregion

    #region Graphics Data

    private Texture2D backTexture;
    private Texture2D selectIconTexture;
    private Texture2D fadeTexture;
    private Vector2 backgroundPosition;
    private Vector2 titlePosition;
    private Vector2 gameOverPosition;
    private Vector2 selectPosition;
    private Vector2 selectIconPosition;

    #endregion


    #region Text Data

    private readonly string titleString = "Game Over";
    private readonly string gameOverString = "The party has been defeated.";
    private readonly string selectString = "Continue";

    #endregion


    #region Initialization

    /// <summary>
    ///     Create a new GameOverScreen object.
    /// </summary>
    public GameOverScreen()
    {
        AudioManager.PushMusic("LoseTheme");
        Exiting += GameOverScreen_Exiting;
    }


    private void GameOverScreen_Exiting(object sender, EventArgs e)
    {
        AudioManager.PopMusic();
    }


    /// <summary>
    ///     Load the graphics data from the content manager.
    /// </summary>
    public override void LoadContent()
    {
        var content = ScreenManager.Game.Content;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        fadeTexture = content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
        backTexture = content.Load<Texture2D>(@"Textures\GameScreens\PopupScreen");
        selectIconTexture = content.Load<Texture2D>(@"Textures\Buttons\AButton");

        backgroundPosition.X = (viewport.Width - backTexture.Width) / 2;
        backgroundPosition.Y = (viewport.Height - backTexture.Height) / 2;

        titlePosition.X = (viewport.Width -
                           Fonts.MainFont.MeasureString(titleString).X) / 2;
        titlePosition.Y = backgroundPosition.Y + 70f;

        gameOverPosition.X = (viewport.Width -
                              Fonts.MainFont.MeasureString(titleString).X) / 2;
        gameOverPosition.Y = backgroundPosition.Y + backTexture.Height / 2;

        selectIconPosition.X = viewport.Width / 2 + 260;
        selectIconPosition.Y = backgroundPosition.Y + 530f;
        selectPosition.X = selectIconPosition.X -
                           Fonts.MainFont.MeasureString(selectString).X - 10f;
        selectPosition.Y = backgroundPosition.Y + 530f;
    }

    #endregion
}
