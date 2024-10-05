#region File Description

//-----------------------------------------------------------------------------
// CharacterLevelDescription.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData;

/// <summary>
///     The requirements and rewards for each level for a character class.
/// </summary>
public class CharacterLevelDescription
{
    #region Content Type Reader

    /// <summary>
    ///     Read a CharacterLevelDescription object from the content pipeline.
    /// </summary>
    public class CharacterLevelDescriptionReader :
        ContentTypeReader<CharacterLevelDescription>
    {
        /// <summary>
        ///     Read a CharacterLevelDescription object from the content pipeline.
        /// </summary>
        protected override CharacterLevelDescription Read(ContentReader input,
            CharacterLevelDescription existingInstance)
        {
            var desc = existingInstance;
            if (desc == null)
            {
                desc = new CharacterLevelDescription();
            }

            desc.ExperiencePoints = input.ReadInt32();
            desc.SpellContentNames.AddRange(input.ReadObject<List<string>>());

            // load all of the spells immediately
            foreach (var spellContentName in desc.SpellContentNames)
            {
                desc.spells.Add(input.ContentManager.Load<Spell>(
                    Path.Combine("Spells", spellContentName)));
            }

            return desc;
        }
    }

    #endregion

    #region Experience Requirements

    /// <summary>
    ///     The amount of additional experience necessary to achieve this level.
    /// </summary>
    private int experiencePoints;

    /// <summary>
    ///     The amount of additional experience necessary to achieve this level.
    /// </summary>
    public int ExperiencePoints
    {
        get => experiencePoints;
        set => experiencePoints = value;
    }

    #endregion


    #region Spell Rewards

    /// <summary>
    ///     The content names of the spells given to the character
    ///     when it reaches this level.
    /// </summary>
    private List<string> spellContentNames = new();

    /// <summary>
    ///     The content names of the spells given to the character
    ///     when it reaches this level.
    /// </summary>
    public List<string> SpellContentNames
    {
        get => spellContentNames;
        set => spellContentNames = value;
    }


    /// <summary>
    ///     Spells given to the character when it reaches this level.
    /// </summary>
    private List<Spell> spells = new();

    /// <summary>
    ///     Spells given to the character when it reaches this level.
    /// </summary>
    [ContentSerializerIgnore]
    public List<Spell> Spells
    {
        get => spells;
        set => spells = value;
    }

    #endregion
}
