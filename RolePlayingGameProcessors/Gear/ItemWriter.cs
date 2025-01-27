#region File Description

//-----------------------------------------------------------------------------
// ItemWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGameProcessors.Gear;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class ItemWriter : ContentTypeWriter<Item>
{
    private readonly IContentTypeWriterDelegate<RolePlayingGameData.Gear.Gear> gearWriter = new GearWriter();

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Item.ItemReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Item value)
    {
        gearWriter.WriteContent(output, value);
        output.Write((int) value.Usage);
        output.Write(value.IsOffensive);
        output.Write(value.TargetDuration);
        output.WriteObject(value.TargetEffectRange);
        output.Write(value.AdjacentTargets);
        output.Write(value.UsingCueName);
        output.Write(value.TravelingCueName);
        output.Write(value.ImpactCueName);
        output.Write(value.BlockCueName);
        output.WriteObject(value.CreationSprite);
        output.WriteObject(value.SpellSprite);
        output.WriteObject(value.Overlay);
    }
}
