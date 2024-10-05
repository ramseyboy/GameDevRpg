#region File Description

//-----------------------------------------------------------------------------
// CharacterLevelingStatistics.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData;

/// <summary>
///     Information about how to increment statistics with additional character levels.
/// </summary>
#if !XBOX
[Serializable]
#endif
public struct CharacterLevelingStatistics
{
    /// <summary>
    ///     The amount that the character's health points will increase.
    /// </summary>
    public int HealthPointsIncrease;

    /// <summary>
    ///     The number of levels between each health point increase.
    /// </summary>
    public int LevelsPerHealthPointsIncrease;


    /// <summary>
    ///     The amount that the character's magic points will increase.
    /// </summary>
    public int MagicPointsIncrease;

    /// <summary>
    ///     The number of levels between each magic point increase.
    /// </summary>
    public int LevelsPerMagicPointsIncrease;


    /// <summary>
    ///     The amount that the character's physical offense will increase.
    /// </summary>
    public int PhysicalOffenseIncrease;

    /// <summary>
    ///     The number of levels between each physical offense increase.
    /// </summary>
    public int LevelsPerPhysicalOffenseIncrease;


    /// <summary>
    ///     The amount that the character's physical defense will increase.
    /// </summary>
    public int PhysicalDefenseIncrease;

    /// <summary>
    ///     The number of levels between each physical defense increase.
    /// </summary>
    public int LevelsPerPhysicalDefenseIncrease;


    /// <summary>
    ///     The amount that the character's magical offense will increase.
    /// </summary>
    public int MagicalOffenseIncrease;

    /// <summary>
    ///     The number of levels between each magical offense increase.
    /// </summary>
    public int LevelsPerMagicalOffenseIncrease;


    /// <summary>
    ///     The amount that the character's magical defense will increase.
    /// </summary>
    public int MagicalDefenseIncrease;

    /// <summary>
    ///     The number of levels between each magical defense increase.
    /// </summary>
    public int LevelsPerMagicalDefenseIncrease;


    #region Content Type Reader

    /// <summary>
    ///     Reads a CharacterLevelingStatistics object from the content pipeline.
    /// </summary>
    public class CharacterLevelingStatisticsReader :
        ContentTypeReader<CharacterLevelingStatistics>
    {
        /// <summary>
        ///     Reads a CharacterLevelingStatistics object from the content pipeline.
        /// </summary>
        protected override CharacterLevelingStatistics Read(ContentReader input,
            CharacterLevelingStatistics existingInstance)
        {
            var stats = existingInstance;

            stats.HealthPointsIncrease = input.ReadInt32();
            stats.MagicPointsIncrease = input.ReadInt32();
            stats.PhysicalOffenseIncrease = input.ReadInt32();
            stats.PhysicalDefenseIncrease = input.ReadInt32();
            stats.MagicalOffenseIncrease = input.ReadInt32();
            stats.MagicalDefenseIncrease = input.ReadInt32();

            stats.LevelsPerHealthPointsIncrease = input.ReadInt32();
            stats.LevelsPerMagicPointsIncrease = input.ReadInt32();
            stats.LevelsPerPhysicalOffenseIncrease = input.ReadInt32();
            stats.LevelsPerPhysicalDefenseIncrease = input.ReadInt32();
            stats.LevelsPerMagicalOffenseIncrease = input.ReadInt32();
            stats.LevelsPerMagicalDefenseIncrease = input.ReadInt32();

            return stats;
        }
    }

    #endregion
}
