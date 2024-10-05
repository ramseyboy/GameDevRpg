using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RolePlayingGameData.Gear;

public class GearReader : IContentTypeReaderDelegate<Gear>
{
    public void ReadContent(ContentReader input, Gear existingInstance)
    {
        var gear = existingInstance;
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
            Path.Combine(@"Textures\Gear", gear.IconTextureName));
    }
}
