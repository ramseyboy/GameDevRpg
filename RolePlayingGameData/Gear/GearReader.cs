using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RolePlayingGameData;

public class GearReader : IContentTypeReaderDelegate<Gear>
{
    public Gear Read(ContentReader input, Gear existingInstance)
    {
        Gear gear = existingInstance;
        if (gear == null)
        {
            throw new ArgumentException("Unable to create new Gear objects.");
        }

        gear.AssetName = input.AssetName;

        // read gear settings
        gear.Name = input.ReadString();
        gear.Description = input.ReadString();
        gear.GoldValue = input.ReadInt32();
        gear.IsDroppable = input.ReadBoolean();
        gear.MinimumCharacterLevel = input.ReadInt32();
        gear.SupportedClasses.AddRange(input.ReadObject<List<string>>());
        gear.IconTextureName = input.ReadString();
        gear.IconTexture = input.ContentManager.Load<Texture2D>(
            System.IO.Path.Combine(@"Textures\Gear", gear.IconTextureName));

        return gear;
    }
}
