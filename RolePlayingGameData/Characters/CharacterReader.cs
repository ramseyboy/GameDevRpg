using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RolePlayingGameData;

public class CharacterReader : IContentTypeReaderDelegate<Character>
{
    public void ReadContent(ContentReader input,
        Character existingInstance)
    {
        var character = existingInstance;
        if (character == null)
        {
            throw new ArgumentNullException("existingInstance");
        }

        character.Name = input.ReadString();
        character.MapIdleAnimationInterval = input.ReadInt32();
        character.MapSprite = input.ReadObject<AnimatingSprite>();
        if (character.MapSprite != null)
        {
            character.MapSprite.SourceOffset =
                new Vector2(
                    character.MapSprite.SourceOffset.X - 32,
                    character.MapSprite.SourceOffset.Y - 32);
        }

        character.AddStandardCharacterIdleAnimations();

        character.MapWalkingAnimationInterval = input.ReadInt32();
        character.WalkingSprite = input.ReadObject<AnimatingSprite>();
        if (character.WalkingSprite != null)
        {
            character.WalkingSprite.SourceOffset =
                new Vector2(
                    character.WalkingSprite.SourceOffset.X - 32,
                    character.WalkingSprite.SourceOffset.Y - 32);
        }

        character.AddStandardCharacterWalkingAnimations();

        character.ResetAnimation(false);

        character.ShadowTexture = input.ContentManager.Load<Texture2D>(
            @"Textures\Characters\CharacterShadow");
    }
}
