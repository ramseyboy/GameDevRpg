#region File Description

//-----------------------------------------------------------------------------
// StatisticsRangeWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Data;

#endregion

namespace RolePlayingGameProcessors.Data;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class StatisticsRangeWriter : ContentTypeWriter<StatisticsRange>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(StatisticsRange.StatisticsRangeReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, StatisticsRange value)
    {
        output.WriteObject(value.HealthPointsRange);
        output.WriteObject(value.MagicPointsRange);
        output.WriteObject(value.PhysicalOffenseRange);
        output.WriteObject(value.PhysicalDefenseRange);
        output.WriteObject(value.MagicalOffenseRange);
        output.WriteObject(value.MagicalDefenseRange);
    }
}
