using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public class FightingCharacterReader : IContentTypeReaderDelegate<FightingCharacter>
{
    private readonly IContentTypeReaderDelegate<Character> characterReader = new CharacterReader();

    public void ReadContent(ContentReader input,
        FightingCharacter existingInstance)
    {
        var fightingCharacter = existingInstance;
        if (fightingCharacter == null)
        {
            throw new ArgumentNullException("existingInstance");
        }

        characterReader.ReadContent(input, fightingCharacter);
        fightingCharacter.CharacterClassContentName = input.ReadString();
        fightingCharacter.CharacterLevel = input.ReadInt32();
        fightingCharacter.InitialEquipmentContentNames.AddRange(
            input.ReadObject<List<string>>());
        fightingCharacter.Inventory.AddRange(
            input.ReadObject<List<ContentEntry<Gear>>>());
        fightingCharacter.CombatAnimationInterval = input.ReadInt32();
        fightingCharacter.CombatSprite = input.ReadObject<AnimatingSprite>();
        fightingCharacter.AddStandardCharacterCombatAnimations();
        fightingCharacter.ResetAnimation(false);

        // load the character class
        fightingCharacter.CharacterClass =
            input.ContentManager.Load<CharacterClass>(
                Path.Combine("CharacterClasses",
                    fightingCharacter.CharacterClassContentName));

        // populate the equipment list
        foreach (var gearName in
                 fightingCharacter.InitialEquipmentContentNames)
        {
            fightingCharacter.Equip(input.ContentManager.Load<Equipment>(
                Path.Combine("Gear", gearName)));
        }

        fightingCharacter.RecalculateEquipmentStatistics();
        fightingCharacter.RecalculateTotalTargetDamageRange();
        fightingCharacter.RecalculateTotalDefenseRanges();

        // populate the inventory based on the content names
        foreach (var inventoryEntry in
                 fightingCharacter.Inventory)
        {
            inventoryEntry.Content = input.ContentManager.Load<Gear>(
                Path.Combine("Gear", inventoryEntry.ContentName));
        }
    }
}
