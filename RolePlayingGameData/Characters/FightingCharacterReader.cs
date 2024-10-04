using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public class FightingCharacterReader : IContentTypeReaderDelegate<FightingCharacter>
{
    private IContentTypeReaderDelegate<Character> characterReader = new CharacterReader();

    public FightingCharacter Read(ContentReader input,
        FightingCharacter existingInstance)
    {
        FightingCharacter fightingCharacter = existingInstance;
        if (fightingCharacter == null)
        {
            throw new ArgumentNullException("existingInstance");
        }

        characterReader.Read(input, fightingCharacter);
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
                System.IO.Path.Combine("CharacterClasses",
                    fightingCharacter.CharacterClassContentName));

        // populate the equipment list
        foreach (string gearName in
                 fightingCharacter.InitialEquipmentContentNames)
        {
            fightingCharacter.Equip(input.ContentManager.Load<Equipment>(
                System.IO.Path.Combine("Gear", gearName)));
        }

        fightingCharacter.RecalculateEquipmentStatistics();
        fightingCharacter.RecalculateTotalTargetDamageRange();
        fightingCharacter.RecalculateTotalDefenseRanges();

        // populate the inventory based on the content names
        foreach (ContentEntry<Gear> inventoryEntry in
                 fightingCharacter.Inventory)
        {
            inventoryEntry.Content = input.ContentManager.Load<Gear>(
                System.IO.Path.Combine("Gear", inventoryEntry.ContentName));
        }

        return fightingCharacter;
    }
}
