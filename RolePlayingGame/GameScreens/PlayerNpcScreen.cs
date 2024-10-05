#region File Description

//-----------------------------------------------------------------------------
// PlayerNpcScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using RolePlayingGameData;
using RolePlayingGameData.Characters;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Displays the Player NPC screen, shown when encountering a player on the map.
///     Typically, the user has an opportunity to invite the Player into the party.
/// </summary>
internal class PlayerNpcScreen : NpcScreen<Player>
{
    /// <summary>
    ///     If true, the NPC's introduction dialogue is shown.
    /// </summary>
    private bool isIntroduction = true;


    /// <summary>
    ///     Constructs a new PlayerNpcScreen object.
    /// </summary>
    /// <param name="mapEntry"></param>
    public PlayerNpcScreen(MapEntry<Player> mapEntry)
        : base(mapEntry)
    {
        // assign and check the parameter
        var playerNpc = character as Player;
        if (playerNpc == null)
        {
            throw new ArgumentException(
                "PlayerNpcScreen requires a MapEntry with a Player");
        }

        DialogueText = playerNpc.IntroductionDialogue;
        BackText = "Reject";
        SelectText = "Accept";
        isIntroduction = true;
    }


    /// <summary>
    ///     Handles user input.
    /// </summary>
    public override void HandleInput()
    {
        // view the player's statistics
        if (InputManager.IsActionTriggered(InputManager.Action.TakeView))
        {
            ScreenManager.AddScreen(new StatisticsScreen(character as Player));
            return;
        }

        if (isIntroduction)
        {
            // accept the invitation
            if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                isIntroduction = false;
                var player = character as Player;
                Session.Session.Party.JoinParty(player);
                Session.Session.RemovePlayerNpc(mapEntry);
                DialogueText = player.JoinAcceptedDialogue;
                BackText = "Back";
                SelectText = "Back";
            }

            // reject the invitation
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                isIntroduction = false;
                var player = character as Player;
                DialogueText = player.JoinRejectedDialogue;
                BackText = "Back";
                SelectText = "Back";
            }
        }
        else
        {
            // exit the screen
            if (InputManager.IsActionTriggered(InputManager.Action.Ok) ||
                InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                ExitScreen();
            }
        }
    }
}
