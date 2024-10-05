#region File Description

//-----------------------------------------------------------------------------
// ChestWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;
using RolePlayingGameData.Map;

#endregion

namespace RolePlayingGameProcessors.Map;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class ChestWriter : ContentTypeWriter<Chest>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Chest.ChestReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Chest value)
    {
        // remove any entries that have zero quantity
        value.Entries.RemoveAll(delegate(ContentEntry<RolePlayingGameData.Gear.Gear> contentEntry) { return contentEntry.Count <= 0; });

        output.Write(value.Name);
        output.Write(value.Gold);
        output.WriteObject(value.Entries);
        output.Write(value.TextureName);
    }
}
