#region File Description

//-----------------------------------------------------------------------------
// FightingCharacter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData.Animation;
using RolePlayingGameData.Data;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGameData.Characters;

/// <summary>
///     A character that engages in combat.
/// </summary>
public abstract class FightingCharacter : Character
{
    #region Class Data

    /// <summary>
    ///     The name of the character class.
    /// </summary>
    private string characterClassContentName;

    /// <summary>
    ///     The name of the character class.
    /// </summary>
    public string CharacterClassContentName
    {
        get => characterClassContentName;
        set => characterClassContentName = value;
    }


    /// <summary>
    ///     The character class itself.
    /// </summary>
    private CharacterClass characterClass;

    /// <summary>
    ///     The character class itself.
    /// </summary>
    [ContentSerializerIgnore]
    public CharacterClass CharacterClass
    {
        get => characterClass;
        set
        {
            characterClass = value;
            ResetBaseStatistics();
        }
    }


    /// <summary>
    ///     The level of the character.
    /// </summary>
    private int characterLevel = 1;

    /// <summary>
    ///     The level of the character.
    /// </summary>
    public int CharacterLevel
    {
        get => characterLevel;
        set
        {
            characterLevel = value;
            ResetBaseStatistics();
            spells = null;
        }
    }


    /// <summary>
    ///     Returns true if the character is at the maximum level allowed by their class.
    /// </summary>
    public bool IsMaximumCharacterLevel => characterLevel >= characterClass.LevelEntries.Count;


    /// <summary>
    ///     The cached list of spells for this level.
    /// </summary>
    private List<Spell> spells;

    /// <summary>
    ///     The cached list of spells for this level.
    /// </summary>
    [ContentSerializerIgnore]
    public List<Spell> Spells
    {
        get
        {
            if (spells == null && characterClass != null)
            {
                spells = characterClass.GetAllSpellsForLevel(characterLevel);
            }

            return spells;
        }
    }

    #endregion


    #region Experience

    /// <summary>
    ///     The amount of experience points that this character has.
    /// </summary>
    private int experience;

    /// <summary>
    ///     The amount of experience points that this character has.
    /// </summary>
    [ContentSerializerIgnore]
    public int Experience
    {
        get => experience;
        set
        {
            experience = value;
            while (experience >= ExperienceForNextLevel)
            {
                if (IsMaximumCharacterLevel)
                {
                    break;
                }

                experience -= ExperienceForNextLevel;
                CharacterLevel++;
            }
        }
    }


    /// <summary>
    ///     Returns the amount of experience necessary to reach the next character level.
    /// </summary>
    public int ExperienceForNextLevel
    {
        get
        {
            var checkIndex = Math.Min(characterLevel,
                characterClass.LevelEntries.Count) - 1;
            return characterClass.LevelEntries[checkIndex].ExperiencePoints;
        }
    }

    #endregion


    #region Statistics

    /// <summary>
    ///     The base statistics of this character, from the character class and level.
    /// </summary>
    private StatisticsValue baseStatistics;

    /// <summary>
    ///     The base statistics of this character, from the character class and level.
    /// </summary>
    [ContentSerializerIgnore]
    public StatisticsValue BaseStatistics
    {
        get => baseStatistics;
        set => baseStatistics = value;
    }


    /// <summary>
    ///     Reset the character's base statistics.
    /// </summary>
    public void ResetBaseStatistics()
    {
        if (characterClass == null)
        {
            baseStatistics = new StatisticsValue();
        }
        else
        {
            baseStatistics = characterClass.GetStatisticsForLevel(characterLevel);
        }
    }


    /// <summary>
    ///     The total statistics for this character.
    /// </summary>
    [ContentSerializerIgnore]
    public StatisticsValue CharacterStatistics => baseStatistics + equipmentBuffStatistics;

    #endregion


    #region Equipment

    /// <summary>
    ///     The equipment currently equipped on this character.
    /// </summary>
    private readonly List<Equipment> equippedEquipment = new();

    /// <summary>
    ///     The equipment currently equipped on this character.
    /// </summary>
    [ContentSerializerIgnore]
    public List<Equipment> EquippedEquipment => equippedEquipment;


    /// <summary>
    ///     The content names of the equipment initially equipped on the character.
    /// </summary>
    private List<string> initialEquipmentContentNames = new();

    /// <summary>
    ///     The content names of the equipment initially equipped on the character.
    /// </summary>
    public List<string> InitialEquipmentContentNames
    {
        get => initialEquipmentContentNames;
        set => initialEquipmentContentNames = value;
    }


    /// <summary>
    ///     Retrieve the currently equipped weapon.
    /// </summary>
    /// <remarks>There can only be one weapon equipped at the same time.</remarks>
    public Weapon GetEquippedWeapon()
    {
        return equippedEquipment.Find(delegate(Equipment equipment) { return equipment is Weapon; }) as Weapon;
    }


    /// <summary>
    ///     Equip a new weapon.
    /// </summary>
    /// <returns>True if the weapon was equipped.</returns>
    public bool EquipWeapon(Weapon weapon, out Equipment oldEquipment)
    {
        // check the parameter
        if (weapon == null)
        {
            throw new ArgumentNullException("weapon");
        }

        // check equipment restrictions
        if (!weapon.CheckRestrictions(this))
        {
            oldEquipment = null;
            return false;
        }

        // unequip any existing weapon
        var existingWeapon = GetEquippedWeapon();
        if (existingWeapon != null)
        {
            oldEquipment = existingWeapon;
            equippedEquipment.Remove(existingWeapon);
        }
        else
        {
            oldEquipment = null;
        }

        // add the weapon
        equippedEquipment.Add(weapon);

        // recalculate the statistic changes from equipment
        RecalculateEquipmentStatistics();
        RecalculateTotalTargetDamageRange();

        return true;
    }


    /// <summary>
    ///     Remove any equipped weapons.
    /// </summary>
    public void UnequipWeapon()
    {
        equippedEquipment.RemoveAll(delegate(Equipment equipment) { return equipment is Weapon; });
        RecalculateEquipmentStatistics();
    }


    /// <summary>
    ///     Retrieve the armor equipped in the given slot.
    /// </summary>
    public Armor GetEquippedArmor(Armor.ArmorSlot slot)
    {
        return equippedEquipment.Find(delegate(Equipment equipment)
        {
            var armor = equipment as Armor;
            return armor != null && armor.Slot == slot;
        }) as Armor;
    }


    /// <summary>
    ///     Equip a new piece of armor.
    /// </summary>
    /// <returns>True if the armor could be equipped.</returns>
    public bool EquipArmor(Armor armor, out Equipment oldEquipment)
    {
        // check the parameter
        if (armor == null)
        {
            throw new ArgumentNullException("armor");
        }

        // check equipment requirements
        if (!armor.CheckRestrictions(this))
        {
            oldEquipment = null;
            return false;
        }

        // remove any armor equipped in this slot
        var equippedArmor = GetEquippedArmor(armor.Slot);
        if (equippedArmor != null)
        {
            oldEquipment = equippedArmor;
            equippedEquipment.Remove(equippedArmor);
        }
        else
        {
            oldEquipment = null;
        }

        // add the armor
        equippedEquipment.Add(armor);

        // recalcuate the total armor defense values
        RecalculateTotalDefenseRanges();

        // recalculate the statistics buffs from equipment
        RecalculateEquipmentStatistics();

        return true;
    }


    /// <summary>
    ///     Unequip any armor in the given slot.
    /// </summary>
    public void UnequipArmor(Armor.ArmorSlot slot)
    {
        equippedEquipment.RemoveAll(delegate(Equipment equipment)
        {
            var armor = equipment as Armor;
            return armor != null && armor.Slot == slot;
        });
        RecalculateEquipmentStatistics();
        RecalculateTotalDefenseRanges();
    }


    /// <summary>
    ///     Equip a new piece of equipment.
    /// </summary>
    /// <returns>True if the equipment could be equipped.</returns>
    public virtual bool Equip(Equipment equipment)
    {
        Equipment oldEquipment;

        return Equip(equipment, out oldEquipment);
    }


    /// <summary>
    ///     Equip a new piece of equipment, specifying any equipment auto-unequipped.
    /// </summary>
    /// <returns>True if the equipment could be equipped.</returns>
    public virtual bool Equip(Equipment equipment, out Equipment oldEquipment)
    {
        if (equipment == null)
        {
            throw new ArgumentNullException("equipment");
        }

        if (equipment is Weapon)
        {
            return EquipWeapon(equipment as Weapon, out oldEquipment);
        }

        if (equipment is Armor)
        {
            return EquipArmor(equipment as Armor, out oldEquipment);
        }

        oldEquipment = null;

        return false;
    }


    /// <summary>
    ///     Unequip a piece of equipment.
    /// </summary>
    /// <returns>True if the equipment could be unequipped.</returns>
    public virtual bool Unequip(Equipment equipment)
    {
        if (equipment == null)
        {
            throw new ArgumentNullException("equipment");
        }

        if (equippedEquipment.Remove(equipment))
        {
            RecalculateEquipmentStatistics();
            RecalculateTotalTargetDamageRange();
            RecalculateTotalDefenseRanges();
            return true;
        }

        return false;
    }

    #endregion


    #region Combined Equipment Values

    /// <summary>
    ///     The total statistics changes (buffs) from all equipped equipment.
    /// </summary>
    private StatisticsValue equipmentBuffStatistics;

    /// <summary>
    ///     The total statistics changes (buffs) from all equipped equipment.
    /// </summary>
    [ContentSerializerIgnore]
    public StatisticsValue EquipmentBuffStatistics
    {
        get => equipmentBuffStatistics;
        set => equipmentBuffStatistics = value;
    }


    /// <summary>
    ///     Recalculate the character's equipment-buff statistics.
    /// </summary>
    public void RecalculateEquipmentStatistics()
    {
        // start from scratch
        equipmentBuffStatistics = new StatisticsValue();

        // add the statistics for each piece of equipped equipment
        foreach (var equipment in equippedEquipment)
        {
            equipmentBuffStatistics += equipment.OwnerBuffStatistics;
        }
    }


    /// <summary>
    ///     The target damage range for this character, aggregated from all weapons.
    /// </summary>
    private Int32Range targetDamageRange;

    /// <summary>
    ///     The health damage range for this character, aggregated from all weapons.
    /// </summary>
    public Int32Range TargetDamageRange => targetDamageRange;

    /// <summary>
    ///     Recalculate the character's defense ranges from all of their armor.
    /// </summary>
    public void RecalculateTotalTargetDamageRange()
    {
        // set the initial damage range to the physical offense statistic
        targetDamageRange = new Int32Range();

        // add each weapon's target damage range
        foreach (var equipment in equippedEquipment)
        {
            var weapon = equipment as Weapon;
            if (weapon != null)
            {
                targetDamageRange += weapon.TargetDamageRange;
            }
        }
    }


    /// <summary>
    ///     The health defense range for this character, aggregated from all armor.
    /// </summary>
    private Int32Range healthDefenseRange;

    /// <summary>
    ///     The health defense range for this character, aggregated from all armor.
    /// </summary>
    public Int32Range HealthDefenseRange => healthDefenseRange;


    /// <summary>
    ///     The magic defense range for this character, aggregated from all armor.
    /// </summary>
    private Int32Range magicDefenseRange;

    /// <summary>
    ///     The magic defense range for this character, aggregated from all armor.
    /// </summary>
    public Int32Range MagicDefenseRange => magicDefenseRange;


    /// <summary>
    ///     Recalculate the character's defense ranges from all of their armor.
    /// </summary>
    public void RecalculateTotalDefenseRanges()
    {
        // set the initial damage ranges based on character statistics
        healthDefenseRange = new Int32Range();
        magicDefenseRange = new Int32Range();

        // add the defense ranges for each piece of equipped armor
        foreach (var equipment in equippedEquipment)
        {
            var armor = equipment as Armor;
            if (armor != null)
            {
                healthDefenseRange += armor.OwnerHealthDefenseRange;
                magicDefenseRange += armor.OwnerMagicDefenseRange;
            }
        }
    }

    #endregion


    #region Inventory

    /// <summary>
    ///     The gear in this character's inventory (and not equipped).
    /// </summary>
    private readonly List<ContentEntry<Gear.Gear>> inventory = new();

    /// <summary>
    ///     The gear in this character's inventory (and not equipped).
    /// </summary>
    public List<ContentEntry<Gear.Gear>> Inventory => inventory;

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The animating sprite for the combat view of this character.
    /// </summary>
    private AnimatingSprite combatSprite;

    /// <summary>
    ///     The animating sprite for the combat view of this character.
    /// </summary>
    public AnimatingSprite CombatSprite
    {
        get => combatSprite;
        set => combatSprite = value;
    }


    /// <summary>
    ///     Reset the animations for this character.
    /// </summary>
    public override void ResetAnimation(bool isWalking)
    {
        base.ResetAnimation(isWalking);
        if (combatSprite != null)
        {
            combatSprite.PlayAnimation("Idle");
        }
    }

    #endregion


    #region Static Animation Data

    /// <summary>
    ///     The default animation interval for the combat map sprite.
    /// </summary>
    private int combatAnimationInterval = 100;

    /// <summary>
    ///     The default animation interval for the combat map sprite.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public int CombatAnimationInterval
    {
        get => combatAnimationInterval;
        set => combatAnimationInterval = value;
    }


    /// <summary>
    ///     Add the standard character walk animations to this character.
    /// </summary>
    public void AddStandardCharacterCombatAnimations()
    {
        if (combatSprite != null)
        {
            combatSprite.AddAnimation(new Animation.Animation("Idle",
                37,
                42,
                CombatAnimationInterval,
                true));
            combatSprite.AddAnimation(new Animation.Animation("Walk",
                25,
                30,
                CombatAnimationInterval,
                true));
            combatSprite.AddAnimation(new Animation.Animation("Attack",
                1,
                6,
                CombatAnimationInterval,
                false));
            combatSprite.AddAnimation(new Animation.Animation("SpellCast",
                31,
                36,
                CombatAnimationInterval,
                false));
            combatSprite.AddAnimation(new Animation.Animation("Defend",
                13,
                18,
                CombatAnimationInterval,
                false));
            combatSprite.AddAnimation(new Animation.Animation("Dodge",
                13,
                18,
                CombatAnimationInterval,
                false));
            combatSprite.AddAnimation(new Animation.Animation("Hit",
                19,
                24,
                CombatAnimationInterval,
                false));
            combatSprite.AddAnimation(new Animation.Animation("Die",
                7,
                12,
                CombatAnimationInterval,
                false));
        }
    }

    #endregion
}
