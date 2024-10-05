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

#endregion

namespace RolePlayingGameProcessors.Gear;

public class GearWriter : IContentTypeWriterDelegate<RolePlayingGameData.Gear.Gear>
{
    public void WriteContent(ContentWriter output, RolePlayingGameData.Gear.Gear value)
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
