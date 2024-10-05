#region File Description

//-----------------------------------------------------------------------------
// Equipment.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content;
using RolePlayingGameData.Data;

#endregion

namespace RolePlayingGameData.Gear;

/// <summary>
///     Gear that may be equipped onto a FightingCharacter.
/// </summary>
public abstract class Equipment : Gear
{
    #region Owner Buff

    /// <summary>
    ///     The statistics buff applied by this equipment to its owner.
    /// </summary>
    /// <remarks>Buff values are positive, and will be added.</remarks>
    private StatisticsValue ownerBuffStatistics;

    /// <summary>
    ///     The statistics buff applied by this equipment to its owner.
    /// </summary>
    /// <remarks>Buff values are positive, and will be added.</remarks>
    [ContentSerializer(Optional = true)]
    public StatisticsValue OwnerBuffStatistics
    {
        get => ownerBuffStatistics;
        set => ownerBuffStatistics = value;
    }

    #endregion
}
