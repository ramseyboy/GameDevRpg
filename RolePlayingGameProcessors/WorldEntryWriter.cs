#region File Description

//-----------------------------------------------------------------------------
// WorldEntryWriter.cs
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
public class WorldEntryWriter<T> : ContentTypeWriter<WorldEntry<T>>
    where T : ContentObject
{
    private readonly IContentTypeWriterDelegate<MapEntry<T>> mapEntryWriter = new MapEntryWriter<T>();

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(WorldEntry<T>.WorldEntryReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, WorldEntry<T> value)
    {
        mapEntryWriter.WriteContent(output, value);
        output.Write(value.MapContentName);
    }
}
