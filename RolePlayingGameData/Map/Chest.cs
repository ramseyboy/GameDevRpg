#region File Description

//-----------------------------------------------------------------------------
// Chest.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RolePlayingGameData.Map;

/// <summary>
///     A treasure chest in the game world.
/// </summary>
public class Chest : WorldObject
#if WINDOWS
, ICloneable
#endif
{
    #region Chest Contents

    /// <summary>
    ///     The amount of gold in the chest.
    /// </summary>
    private int gold;

    /// <summary>
    ///     The amount of gold in the chest.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public int Gold
    {
        get => gold;
        set => gold = value;
    }


    /// <summary>
    ///     The gear in the chest, along with quantities.
    /// </summary>
    private List<ContentEntry<Gear.Gear>> entries = new();

    /// <summary>
    ///     The gear in the chest, along with quantities.
    /// </summary>
    public List<ContentEntry<Gear.Gear>> Entries
    {
        get => entries;
        set => entries = value;
    }


    /// <summary>
    ///     Array accessor for the chest's contents.
    /// </summary>
    public ContentEntry<Gear.Gear> this[int index] => entries[index];


    /// <summary>
    ///     Returns true if the chest is empty.
    /// </summary>
    public bool IsEmpty => gold <= 0 && entries.Count <= 0;

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The content name of the texture for this chest.
    /// </summary>
    private string textureName;

    /// <summary>
    ///     The content name of the texture for this chest.
    /// </summary>
    public string TextureName
    {
        get => textureName;
        set => textureName = value;
    }


    /// <summary>
    ///     The texture for this chest.
    /// </summary>
    private Texture2D texture;

    /// <summary>
    ///     The texture for this chest.
    /// </summary>
    [ContentSerializerIgnore]
    public Texture2D Texture
    {
        get => texture;
        set => texture = value;
    }

    #endregion


    #region Content Type Reader

    /// <summary>
    ///     Reads a Chest object from the content pipeline.
    /// </summary>
    public class ChestReader : ContentTypeReader<Chest>
    {
        protected override Chest Read(ContentReader input,
            Chest existingInstance)
        {
            var chest = existingInstance;
            if (chest == null)
            {
                chest = new Chest();
            }

            chest.Name = input.ReadString();
            chest.Gold = input.ReadInt32();

            chest.Entries.AddRange(
                input.ReadObject<List<ContentEntry<Gear.Gear>>>());
            foreach (var contentEntry in chest.Entries)
            {
                contentEntry.Content = input.ContentManager.Load<Gear.Gear>(
                    Path.Combine(@"Gear",
                        contentEntry.ContentName));
            }

            chest.TextureName = input.ReadString();
            chest.Texture = input.ContentManager.Load<Texture2D>(
                Path.Combine(@"Textures\Chests", chest.TextureName));

            return chest;
        }
    }

    #endregion


    #region ICloneable Members

    /// <summary>
    ///     Clone implementation for chest copies.
    /// </summary>
    /// <remarks>
    ///     The game has to handle chests that have had some contents removed
    ///     without modifying the original chest (and all chests that come after).
    /// </remarks>
    public object Clone()
    {
        // create the new chest
        var chest = new Chest();

        // copy the data
        chest.Gold = Gold;
        chest.Name = Name;
        chest.Texture = Texture;
        chest.TextureName = TextureName;

        // recreate the list and entries, as counts may have changed
        chest.entries = new List<ContentEntry<Gear.Gear>>();
        foreach (var originalEntry in Entries)
        {
            var newEntry = new ContentEntry<Gear.Gear>();
            newEntry.Count = originalEntry.Count;
            newEntry.ContentName = originalEntry.ContentName;
            newEntry.Content = originalEntry.Content;
            chest.Entries.Add(newEntry);
        }

        return chest;
    }

    #endregion
}
