#region File Description

//-----------------------------------------------------------------------------
// SpellWriter.cs
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
public class SpellWriter : ContentTypeWriter<Spell>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Spell.SpellReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Spell value)
    {
        output.Write(value.Name);
        output.Write(value.Description);
        output.Write(value.MagicPointCost);
        output.Write(value.IconTextureName);
        output.Write(value.IsOffensive);
        output.Write(value.TargetDuration);
        output.WriteObject(value.InitialTargetEffectRange);
        output.Write(value.AdjacentTargets);
        output.WriteObject(value.LevelingProgression);
        output.Write(value.CreatingCueName);
        output.Write(value.TravelingCueName);
        output.Write(value.ImpactCueName);
        output.Write(value.BlockCueName);
        output.WriteObject(value.SpellSprite);
        output.WriteObject(value.Overlay);
    }
}
