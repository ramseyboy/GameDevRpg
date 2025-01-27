#region File Description

//-----------------------------------------------------------------------------
// StoreScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGame.ScreenManager;
using RolePlayingGameData.Map;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Draws the options available in a store - typically to buy or sell gear.
/// </summary>
internal class StoreScreen : GameScreen
{
    private readonly Store store;


    #region Updating

    /// <summary>
    ///     Handles user input.
    /// </summary>
    public override void HandleInput()
    {
        // exits the screen
        if (InputManager.IsActionTriggered(InputManager.Action.Back))
        {
            ExitScreen();
            return;
        }
        // select one of the buttons

        if (InputManager.IsActionTriggered(InputManager.Action.Ok))
        {
            if (currentCursor == 0)
            {
                ScreenManager.AddScreen(new StoreBuyScreen(store));
            }
            else if (currentCursor == 1)
            {
                ScreenManager.AddScreen(new StoreSellScreen(store));
            }
            else
            {
                ExitScreen();
            }

            return;
        }
        // move the cursor up

        if (InputManager.IsActionTriggered(
                InputManager.Action.MoveCharacterUp))
        {
            currentCursor--;
            if (currentCursor < 0)
            {
                currentCursor = 0;
            }
        }
        // move the cursor down
        else if (InputManager.IsActionTriggered(
                     InputManager.Action.MoveCharacterDown))
        {
            currentCursor++;
            if (currentCursor > 2)
            {
                currentCursor = 2;
            }
        }
    }

    #endregion


    #region Graphics Data

    private Texture2D shopDrawScreen;
    private Texture2D selectButton;
    private Texture2D backButton;
    private Texture2D highlightItem;
    private Texture2D selectionArrow;
    private Texture2D conversationStrip;
    private Texture2D plankTexture;
    private Texture2D fadeTexture;
    private Texture2D goldIcon;

    private readonly Vector2 textPosition = new(620, 250);
    private readonly Vector2 backButtonPosition = new(80, 640);
    private readonly Vector2 selectButtonPosition = new(1150, 640);
    private readonly Vector2 partyGoldPosition = new(565, 648);
    private readonly Vector2 shopKeeperPosition = new(290, 370);
    private readonly Vector2 welcomeMessagePosition = new(470, 460);
    private readonly Vector2 conversationStripPosition = new(240, 405);
    private readonly Vector2 goldIconPosition = new(490, 640);
    private readonly Vector2 highlightItemOffset = new(400, 20);
    private readonly Vector2 selectionArrowOffset = new(100, 16);

    private Vector2 shopNamePosition;
    private Vector2 plankPosition;
    private Vector2 titleBarMidPosition;
    private readonly Vector2 placeTextMid;
    private Rectangle screenRect;

    private int currentCursor;
    private const int interval = 50;

    #endregion


    #region Initialization

    /// <summary>
    ///     Constructs a new StoreScreen object for the given store.
    /// </summary>
    public StoreScreen(Store store)
    {
        // check the parameter
        if (store == null)
        {
            throw new ArgumentNullException("store");
        }

        IsPopup = true;
        this.store = store;

        titleBarMidPosition = new Vector2(
            -Fonts.HeaderFont.MeasureString(store.Name).X / 2,
            0f);
        placeTextMid = Fonts.MainFont.MeasureString("Select");
    }


    /// <summary>
    ///     Loads the graphics content from the content manager.
    /// </summary>
    public override void LoadContent()
    {
        var content = ScreenManager.Game.Content;

        shopDrawScreen =
            content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
        backButton =
            content.Load<Texture2D>(@"Textures\Buttons\BButton");
        selectButton =
            content.Load<Texture2D>(@"Textures\Buttons\AButton");
        highlightItem =
            content.Load<Texture2D>(@"Textures\GameScreens\HighlightLarge");
        selectionArrow =
            content.Load<Texture2D>(@"Textures\GameScreens\SelectionArrow");
        fadeTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");
        conversationStrip =
            content.Load<Texture2D>(@"Textures\GameScreens\ConversationStrip");
        goldIcon =
            content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");
        plankTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");

        var viewport = ScreenManager.GraphicsDevice.Viewport;
        screenRect = new Rectangle(viewport.X,
            viewport.Y,
            viewport.Width,
            viewport.Height);
        plankPosition = new Vector2(
            (viewport.Width - plankTexture.Width) / 2,
            66f);
        shopNamePosition = new Vector2(
            (viewport.Width - Fonts.HeaderFont.MeasureString(store.Name).X) / 2,
            90f);
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        // Draw Shop Main Menu
        spriteBatch.Begin();

        // Draw Shop Main Menu Screen
        DrawMainMenu();

        // Draw Buttons
        if (IsActive)
        {
            DrawButtons();
        }

        // Measure Title of the Screen


        spriteBatch.Draw(plankTexture, plankPosition, Color.White);

        // Draw the Title of the Screen
        spriteBatch.DrawString(Fonts.HeaderFont,
            store.Name,
            shopNamePosition,
            Fonts.TitleColor);

        // Draw Conversation Strip
        spriteBatch.Draw(conversationStrip,
            conversationStripPosition,
            Color.White);

        // Draw Shop Keeper
        spriteBatch.Draw(store.ShopkeeperTexture, shopKeeperPosition, Color.White);

        // Draw Shop Info
        spriteBatch.DrawString(Fonts.MainFont,
            Fonts.BreakTextIntoLines(store.WelcomeMessage, 55, 3),
            welcomeMessagePosition,
            Fonts.DescriptionColor);

        spriteBatch.End();
    }


    /// <summary>
    ///     Draws the main menu for the store.
    /// </summary>
    private void DrawMainMenu()
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        var arrowPosition = Vector2.Zero;
        var highlightPosition = Vector2.Zero;
        var position = textPosition;

        // Draw faded screen
        spriteBatch.Draw(fadeTexture, screenRect, Color.White);

        spriteBatch.Draw(shopDrawScreen, screenRect, Color.White);

        arrowPosition.X = textPosition.X - selectionArrowOffset.X;
        arrowPosition.Y = textPosition.Y - selectionArrowOffset.Y;

        highlightPosition.X = textPosition.X - highlightItemOffset.X;
        highlightPosition.Y = textPosition.Y - highlightItemOffset.Y;

        // "Buy" is highlighted
        if (currentCursor == 0)
        {
            spriteBatch.Draw(highlightItem, highlightPosition, Color.White);
            spriteBatch.Draw(selectionArrow, arrowPosition, Color.White);
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Buy",
                position,
                Fonts.HighlightColor);

            position.Y += interval;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Sell",
                position,
                Fonts.DisplayColor);

            position.Y += interval;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Leave",
                position,
                Fonts.DisplayColor);
        }
        // "Sell" is highlighted
        else if (currentCursor == 1)
        {
            position = textPosition;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Buy",
                position,
                Fonts.DisplayColor);

            highlightPosition.Y += interval;
            arrowPosition.Y += interval;
            position.Y += interval;

            spriteBatch.Draw(highlightItem, highlightPosition, Color.White);
            spriteBatch.Draw(selectionArrow, arrowPosition, Color.White);
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Sell",
                position,
                Fonts.HighlightColor);
            position.Y += interval;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Leave",
                position,
                Fonts.DisplayColor);
        }
        // "Leave" is highlighted
        else if (currentCursor == 2)
        {
            position = textPosition;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Buy",
                position,
                Fonts.DisplayColor);

            position.Y += interval;
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Sell",
                position,
                Fonts.DisplayColor);

            highlightPosition.Y += interval + interval;
            arrowPosition.Y += interval + interval;
            position.Y += interval;

            spriteBatch.Draw(highlightItem, highlightPosition, Color.White);
            spriteBatch.Draw(selectionArrow, arrowPosition, Color.White);
            spriteBatch.DrawString(Fonts.GearInfoFont,
                "Leave",
                position,
                Fonts.HighlightColor);
        }
    }


    /// <summary>
    ///     Draws the buttons.
    /// </summary>
    private void DrawButtons()
    {
        if (!IsActive)
        {
            return;
        }

        var spriteBatch = ScreenManager.SpriteBatch;

        var position = new Vector2();

        // Draw Back Button
        spriteBatch.Draw(backButton, backButtonPosition, Color.White);

        // Draw Back Text
        position = backButtonPosition;
        position.X += backButton.Width + 10;
        position.Y += 5;
        spriteBatch.DrawString(Fonts.MainFont, "Back", position, Color.White);

        // Draw Select Button
        spriteBatch.Draw(selectButton, selectButtonPosition, Color.White);

        // Draw Select Text
        position = selectButtonPosition;
        position.X -= placeTextMid.X + 10;
        position.Y += 5;
        spriteBatch.DrawString(Fonts.MainFont,
            "Select",
            position,
            Color.White);

        // Draw Gold Text
        spriteBatch.DrawString(Fonts.MainFont,
            Fonts.GetGoldString(Session.Session.Party.PartyGold),
            partyGoldPosition,
            Color.White);

        // Draw Gold Icon
        spriteBatch.Draw(goldIcon, goldIconPosition, Color.White);
    }

    #endregion
}
