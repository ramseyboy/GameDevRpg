#region File Description

//-----------------------------------------------------------------------------
// PartySaveData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using RolePlayingGameData;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGame.Session;

/// <summary>
///     Serializable data for the state of the party.
/// </summary>
public class PartySaveData
{
    /// <summary>
    ///     The asset names of all party gear.
    /// </summary>
    public List<ContentEntry<Gear>> inventory = new();

    /// <summary>
    ///     The kill-counts of the monsters killed so far during the active quest.
    /// </summary>
    /// <remarks>
    ///     Dictionaries don't serialize easily, so the party data is stored separately.
    /// </remarks>
    public List<int> monsterKillCounts = new();

    /// <summary>
    ///     The names of the monsters killed so far during the active quest.
    /// </summary>
    /// <remarks>
    ///     Dictionaries don't serialize easily, so the party data is stored separately.
    /// </remarks>
    public List<string> monsterKillNames = new();

    /// <summary>
    ///     The amount of gold held by the party.
    /// </summary>
    public int partyGold;

    /// <summary>
    ///     The serializable data for all party members.
    /// </summary>
    public List<PlayerSaveData> players = new();


    #region Initialization

    /// <summary>
    ///     Creates a new PartyData object.
    /// </summary>
    public PartySaveData()
    {
    }


    /// <summary>
    ///     Creates a new PartyData object from the given Party object.
    /// </summary>
    public PartySaveData(Party party)
        : this()
    {
        // check the parameter
        if (party == null)
        {
            throw new ArgumentNullException("party");
        }

        // create and add the serializable player data
        foreach (var player in party.Players)
        {
            players.Add(new PlayerSaveData(player));
        }

        // add the items
        inventory.AddRange(party.Inventory);

        // store the amount of gold held by the party
        partyGold = party.PartyGold;

        // add the monster kill data for the active quest
        foreach (var monsterName in party.MonsterKills.Keys)
        {
            monsterKillNames.Add(monsterName);
            monsterKillCounts.Add(party.MonsterKills[monsterName]);
        }
    }

    #endregion
}
