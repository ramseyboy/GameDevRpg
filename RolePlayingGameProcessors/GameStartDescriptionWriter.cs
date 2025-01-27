#region File Description

//-----------------------------------------------------------------------------
// GameStartDescriptionWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;

#endregion

namespace RolePlayingGameProcessors;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class GameStartDescriptionWriter : ContentTypeWriter<GameStartDescription>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(GameStartDescription.GameStartDescriptionReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, GameStartDescription value)
    {
        if (value.PlayerContentNames.Count <= 0)
        {
            throw new ArgumentException(
                "The starting party must have at least one player in it.");
        }

        output.Write(value.MapContentName);
        output.WriteObject(value.PlayerContentNames);
        output.Write(value.QuestLineContentName);
    }
}
