#region File Description

//-----------------------------------------------------------------------------
// AnimatingSpriteWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Animation;

#endregion

namespace RolePlayingGameProcessors.Animation;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class AnimatingSpriteWriter : ContentTypeWriter<AnimatingSprite>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(AnimatingSprite.AnimatingSpriteReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, AnimatingSprite value)
    {
        output.Write(value.TextureName);
        output.WriteObject(value.FrameDimensions);
        output.Write(value.FramesPerRow);
        output.WriteObject(value.SourceOffset);
        output.WriteObject(value.Animations);
    }
}
