#region File Description

//-----------------------------------------------------------------------------
// QuestLineWriter.cs
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
public class QuestLineWriter : ContentTypeWriter<QuestLine>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(QuestLine.QuestLineReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, QuestLine value)
    {
        output.Write(value.Name);
        output.WriteObject(value.QuestContentNames);
    }
}
