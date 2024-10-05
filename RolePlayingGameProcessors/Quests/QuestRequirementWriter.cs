#region File Description

//-----------------------------------------------------------------------------
// QuestRequirementWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;
using RolePlayingGameData.Quests;

#endregion

namespace RolePlayingGameProcessors.Quests;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class QuestRequirementWriter<T> : ContentTypeWriter<QuestRequirement<T>>
    where T : ContentObject
{
    private readonly IContentTypeWriterDelegate<ContentEntry<T>> contentEntryWriter = new ContentEntryWriter<T>();

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(QuestRequirement<T>.QuestRequirementReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, QuestRequirement<T> value)
    {
        contentEntryWriter.WriteContent(output, value);
    }
}
