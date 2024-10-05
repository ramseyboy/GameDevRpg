#region File Description

//-----------------------------------------------------------------------------
// GearDropWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;

#endregion

namespace RolePlayingGameProcessors;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class GearDropWriter : ContentTypeWriter<GearDrop>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(GearDrop.GearDropReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, GearDrop value)
    {
        output.Write(value.GearName);
        output.Write(value.DropPercentage);
    }
}
