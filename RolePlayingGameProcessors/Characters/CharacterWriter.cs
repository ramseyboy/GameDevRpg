#region File Description
//-----------------------------------------------------------------------------
// CharacterWriter.cs
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
    public class CharacterWriter : ContentTypeWriter<Character>
    {
        /// <inheritdoc />
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
            => typeof(Character.CharacterReader).AssemblyQualifiedName ?? string.Empty;

        protected override void Write(ContentWriter output, Character value)
        {
            output.Write(value.Name);
            output.Write(value.MapIdleAnimationInterval);
            output.WriteObject(value.MapSprite);
            output.Write(value.MapWalkingAnimationInterval);
            output.WriteObject(value.WalkingSprite);
        }
    }
}
