#region File Description

//-----------------------------------------------------------------------------
// CharacterWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Characters;

#endregion

namespace RolePlayingGameProcessors.Characters;

public class CharacterWriter : IContentTypeWriterDelegate<Character>
{
    public void WriteContent(ContentWriter output, Character value)
    {
        output.Write(value.Name);
        output.Write(value.MapIdleAnimationInterval);
        output.WriteObject(value.MapSprite);
        output.Write(value.MapWalkingAnimationInterval);
        output.WriteObject(value.WalkingSprite);
    }
}
