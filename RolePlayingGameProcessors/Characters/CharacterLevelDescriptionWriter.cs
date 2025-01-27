#region File Description

//-----------------------------------------------------------------------------
// CharacterLevelDescriptionWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Characters;

#endregion

namespace RolePlayingGameProcessors.Characters;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class CharacterLevelDescriptionWriter :
    ContentTypeWriter<CharacterLevelDescription>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(CharacterLevelDescription.CharacterLevelDescriptionReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output,
        CharacterLevelDescription value)
    {
        output.Write(value.ExperiencePoints);
        output.WriteObject(value.SpellContentNames);
    }
}
