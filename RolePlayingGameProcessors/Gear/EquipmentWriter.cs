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
using RolePlayingGameData;

#endregion

namespace RolePlayingGameProcessors;

public class EquipmentWriter : IContentTypeWriterDelegate<Equipment>
{
    private readonly IContentTypeWriterDelegate<Gear> gearWriter = new GearWriter();

    public void Write(ContentWriter output, Equipment value)
    {
        gearWriter.Write(output, value);
        output.WriteObject(value.OwnerBuffStatistics);
    }
}
