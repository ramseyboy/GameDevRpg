#region File Description

//-----------------------------------------------------------------------------
// PlayerWriter.cs
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
public class PlayerWriter : ContentTypeWriter<Player>
{
    private readonly IContentTypeWriterDelegate<FightingCharacter> fightingCharacterWriter = new FightingCharacterWriter();

    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Player.PlayerReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Player value)
    {
        fightingCharacterWriter.WriteContent(output, value);
        output.Write(value.Gold);
        output.Write(value.IntroductionDialogue);
        output.Write(value.JoinAcceptedDialogue);
        output.Write(value.JoinRejectedDialogue);
        output.Write(value.ActivePortraitTextureName);
        output.Write(value.InactivePortraitTextureName);
        output.Write(value.UnselectablePortraitTextureName);
    }
}
