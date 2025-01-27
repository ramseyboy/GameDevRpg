#region File Description

//-----------------------------------------------------------------------------
// StoreCategory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData.Map;

/// <summary>
///     A category of gear for sale in a store.
/// </summary>
public class StoreCategory
{
    /// <summary>
    ///     The content names for the gear available in this category.
    /// </summary>
    private List<string> availableContentNames = new();


    /// <summary>
    ///     The gear available in this category.
    /// </summary>
    private List<Gear.Gear> availableGear = new();

    /// <summary>
    ///     The display name of this store category.
    /// </summary>
    private string name;

    /// <summary>
    ///     The display name of this store category.
    /// </summary>
    public string Name
    {
        get => name;
        set => name = value;
    }

    /// <summary>
    ///     The content names for the gear available in this category.
    /// </summary>
    public List<string> AvailableContentNames
    {
        get => availableContentNames;
        set => availableContentNames = value;
    }

    /// <summary>
    ///     The gear available in this category.
    /// </summary>
    [ContentSerializerIgnore]
    public List<Gear.Gear> AvailableGear
    {
        get => availableGear;
        set => availableGear = value;
    }


    #region Content Type Reader

    /// <summary>
    ///     Reads a StoreCategory object from the content pipeline.
    /// </summary>
    public class StoreCategoryReader : ContentTypeReader<StoreCategory>
    {
        /// <summary>
        ///     Reads a StoreCategory object from the content pipeline.
        /// </summary>
        protected override StoreCategory Read(ContentReader input,
            StoreCategory existingInstance)
        {
            var storeCategory = existingInstance;
            if (storeCategory == null)
            {
                storeCategory = new StoreCategory();
            }

            storeCategory.Name = input.ReadString();
            storeCategory.AvailableContentNames.AddRange(
                input.ReadObject<List<string>>());

            // populate the gear list based on the content names
            foreach (var gearName in storeCategory.AvailableContentNames)
            {
                storeCategory.AvailableGear.Add(input.ContentManager.Load<Gear.Gear>(
                    Path.Combine("Gear", gearName)));
            }

            return storeCategory;
        }
    }

    #endregion
}
