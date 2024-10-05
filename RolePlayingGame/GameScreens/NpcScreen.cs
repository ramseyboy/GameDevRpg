#region File Description

//-----------------------------------------------------------------------------
// NpcScreen.cs
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
///     Display of conversation dialog between the player and the npc
/// </summary>
internal abstract class NpcScreen<T> : DialogueScreen where T : Character
{
    protected Character character;
    protected MapEntry<T> mapEntry;


    #region Initialization

    /// <summary>
    ///     Create a new NpcScreen object.
    /// </summary>
    /// <param name="mapEntry"></param>
    public NpcScreen(MapEntry<T> mapEntry)
    {
        if (mapEntry == null)
        {
            throw new ArgumentNullException("mapEntry");
        }

        this.mapEntry = mapEntry;
        character = mapEntry.Content;
        if (character == null)
        {
            throw new ArgumentNullException(
                "NpcScreen requires a MapEntry with a character.");
        }

        TitleText = character.Name;
    }

    #endregion
}
