#region File Description

//-----------------------------------------------------------------------------
// Int32RangeWriter.cs
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
public class Int32RangeWriter : ContentTypeWriter<Int32Range>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Int32Range.Int32RangeReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Int32Range value)
    {
        output.Write(value.Minimum);
        output.Write(value.Maximum);
    }
}
