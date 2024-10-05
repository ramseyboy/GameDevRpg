#region File Description

//-----------------------------------------------------------------------------
// GearWriter.cs
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

public class GearWriter : IContentTypeWriterDelegate<Gear>
{
    public void WriteContent(ContentWriter output, Gear value)
    {
        output.Write(value.Name);
        output.Write(value.Description);
        output.Write(value.GoldValue);
        output.Write(value.IsDroppable);
        output.Write(value.MinimumCharacterLevel);
        output.WriteObject(value.SupportedClasses);
        output.Write(value.IconTextureName);
    }
}
