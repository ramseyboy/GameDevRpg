#region File Description

//-----------------------------------------------------------------------------
// StatisticsValueWriter.cs
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
public class StatisticsValueWriter : ContentTypeWriter<StatisticsValue>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(StatisticsValue.StatisticsValueReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, StatisticsValue value)
    {
        output.Write(value.HealthPoints);
        output.Write(value.MagicPoints);
        output.Write(value.PhysicalOffense);
        output.Write(value.PhysicalDefense);
        output.Write(value.MagicalOffense);
        output.Write(value.MagicalDefense);
    }
}
