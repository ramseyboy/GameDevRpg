#region File Description

//-----------------------------------------------------------------------------
// EquipmentWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGameProcessors.Gear;

public class EquipmentWriter : IContentTypeWriterDelegate<Equipment>
{
    private readonly IContentTypeWriterDelegate<RolePlayingGameData.Gear.Gear> gearWriter = new GearWriter();

    public void WriteContent(ContentWriter output, Equipment value)
    {
        gearWriter.WriteContent(output, value);
        output.WriteObject(value.OwnerBuffStatistics);
    }
}
