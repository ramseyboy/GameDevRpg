#region File Description
//-----------------------------------------------------------------------------
// FightingCharacterWriter.cs
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
    public class FightingCharacterWriter : IContentTypeWriterDelegate<FightingCharacter>
    {
        private readonly IContentTypeWriterDelegate<Character> characterWriter = new CharacterWriter();

        public void Write(ContentWriter output, FightingCharacter value)
        {
            characterWriter.Write(output, value);
            output.Write(value.CharacterClassContentName);
            output.Write(value.CharacterLevel);
            output.WriteObject(value.InitialEquipmentContentNames);
            output.WriteObject(value.Inventory);
            output.Write(value.CombatAnimationInterval);
            output.WriteObject(value.CombatSprite);
        }
    }
}
