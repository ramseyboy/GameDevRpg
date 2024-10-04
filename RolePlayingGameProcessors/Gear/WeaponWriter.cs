#region File Description
//-----------------------------------------------------------------------------
// WeaponWriter.cs
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
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class WeaponWriter : ContentTypeWriter<Weapon>
    {
        private readonly IContentTypeWriterDelegate<Equipment> equipmentWriter = new EquipmentWriter();

        /// <inheritdoc />
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
            => typeof(Weapon.WeaponReader).AssemblyQualifiedName ?? string.Empty;

        protected override void Write(ContentWriter output, Weapon value)
        {
            // write out equipment values
            equipmentWriter.Write(output, value);

            output.WriteObject(value.TargetDamageRange);
            output.Write(value.SwingCueName);
            output.Write(value.HitCueName);
            output.Write(value.BlockCueName);
            output.WriteObject(value.Overlay);
        }
    }
}
