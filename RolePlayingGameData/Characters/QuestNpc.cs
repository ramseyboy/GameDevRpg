#region File Description

//-----------------------------------------------------------------------------
// QuestNpc.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData.Characters;

/// <summary>
///     An NPC that does not fight and does not join the party.
/// </summary>
public class QuestNpc : Character
{
    #region Content Type Reader

    /// <summary>
    ///     Read a QuestNpc object from the content pipeline.
    /// </summary>
    public class QuestNpcReader : ContentTypeReader<QuestNpc>
    {
        private readonly IContentTypeReaderDelegate<Character> characterReader = new CharacterReader();

        protected override QuestNpc Read(ContentReader input,
            QuestNpc existingInstance)
        {
            var questNpc = existingInstance;
            if (questNpc == null)
            {
                questNpc = new QuestNpc();
            }

            characterReader.ReadContent(input, questNpc);
            questNpc.IntroductionDialogue = input.ReadString();

            return questNpc;
        }
    }

    #endregion

    #region Dialogue Data

    /// <summary>
    ///     The dialogue that the Npc says when it is greeted in the world.
    /// </summary>
    private string introductionDialogue;

    /// <summary>
    ///     The dialogue that the Npc says when it is greeted in the world.
    /// </summary>
    public string IntroductionDialogue
    {
        get => introductionDialogue;
        set => introductionDialogue = value;
    }

    #endregion
}
