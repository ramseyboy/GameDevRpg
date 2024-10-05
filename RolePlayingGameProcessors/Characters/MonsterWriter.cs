#region File Description

//-----------------------------------------------------------------------------
// MonsterWriter.cs
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
public class MonsterWriter : ContentTypeWriter<Monster>
{
    private readonly IContentTypeWriterDelegate<FightingCharacter> fightingCharacterWriter = new FightingCharacterWriter();

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Monster.MonsterReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Monster value)
    {
        fightingCharacterWriter.WriteContent(output, value);
        output.Write(value.DefendPercentage);
        output.WriteObject(value.GearDrops);
    }
}
