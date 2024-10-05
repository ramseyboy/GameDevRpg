#region File Description
//-----------------------------------------------------------------------------
// ContentEntryWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;
#endregion

namespace RolePlayingGameProcessors
{
    public class ContentEntryWriter<T> : IContentTypeWriterDelegate<ContentEntry<T>>
        where T : ContentObject
    {
        public void WriteContent(ContentWriter output, ContentEntry<T> value)
        {
            output.Write(value.ContentName == null ? String.Empty : value.ContentName);
            output.Write(value.Count);
        }
    }
}
