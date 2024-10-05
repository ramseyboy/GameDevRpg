#region File Description

//-----------------------------------------------------------------------------
// StatisticsValueStack.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;

#endregion

namespace RolePlayingGameData.Data;

/// <summary>
///     A collection of statistics that expire over time.
/// </summary>
public class StatisticsValueStack
{
    /// <summary>
    ///     Add a new statistics, with a given duration, to the stack.
    /// </summary>
    /// <remarks>Entries with durations of 0 or less never expire.</remarks>
    public void AddStatistics(StatisticsValue statistics, int duration)
    {
        if (duration < 0)
        {
            throw new ArgumentOutOfRangeException("duration");
        }

        var entry = new StatisticsValueStackEntry();
        entry.Statistics = statistics;
        entry.RemainingDuration = duration;

        entries.Add(entry);

        CalculateTotalStatistics();
    }


    /// <summary>
    ///     Advance the stack and remove expired entries.
    /// </summary>
    public void Advance()
    {
        // remove the entries at 1 - they are about to go to zero
        // -- values that are zero now, never expire
        entries.RemoveAll(delegate(StatisticsValueStackEntry entry) { return entry.RemainingDuration == 1; });

        // decrement all of the remaining entries.
        foreach (var entry in entries)
        {
            entry.RemainingDuration--;
        }

        // recalculate the total
        CalculateTotalStatistics();
    }

    #region Entry List

    /// <summary>
    ///     One entry in the stack.
    /// </summary>
    private class StatisticsValueStackEntry
    {
        public int RemainingDuration;
        public StatisticsValue Statistics;
    }

    /// <summary>
    ///     One entry in the stack.
    /// </summary>
    private readonly List<StatisticsValueStackEntry> entries = new();

    #endregion


    #region Totals

    /// <summary>
    ///     The total of all unexpired statistics in the stack.
    /// </summary>
    private StatisticsValue totalStatistics;

    /// <summary>
    ///     The total of all unexpired statistics in the stack.
    /// </summary>
    public StatisticsValue TotalStatistics => totalStatistics;


    /// <summary>
    ///     Calculate the total of all unexpired entries.
    /// </summary>
    private void CalculateTotalStatistics()
    {
        totalStatistics = new StatisticsValue();
        foreach (var entry in entries)
        {
            totalStatistics += entry.Statistics;
        }
    }

    #endregion
}
