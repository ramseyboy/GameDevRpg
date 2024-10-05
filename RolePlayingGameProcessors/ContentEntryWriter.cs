#region File Description

//-----------------------------------------------------------------------------
// ContentEntryWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;

#endregion

namespace RolePlayingGameProcessors;

public class ContentEntryWriter<T> : IContentTypeWriterDelegate<ContentEntry<T>>
    where T : ContentObject
{
    public void WriteContent(ContentWriter output, ContentEntry<T> value)
    {
        output.Write(value.ContentName == null ? string.Empty : value.ContentName);
        output.Write(value.Count);
    }
}
