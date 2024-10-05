#region File Description

//-----------------------------------------------------------------------------
// InputManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace RolePlayingGame;

/// <summary>
///     This class handles all keyboard and gamepad actions in the game.
/// </summary>
public static class InputManager
{
    #region Constants

    /// <summary>
    ///     The value of an analog control that reads as a "pressed button".
    /// </summary>
    private const float analogLimit = 0.5f;

    #endregion


    #region Initialization

    /// <summary>
    ///     Initializes the default control keys for all actions.
    /// </summary>
    public static void Initialize()
    {
        ResetActionMaps();
    }

    #endregion


    #region Updating

    /// <summary>
    ///     Updates the keyboard and gamepad control states.
    /// </summary>
    public static void Update()
    {
        // update the keyboard state
        previousKeyboardState = currentKeyboardState;
        currentKeyboardState = Keyboard.GetState();

        // update the gamepad state
        previousGamePadState = currentGamePadState;
        currentGamePadState = GamePad.GetState(PlayerIndex.One);
    }

    #endregion

    #region Action Enumeration

    /// <summary>
    ///     The actions that are possible within the game.
    /// </summary>
    public enum Action
    {
        MainMenu,
        Ok,
        Back,
        CharacterManagement,
        ExitGame,
        TakeView,
        DropUnEquip,
        MoveCharacterUp,
        MoveCharacterDown,
        MoveCharacterLeft,
        MoveCharacterRight,
        CursorUp,
        CursorDown,
        DecreaseAmount,
        IncreaseAmount,
        PageLeft,
        PageRight,
        TargetUp,
        TargetDown,
        ActiveCharacterLeft,
        ActiveCharacterRight,
        TotalActionCount
    }


    /// <summary>
    ///     Readable names of each action.
    /// </summary>
    private static readonly string[] actionNames =
    {
        "Main Menu",
        "Ok",
        "Back",
        "Character Management",
        "Exit Game",
        "Take / View",
        "Drop / Unequip",
        "Move Character - Up",
        "Move Character - Down",
        "Move Character - Left",
        "Move Character - Right",
        "Move Cursor - Up",
        "Move Cursor - Down",
        "Decrease Amount",
        "Increase Amount",
        "Page Screen Left",
        "Page Screen Right",
        "Select Target -Up",
        "Select Target - Down",
        "Select Active Character - Left",
        "Select Active Character - Right"
    };

    /// <summary>
    ///     Returns the readable name of the given action.
    /// </summary>
    public static string GetActionName(Action action)
    {
        var index = (int) action;

        if (index < 0 || index > actionNames.Length)
        {
            throw new ArgumentException("action");
        }

        return actionNames[index];
    }

    #endregion


    #region Support Types

    /// <summary>
    ///     GamePad controls expressed as one type, unified with button semantics.
    /// </summary>
    public enum GamePadButtons
    {
        Start,
        Back,
        A,
        B,
        X,
        Y,
        Up,
        Down,
        Left,
        Right,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger
    }


    /// <summary>
    ///     A combination of gamepad and keyboard keys mapped to a particular action.
    /// </summary>
    public class ActionMap
    {
        /// <summary>
        ///     List of GamePad controls to be mapped to a given action.
        /// </summary>
        public List<GamePadButtons> gamePadButtons = new();


        /// <summary>
        ///     List of Keyboard controls to be mapped to a given action.
        /// </summary>
        public List<Keys> keyboardKeys = new();
    }

    #endregion


    #region Keyboard Data

    /// <summary>
    ///     The state of the keyboard as of the last update.
    /// </summary>
    private static KeyboardState currentKeyboardState;

    /// <summary>
    ///     The state of the keyboard as of the last update.
    /// </summary>
    public static KeyboardState CurrentKeyboardState => currentKeyboardState;


    /// <summary>
    ///     The state of the keyboard as of the previous update.
    /// </summary>
    private static KeyboardState previousKeyboardState;


    /// <summary>
    ///     Check if a key is pressed.
    /// </summary>
    public static bool IsKeyPressed(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key);
    }


    /// <summary>
    ///     Check if a key was just pressed in the most recent update.
    /// </summary>
    public static bool IsKeyTriggered(Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) &&
               !previousKeyboardState.IsKeyDown(key);
    }

    #endregion


    #region GamePad Data

    /// <summary>
    ///     The state of the gamepad as of the last update.
    /// </summary>
    private static GamePadState currentGamePadState;

    /// <summary>
    ///     The state of the gamepad as of the last update.
    /// </summary>
    public static GamePadState CurrentGamePadState => currentGamePadState;


    /// <summary>
    ///     The state of the gamepad as of the previous update.
    /// </summary>
    private static GamePadState previousGamePadState;


    #region GamePadButton Pressed Queries

    /// <summary>
    ///     Check if the gamepad's Start button is pressed.
    /// </summary>
    public static bool IsGamePadStartPressed()
    {
        return currentGamePadState.Buttons.Start == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's Back button is pressed.
    /// </summary>
    public static bool IsGamePadBackPressed()
    {
        return currentGamePadState.Buttons.Back == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's A button is pressed.
    /// </summary>
    public static bool IsGamePadAPressed()
    {
        return currentGamePadState.Buttons.A == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's B button is pressed.
    /// </summary>
    public static bool IsGamePadBPressed()
    {
        return currentGamePadState.Buttons.B == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's X button is pressed.
    /// </summary>
    public static bool IsGamePadXPressed()
    {
        return currentGamePadState.Buttons.X == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's Y button is pressed.
    /// </summary>
    public static bool IsGamePadYPressed()
    {
        return currentGamePadState.Buttons.Y == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's LeftShoulder button is pressed.
    /// </summary>
    public static bool IsGamePadLeftShoulderPressed()
    {
        return currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed;
    }


    /// <summary>
    ///     <summary>
    ///         Check if the gamepad's RightShoulder button is pressed.
    ///     </summary>
    public static bool IsGamePadRightShoulderPressed()
    {
        return currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if Up on the gamepad's directional pad is pressed.
    /// </summary>
    public static bool IsGamePadDPadUpPressed()
    {
        return currentGamePadState.DPad.Up == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if Down on the gamepad's directional pad is pressed.
    /// </summary>
    public static bool IsGamePadDPadDownPressed()
    {
        return currentGamePadState.DPad.Down == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if Left on the gamepad's directional pad is pressed.
    /// </summary>
    public static bool IsGamePadDPadLeftPressed()
    {
        return currentGamePadState.DPad.Left == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if Right on the gamepad's directional pad is pressed.
    /// </summary>
    public static bool IsGamePadDPadRightPressed()
    {
        return currentGamePadState.DPad.Right == ButtonState.Pressed;
    }


    /// <summary>
    ///     Check if the gamepad's left trigger is pressed.
    /// </summary>
    public static bool IsGamePadLeftTriggerPressed()
    {
        return currentGamePadState.Triggers.Left > analogLimit;
    }


    /// <summary>
    ///     Check if the gamepad's right trigger is pressed.
    /// </summary>
    public static bool IsGamePadRightTriggerPressed()
    {
        return currentGamePadState.Triggers.Right > analogLimit;
    }


    /// <summary>
    ///     Check if Up on the gamepad's left analog stick is pressed.
    /// </summary>
    public static bool IsGamePadLeftStickUpPressed()
    {
        return currentGamePadState.ThumbSticks.Left.Y > analogLimit;
    }


    /// <summary>
    ///     Check if Down on the gamepad's left analog stick is pressed.
    /// </summary>
    public static bool IsGamePadLeftStickDownPressed()
    {
        return -1f * currentGamePadState.ThumbSticks.Left.Y > analogLimit;
    }


    /// <summary>
    ///     Check if Left on the gamepad's left analog stick is pressed.
    /// </summary>
    public static bool IsGamePadLeftStickLeftPressed()
    {
        return -1f * currentGamePadState.ThumbSticks.Left.X > analogLimit;
    }


    /// <summary>
    ///     Check if Right on the gamepad's left analog stick is pressed.
    /// </summary>
    public static bool IsGamePadLeftStickRightPressed()
    {
        return currentGamePadState.ThumbSticks.Left.X > analogLimit;
    }


    /// <summary>
    ///     Check if the GamePadKey value specified is pressed.
    /// </summary>
    private static bool IsGamePadButtonPressed(GamePadButtons gamePadKey)
    {
        switch (gamePadKey)
        {
            case GamePadButtons.Start:
                return IsGamePadStartPressed();

            case GamePadButtons.Back:
                return IsGamePadBackPressed();

            case GamePadButtons.A:
                return IsGamePadAPressed();

            case GamePadButtons.B:
                return IsGamePadBPressed();

            case GamePadButtons.X:
                return IsGamePadXPressed();

            case GamePadButtons.Y:
                return IsGamePadYPressed();

            case GamePadButtons.LeftShoulder:
                return IsGamePadLeftShoulderPressed();

            case GamePadButtons.RightShoulder:
                return IsGamePadRightShoulderPressed();

            case GamePadButtons.LeftTrigger:
                return IsGamePadLeftTriggerPressed();

            case GamePadButtons.RightTrigger:
                return IsGamePadRightTriggerPressed();

            case GamePadButtons.Up:
                return IsGamePadDPadUpPressed() ||
                       IsGamePadLeftStickUpPressed();

            case GamePadButtons.Down:
                return IsGamePadDPadDownPressed() ||
                       IsGamePadLeftStickDownPressed();

            case GamePadButtons.Left:
                return IsGamePadDPadLeftPressed() ||
                       IsGamePadLeftStickLeftPressed();

            case GamePadButtons.Right:
                return IsGamePadDPadRightPressed() ||
                       IsGamePadLeftStickRightPressed();
        }

        return false;
    }

    #endregion


    #region GamePadButton Triggered Queries

    /// <summary>
    ///     Check if the gamepad's Start button was just pressed.
    /// </summary>
    public static bool IsGamePadStartTriggered()
    {
        return currentGamePadState.Buttons.Start == ButtonState.Pressed &&
               previousGamePadState.Buttons.Start == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's Back button was just pressed.
    /// </summary>
    public static bool IsGamePadBackTriggered()
    {
        return currentGamePadState.Buttons.Back == ButtonState.Pressed &&
               previousGamePadState.Buttons.Back == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's A button was just pressed.
    /// </summary>
    public static bool IsGamePadATriggered()
    {
        return currentGamePadState.Buttons.A == ButtonState.Pressed &&
               previousGamePadState.Buttons.A == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's B button was just pressed.
    /// </summary>
    public static bool IsGamePadBTriggered()
    {
        return currentGamePadState.Buttons.B == ButtonState.Pressed &&
               previousGamePadState.Buttons.B == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's X button was just pressed.
    /// </summary>
    public static bool IsGamePadXTriggered()
    {
        return currentGamePadState.Buttons.X == ButtonState.Pressed &&
               previousGamePadState.Buttons.X == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's Y button was just pressed.
    /// </summary>
    public static bool IsGamePadYTriggered()
    {
        return currentGamePadState.Buttons.Y == ButtonState.Pressed &&
               previousGamePadState.Buttons.Y == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's LeftShoulder button was just pressed.
    /// </summary>
    public static bool IsGamePadLeftShoulderTriggered()
    {
        return currentGamePadState.Buttons.LeftShoulder == ButtonState.Pressed &&
               previousGamePadState.Buttons.LeftShoulder == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's RightShoulder button was just pressed.
    /// </summary>
    public static bool IsGamePadRightShoulderTriggered()
    {
        return currentGamePadState.Buttons.RightShoulder == ButtonState.Pressed &&
               previousGamePadState.Buttons.RightShoulder == ButtonState.Released;
    }


    /// <summary>
    ///     Check if Up on the gamepad's directional pad was just pressed.
    /// </summary>
    public static bool IsGamePadDPadUpTriggered()
    {
        return currentGamePadState.DPad.Up == ButtonState.Pressed &&
               previousGamePadState.DPad.Up == ButtonState.Released;
    }


    /// <summary>
    ///     Check if Down on the gamepad's directional pad was just pressed.
    /// </summary>
    public static bool IsGamePadDPadDownTriggered()
    {
        return currentGamePadState.DPad.Down == ButtonState.Pressed &&
               previousGamePadState.DPad.Down == ButtonState.Released;
    }


    /// <summary>
    ///     Check if Left on the gamepad's directional pad was just pressed.
    /// </summary>
    public static bool IsGamePadDPadLeftTriggered()
    {
        return currentGamePadState.DPad.Left == ButtonState.Pressed &&
               previousGamePadState.DPad.Left == ButtonState.Released;
    }


    /// <summary>
    ///     Check if Right on the gamepad's directional pad was just pressed.
    /// </summary>
    public static bool IsGamePadDPadRightTriggered()
    {
        return currentGamePadState.DPad.Right == ButtonState.Pressed &&
               previousGamePadState.DPad.Right == ButtonState.Released;
    }


    /// <summary>
    ///     Check if the gamepad's left trigger was just pressed.
    /// </summary>
    public static bool IsGamePadLeftTriggerTriggered()
    {
        return currentGamePadState.Triggers.Left > analogLimit &&
               previousGamePadState.Triggers.Left < analogLimit;
    }


    /// <summary>
    ///     Check if the gamepad's right trigger was just pressed.
    /// </summary>
    public static bool IsGamePadRightTriggerTriggered()
    {
        return currentGamePadState.Triggers.Right > analogLimit &&
               previousGamePadState.Triggers.Right < analogLimit;
    }


    /// <summary>
    ///     Check if Up on the gamepad's left analog stick was just pressed.
    /// </summary>
    public static bool IsGamePadLeftStickUpTriggered()
    {
        return currentGamePadState.ThumbSticks.Left.Y > analogLimit &&
               previousGamePadState.ThumbSticks.Left.Y < analogLimit;
    }


    /// <summary>
    ///     Check if Down on the gamepad's left analog stick was just pressed.
    /// </summary>
    public static bool IsGamePadLeftStickDownTriggered()
    {
        return -1f * currentGamePadState.ThumbSticks.Left.Y > analogLimit &&
               -1f * previousGamePadState.ThumbSticks.Left.Y < analogLimit;
    }


    /// <summary>
    ///     Check if Left on the gamepad's left analog stick was just pressed.
    /// </summary>
    public static bool IsGamePadLeftStickLeftTriggered()
    {
        return -1f * currentGamePadState.ThumbSticks.Left.X > analogLimit &&
               -1f * previousGamePadState.ThumbSticks.Left.X < analogLimit;
    }


    /// <summary>
    ///     Check if Right on the gamepad's left analog stick was just pressed.
    /// </summary>
    public static bool IsGamePadLeftStickRightTriggered()
    {
        return currentGamePadState.ThumbSticks.Left.X > analogLimit &&
               previousGamePadState.ThumbSticks.Left.X < analogLimit;
    }


    /// <summary>
    ///     Check if the GamePadKey value specified was pressed this frame.
    /// </summary>
    private static bool IsGamePadButtonTriggered(GamePadButtons gamePadKey)
    {
        switch (gamePadKey)
        {
            case GamePadButtons.Start:
                return IsGamePadStartTriggered();

            case GamePadButtons.Back:
                return IsGamePadBackTriggered();

            case GamePadButtons.A:
                return IsGamePadATriggered();

            case GamePadButtons.B:
                return IsGamePadBTriggered();

            case GamePadButtons.X:
                return IsGamePadXTriggered();

            case GamePadButtons.Y:
                return IsGamePadYTriggered();

            case GamePadButtons.LeftShoulder:
                return IsGamePadLeftShoulderTriggered();

            case GamePadButtons.RightShoulder:
                return IsGamePadRightShoulderTriggered();

            case GamePadButtons.LeftTrigger:
                return IsGamePadLeftTriggerTriggered();

            case GamePadButtons.RightTrigger:
                return IsGamePadRightTriggerTriggered();

            case GamePadButtons.Up:
                return IsGamePadDPadUpTriggered() ||
                       IsGamePadLeftStickUpTriggered();

            case GamePadButtons.Down:
                return IsGamePadDPadDownTriggered() ||
                       IsGamePadLeftStickDownTriggered();

            case GamePadButtons.Left:
                return IsGamePadDPadLeftTriggered() ||
                       IsGamePadLeftStickLeftTriggered();

            case GamePadButtons.Right:
                return IsGamePadDPadRightTriggered() ||
                       IsGamePadLeftStickRightTriggered();
        }

        return false;
    }

    #endregion

    #endregion


    #region Action Mapping

    /// <summary>
    ///     The action mappings for the game.
    /// </summary>
    public static ActionMap[] ActionMaps { get; private set; }


    /// <summary>
    ///     Reset the action maps to their default values.
    /// </summary>
    private static void ResetActionMaps()
    {
        ActionMaps = new ActionMap[(int) Action.TotalActionCount];

        ActionMaps[(int) Action.MainMenu] = new ActionMap();
        ActionMaps[(int) Action.MainMenu].keyboardKeys.Add(
            Keys.Tab);
        ActionMaps[(int) Action.MainMenu].gamePadButtons.Add(
            GamePadButtons.Start);

        ActionMaps[(int) Action.Ok] = new ActionMap();
        ActionMaps[(int) Action.Ok].keyboardKeys.Add(
            Keys.Enter);
        ActionMaps[(int) Action.Ok].gamePadButtons.Add(
            GamePadButtons.A);

        ActionMaps[(int) Action.Back] = new ActionMap();
        ActionMaps[(int) Action.Back].keyboardKeys.Add(
            Keys.Escape);
        ActionMaps[(int) Action.Back].gamePadButtons.Add(
            GamePadButtons.B);

        ActionMaps[(int) Action.CharacterManagement] = new ActionMap();
        ActionMaps[(int) Action.CharacterManagement].keyboardKeys.Add(
            Keys.Space);
        ActionMaps[(int) Action.CharacterManagement].gamePadButtons.Add(
            GamePadButtons.Y);

        ActionMaps[(int) Action.ExitGame] = new ActionMap();
        ActionMaps[(int) Action.ExitGame].keyboardKeys.Add(
            Keys.Escape);
        ActionMaps[(int) Action.ExitGame].gamePadButtons.Add(
            GamePadButtons.Back);

        ActionMaps[(int) Action.TakeView] = new ActionMap();
        ActionMaps[(int) Action.TakeView].keyboardKeys.Add(
            Keys.LeftControl);
        ActionMaps[(int) Action.TakeView].gamePadButtons.Add(
            GamePadButtons.Y);

        ActionMaps[(int) Action.DropUnEquip] = new ActionMap();
        ActionMaps[(int) Action.DropUnEquip].keyboardKeys.Add(
            Keys.D);
        ActionMaps[(int) Action.DropUnEquip].gamePadButtons.Add(
            GamePadButtons.X);

        ActionMaps[(int) Action.MoveCharacterUp] = new ActionMap();
        ActionMaps[(int) Action.MoveCharacterUp].keyboardKeys.Add(
            Keys.Up);
        ActionMaps[(int) Action.MoveCharacterUp].gamePadButtons.Add(
            GamePadButtons.Up);

        ActionMaps[(int) Action.MoveCharacterDown] = new ActionMap();
        ActionMaps[(int) Action.MoveCharacterDown].keyboardKeys.Add(
            Keys.Down);
        ActionMaps[(int) Action.MoveCharacterDown].gamePadButtons.Add(
            GamePadButtons.Down);

        ActionMaps[(int) Action.MoveCharacterLeft] = new ActionMap();
        ActionMaps[(int) Action.MoveCharacterLeft].keyboardKeys.Add(
            Keys.Left);
        ActionMaps[(int) Action.MoveCharacterLeft].gamePadButtons.Add(
            GamePadButtons.Left);

        ActionMaps[(int) Action.MoveCharacterRight] = new ActionMap();
        ActionMaps[(int) Action.MoveCharacterRight].keyboardKeys.Add(
            Keys.Right);
        ActionMaps[(int) Action.MoveCharacterRight].gamePadButtons.Add(
            GamePadButtons.Right);

        ActionMaps[(int) Action.CursorUp] = new ActionMap();
        ActionMaps[(int) Action.CursorUp].keyboardKeys.Add(
            Keys.Up);
        ActionMaps[(int) Action.CursorUp].gamePadButtons.Add(
            GamePadButtons.Up);

        ActionMaps[(int) Action.CursorDown] = new ActionMap();
        ActionMaps[(int) Action.CursorDown].keyboardKeys.Add(
            Keys.Down);
        ActionMaps[(int) Action.CursorDown].gamePadButtons.Add(
            GamePadButtons.Down);

        ActionMaps[(int) Action.DecreaseAmount] = new ActionMap();
        ActionMaps[(int) Action.DecreaseAmount].keyboardKeys.Add(
            Keys.Left);
        ActionMaps[(int) Action.DecreaseAmount].gamePadButtons.Add(
            GamePadButtons.Left);

        ActionMaps[(int) Action.IncreaseAmount] = new ActionMap();
        ActionMaps[(int) Action.IncreaseAmount].keyboardKeys.Add(
            Keys.Right);
        ActionMaps[(int) Action.IncreaseAmount].gamePadButtons.Add(
            GamePadButtons.Right);

        ActionMaps[(int) Action.PageLeft] = new ActionMap();
        ActionMaps[(int) Action.PageLeft].keyboardKeys.Add(
            Keys.LeftShift);
        ActionMaps[(int) Action.PageLeft].gamePadButtons.Add(
            GamePadButtons.LeftTrigger);

        ActionMaps[(int) Action.PageRight] = new ActionMap();
        ActionMaps[(int) Action.PageRight].keyboardKeys.Add(
            Keys.RightShift);
        ActionMaps[(int) Action.PageRight].gamePadButtons.Add(
            GamePadButtons.RightTrigger);

        ActionMaps[(int) Action.TargetUp] = new ActionMap();
        ActionMaps[(int) Action.TargetUp].keyboardKeys.Add(
            Keys.Up);
        ActionMaps[(int) Action.TargetUp].gamePadButtons.Add(
            GamePadButtons.Up);

        ActionMaps[(int) Action.TargetDown] = new ActionMap();
        ActionMaps[(int) Action.TargetDown].keyboardKeys.Add(
            Keys.Down);
        ActionMaps[(int) Action.TargetDown].gamePadButtons.Add(
            GamePadButtons.Down);

        ActionMaps[(int) Action.ActiveCharacterLeft] = new ActionMap();
        ActionMaps[(int) Action.ActiveCharacterLeft].keyboardKeys.Add(
            Keys.Left);
        ActionMaps[(int) Action.ActiveCharacterLeft].gamePadButtons.Add(
            GamePadButtons.Left);

        ActionMaps[(int) Action.ActiveCharacterRight] = new ActionMap();
        ActionMaps[(int) Action.ActiveCharacterRight].keyboardKeys.Add(
            Keys.Right);
        ActionMaps[(int) Action.ActiveCharacterRight].gamePadButtons.Add(
            GamePadButtons.Right);
    }


    /// <summary>
    ///     Check if an action has been pressed.
    /// </summary>
    public static bool IsActionPressed(Action action)
    {
        return IsActionMapPressed(ActionMaps[(int) action]);
    }


    /// <summary>
    ///     Check if an action was just performed in the most recent update.
    /// </summary>
    public static bool IsActionTriggered(Action action)
    {
        return IsActionMapTriggered(ActionMaps[(int) action]);
    }


    /// <summary>
    ///     Check if an action map has been pressed.
    /// </summary>
    private static bool IsActionMapPressed(ActionMap actionMap)
    {
        for (var i = 0; i < actionMap.keyboardKeys.Count; i++)
        {
            if (IsKeyPressed(actionMap.keyboardKeys[i]))
            {
                return true;
            }
        }

        if (currentGamePadState.IsConnected)
        {
            for (var i = 0; i < actionMap.gamePadButtons.Count; i++)
            {
                if (IsGamePadButtonPressed(actionMap.gamePadButtons[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }


    /// <summary>
    ///     Check if an action map has been triggered this frame.
    /// </summary>
    private static bool IsActionMapTriggered(ActionMap actionMap)
    {
        for (var i = 0; i < actionMap.keyboardKeys.Count; i++)
        {
            if (IsKeyTriggered(actionMap.keyboardKeys[i]))
            {
                return true;
            }
        }

        if (currentGamePadState.IsConnected)
        {
            for (var i = 0; i < actionMap.gamePadButtons.Count; i++)
            {
                if (IsGamePadButtonTriggered(actionMap.gamePadButtons[i]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion
}
