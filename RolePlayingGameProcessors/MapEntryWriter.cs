#region File Description

//-----------------------------------------------------------------------------
// MapEntryWriter.cs
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
public class MapEntryWriter<T> : ContentTypeWriter<MapEntry<T>>, IContentTypeWriterDelegate<MapEntry<T>>
    where T : ContentObject
{
    private readonly IContentTypeWriterDelegate<ContentEntry<T>> contentEntryWriter = new ContentEntryWriter<T>();

    public void WriteContent(ContentWriter output, MapEntry<T> value)
    {
        Write(output, value);
    }

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(MapEntryReader<T>).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, MapEntry<T> value)
    {
        contentEntryWriter.WriteContent(output, value);
        output.WriteObject(value.MapPosition);
        output.Write((int) value.Direction);
    }
}
