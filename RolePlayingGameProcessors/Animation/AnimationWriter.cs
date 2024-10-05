#region File Description

//-----------------------------------------------------------------------------
// AnimationWriter.cs
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
public class AnimationWriter : ContentTypeWriter<Animation>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Animation.AnimationReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Animation value)
    {
        output.Write(string.IsNullOrEmpty(value.Name) ? string.Empty : value.Name);
        output.Write(value.StartingFrame);
        output.Write(value.EndingFrame);
        output.Write(value.Interval);
        output.Write(value.IsLoop);
    }
}
