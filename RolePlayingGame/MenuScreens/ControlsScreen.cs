#region File Description

//-----------------------------------------------------------------------------
// ControlsScreen.cs
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

#endregion

namespace RolePlayingGame.MenuScreens;

/// <summary>
///     Displays the in-game controls to the user.
/// </summary>
/// <remarks>One possible extension would be to enable control remapping.</remarks>
internal class ControlsScreen : GameScreen
{
    #region Updating

    /// <summary>
    ///     Handles user input.
    /// </summary>
    public override void HandleInput()
    {
        // exit the screen
        if (InputManager.IsActionTriggered(InputManager.Action.Back))
        {
            ExitScreen();
        }
#if !XBOX
        // toggle between keyboard and gamepad controls
        else if (InputManager.IsActionTriggered(InputManager.Action.PageLeft) ||
                 InputManager.IsActionTriggered(InputManager.Action.PageRight))
        {
            isShowControlPad = !isShowControlPad;
        }

        // scroll through the keyboard controls
        if (isShowControlPad == false)
        {
            if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                if (startIndex < keyboardInfo.totalActionList.Length -
                    maxActionDisplay)
                {
                    startIndex++;
                    keyboardInfo.selectedIndex++;
                }
            }

            if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                if (startIndex > 0)
                {
                    startIndex--;
                    keyboardInfo.selectedIndex--;
                }
            }
        }
#endif
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draws the control screen
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var textPosition = Vector2.Zero;

        spriteBatch.Begin();

        // Draw the background texture
        spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

        // Draw the back icon and text
        spriteBatch.Draw(backTexture, backPosition, Color.White);
        spriteBatch.DrawString(Fonts.MainFont,
            "Back",
            new Vector2(backPosition.X + 55, backPosition.Y + 5),
            Color.White);

        // Draw the plank
        spriteBatch.Draw(plankTexture, plankPosition, Color.White);

#if !XBOX
        // Draw the trigger buttons
        spriteBatch.Draw(leftTriggerButton, leftTriggerPosition, Color.White);
        spriteBatch.Draw(rightTriggerButton, rightTriggerPosition, Color.White);
#endif

        // Draw the base border
        spriteBatch.Draw(baseBorderTexture, baseBorderPosition, Color.White);

        // draw the control pad screen
        if (isShowControlPad)
        {
            spriteBatch.Draw(controlPadTexture,
                controlPosition,
                Color.White);

            for (var i = 0; i < leftStrings.Length; i++)
            {
                spriteBatch.DrawString(Fonts.MainFont,
                    leftStrings[i].text,
                    leftStrings[i].textPosition,
                    Color.Black);
            }

            for (var i = 0; i < rightStrings.Length; i++)
            {
                spriteBatch.DrawString(Fonts.MainFont,
                    rightStrings[i].text,
                    rightStrings[i].textPosition,
                    Color.Black);
            }

#if !XBOX
            // Near left trigger
            spriteBatch.DrawString(Fonts.MainFont,
                "Keyboard",
                new Vector2(leftTriggerPosition.X + (leftTriggerButton.Width -
                                                     Fonts.MainFont.MeasureString("Keyboard").X) / 2,
                    rightTriggerPosition.Y + 85),
                Color.Black);

            // Near right trigger
            spriteBatch.DrawString(Fonts.MainFont,
                "Keyboard",
                new Vector2(rightTriggerPosition.X + (rightTriggerButton.Width -
                                                      Fonts.MainFont.MeasureString("Keyboard").X) / 2,
                    rightTriggerPosition.Y + 85),
                Color.Black);
#endif

            // Draw the title text
            titlePosition.X = plankPosition.X + (plankTexture.Width -
                                                 Fonts.MainFont.MeasureString("Gamepad").X) / 2;
            titlePosition.Y = plankPosition.Y + (plankTexture.Height -
                                                 Fonts.MainFont.MeasureString("Gamepad").Y) / 2;
            spriteBatch.DrawString(Fonts.MainFont,
                "Gamepad",
                titlePosition,
                Fonts.TitleColor);
        }
        else // draws the keyboard screen
        {
            const float spacing = 47;
            string keyboardString;

            spriteBatch.Draw(keyboardTexture, keyboardPosition, Color.White);
            for (int j = 0, i = startIndex;
                 i < startIndex + maxActionDisplay;
                 i++, j++)
            {
                keyboardString = InputManager.GetActionName((InputManager.Action) i);
                textPosition.X = chartLine1Position +
                                 (chartLine2Position - chartLine1Position -
                                  Fonts.MainFont.MeasureString(keyboardString).X) / 2;
                textPosition.Y = 253 + spacing * j;

                // Draw the action
                spriteBatch.DrawString(Fonts.MainFont,
                    keyboardString,
                    textPosition,
                    Color.Black);

                // Draw the key one
                keyboardString =
                    keyboardInfo.totalActionList[i].keyboardKeys[0].ToString();
                textPosition.X = chartLine2Position +
                                 (chartLine3Position - chartLine2Position -
                                  Fonts.MainFont.MeasureString(keyboardString).X) / 2;
                spriteBatch.DrawString(Fonts.MainFont,
                    keyboardString,
                    textPosition,
                    Color.Black);

                // Draw the key two
                if (keyboardInfo.totalActionList[i].keyboardKeys.Count > 1)
                {
                    keyboardString = keyboardInfo.totalActionList[i].keyboardKeys[1].ToString();
                    textPosition.X = chartLine3Position +
                                     (chartLine4Position - chartLine3Position -
                                      Fonts.MainFont.MeasureString(keyboardString).X) / 2;
                    spriteBatch.DrawString(Fonts.MainFont,
                        keyboardString,
                        textPosition,
                        Color.Black);
                }
                else
                {
                    textPosition.X = chartLine3Position +
                                     (chartLine4Position - chartLine3Position -
                                      Fonts.MainFont.MeasureString("---").X) / 2;
                    spriteBatch.DrawString(Fonts.MainFont,
                        "---",
                        textPosition,
                        Color.Black);
                }
            }

            // Draw the Action
            actionPosition.X = chartLine1Position +
                               (chartLine2Position - chartLine1Position -
                                Fonts.MainFont.MeasureString("Action").X) / 2;
            actionPosition.Y = 200;
            spriteBatch.DrawString(Fonts.MainFont,
                "Action",
                actionPosition,
                Fonts.CaptionColor);

            // Draw the Key 1
            key1Position.X = chartLine2Position +
                             (chartLine3Position - chartLine2Position -
                              Fonts.MainFont.MeasureString("Key 1").X) / 2;
            key1Position.Y = 200;
            spriteBatch.DrawString(Fonts.MainFont,
                "Key 1",
                key1Position,
                Fonts.CaptionColor);

            // Draw the Key 2
            key2Position.X = chartLine3Position +
                             (chartLine4Position - chartLine3Position -
                              Fonts.MainFont.MeasureString("Key 2").X) / 2;
            key2Position.Y = 200;
            spriteBatch.DrawString(Fonts.MainFont,
                "Key 2",
                key2Position,
                Fonts.CaptionColor);

            // Near left trigger
            spriteBatch.DrawString(Fonts.MainFont,
                "Gamepad",
                new Vector2(leftTriggerPosition.X + (leftTriggerButton.Width -
                                                     Fonts.MainFont.MeasureString("Gamepad").X) / 2,
                    rightTriggerPosition.Y + 85),
                Color.Black);

            // Near right trigger
            spriteBatch.DrawString(Fonts.MainFont,
                "Gamepad",
                new Vector2(rightTriggerPosition.X + (rightTriggerButton.Width -
                                                      Fonts.MainFont.MeasureString("Gamepad").X) / 2,
                    rightTriggerPosition.Y + 85),
                Color.Black);

            // Draw the title text
            titlePosition.X = plankPosition.X + (plankTexture.Width -
                                                 Fonts.MainFont.MeasureString("Keyboard").X) / 2;
            titlePosition.Y = plankPosition.Y + (plankTexture.Height -
                                                 Fonts.MainFont.MeasureString("Keyboard").Y) / 2;
            spriteBatch.DrawString(Fonts.MainFont,
                "Keyboard",
                titlePosition,
                Fonts.TitleColor);

            // Draw the scroll textures
            spriteBatch.Draw(scrollUpTexture, scrollUpPosition, Color.White);
            spriteBatch.Draw(scrollDownTexture, scrollDownPosition, Color.White);
        }

        spriteBatch.End();
    }

    #endregion

    #region Private Types

    /// <summary>
    ///     Holds the GamePad control info to display
    /// </summary>
    private struct GamePadInfo
    {
        public string text;
        public Vector2 textPosition;
    }


    /// <summary>
    ///     Holds the Keyboard control info to display
    /// </summary>
    private struct KeyboardInfo
    {
        public InputManager.ActionMap[] totalActionList;
        public int selectedIndex;
    }

    #endregion


    #region Graphics Data

    private Texture2D backgroundTexture;
    private Texture2D plankTexture;

    private Vector2 plankPosition;
    private Vector2 titlePosition;
    private Vector2 actionPosition;
    private Vector2 key1Position;
    private Vector2 key2Position;

    private Texture2D baseBorderTexture;
    private readonly Vector2 baseBorderPosition = new(200, 570);

    private Texture2D scrollUpTexture;
    private Texture2D scrollDownTexture;
    private readonly Vector2 scrollUpPosition = new(990, 235);
    private readonly Vector2 scrollDownPosition = new(990, 490);

    private Texture2D rightTriggerButton;
    private Texture2D leftTriggerButton;
    private Vector2 rightTriggerPosition;
    private Vector2 leftTriggerPosition;

    private Texture2D controlPadTexture;
    private readonly Vector2 controlPosition = new(450, 180);

    private Texture2D keyboardTexture;
    private readonly Vector2 keyboardPosition = new(305, 185);

    private readonly float chartLine1Position;
    private readonly float chartLine2Position;
    private readonly float chartLine3Position;
    private readonly float chartLine4Position;

    private Texture2D backTexture;
    private readonly Vector2 backPosition = new(225, 610);

    #endregion


    #region Control Display Data

    private bool isShowControlPad;

    private readonly GamePadInfo[] leftStrings = new GamePadInfo[7];
    private readonly GamePadInfo[] rightStrings = new GamePadInfo[6];
    private KeyboardInfo keyboardInfo;

    private int startIndex;
    private const int maxActionDisplay = 6;

    #endregion


    #region Initialization

    /// <summary>
    ///     Creates a new ControlsScreen object.
    /// </summary>
    public ControlsScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.5);

        chartLine1Position = keyboardPosition.X + 30;
        chartLine2Position = keyboardPosition.X + 340;
        chartLine3Position = keyboardPosition.X + 510;
        chartLine4Position = keyboardPosition.X + 670;

        isShowControlPad = true;
    }


    /// <summary>
    ///     Loads the graphics content required for this screen.
    /// </summary>
    public override void LoadContent()
    {
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        keyboardInfo.totalActionList = InputManager.ActionMaps;
        keyboardInfo.selectedIndex = 0;

        const int leftStringsPosition = 450;
        const int rightStringPosition = 818;

        float height = Fonts.MainFont.LineSpacing - 5;

        // Set the data for gamepad control to display
        leftStrings[0].text = "Page Left";
        leftStrings[0].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[0].text).X,
            170);

        leftStrings[1].text = "N/A";
        leftStrings[1].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[1].text).X,
            220);

        leftStrings[2].text = "Main Menu";
        leftStrings[2].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[2].text).X,
            290);

        leftStrings[3].text = "Exit Game";
        leftStrings[3].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[3].text).X,
            340);

        leftStrings[4].text = "Navigation";
        leftStrings[4].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[4].text).X,
            400);

        leftStrings[5].text = "Navigation";
        leftStrings[5].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[5].text).X,
            455);

        leftStrings[6].text = "N/A";
        leftStrings[6].textPosition = new Vector2(leftStringsPosition -
                                                  Fonts.MainFont.MeasureString(leftStrings[6].text).X,
            510);


        rightStrings[0].text = "Page Right";
        rightStrings[0].textPosition = new Vector2(rightStringPosition, 170);

        rightStrings[1].text = "N/A";
        rightStrings[1].textPosition = new Vector2(rightStringPosition, 230);

        rightStrings[2].text = "Character Management";
        rightStrings[2].textPosition = new Vector2(rightStringPosition, 295);

        rightStrings[3].text = "Back";
        rightStrings[3].textPosition = new Vector2(rightStringPosition, 355);

        rightStrings[4].text = "OK";
        rightStrings[4].textPosition = new Vector2(rightStringPosition, 435);

        rightStrings[5].text = "Drop Gear";
        rightStrings[5].textPosition = new Vector2(rightStringPosition, 510);

        var content = ScreenManager.Game.Content;
        backgroundTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\MainMenu");
        keyboardTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\KeyboardBkgd");
        plankTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
        backTexture =
            content.Load<Texture2D>(@"Textures\Buttons\BButton");
        baseBorderTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");
        controlPadTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\ControlJoystick");
        scrollUpTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\ScrollUp");
        scrollDownTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\ScrollDown");
        rightTriggerButton =
            content.Load<Texture2D>(@"Textures\Buttons\RightTriggerButton");
        leftTriggerButton =
            content.Load<Texture2D>(@"Textures\Buttons\LeftTriggerButton");

        plankPosition.X = backgroundTexture.Width / 2 - plankTexture.Width / 2;
        plankPosition.Y = 60;

        rightTriggerPosition.X = 900;
        rightTriggerPosition.Y = 50;

        leftTriggerPosition.X = 320;
        leftTriggerPosition.Y = 50;

        base.LoadContent();
    }

    #endregion
}
