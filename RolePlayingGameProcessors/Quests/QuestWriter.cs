#region File Description

//-----------------------------------------------------------------------------
// QuestWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Quests;

#endregion

namespace RolePlayingGameProcessors.Quests;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class QuestWriter : ContentTypeWriter<Quest>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Quest.QuestReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Quest value)
    {
        output.Write(value.Name);
        output.Write(value.Description);
        output.Write(value.ObjectiveMessage);
        output.Write(value.CompletionMessage);
        output.WriteObject(value.GearRequirements);
        output.WriteObject(value.MonsterRequirements);
        output.WriteObject(value.FixedCombatEntries);
        output.WriteObject(value.ChestEntries);
        output.Write(value.DestinationMapContentName);
        output.Write(value.DestinationNpcContentName);
        output.Write(value.DestinationObjectiveMessage);
        output.Write(value.ExperienceReward);
        output.Write(value.GoldReward);
        output.WriteObject(value.GearRewardContentNames);
    }
}
