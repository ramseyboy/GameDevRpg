#region File Description

//-----------------------------------------------------------------------------
// FightingCharacterWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Characters;

#endregion

namespace RolePlayingGameProcessors.Characters;

public class FightingCharacterWriter : IContentTypeWriterDelegate<FightingCharacter>
{
    private readonly IContentTypeWriterDelegate<Character> characterWriter = new CharacterWriter();

    public void WriteContent(ContentWriter output, FightingCharacter value)
    {
        characterWriter.WriteContent(output, value);
        output.Write(value.CharacterClassContentName);
        output.Write(value.CharacterLevel);
        output.WriteObject(value.InitialEquipmentContentNames);
        output.WriteObject(value.Inventory);
        output.Write(value.CombatAnimationInterval);
        output.WriteObject(value.CombatSprite);
    }
}
