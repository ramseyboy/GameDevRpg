#region File Description

//-----------------------------------------------------------------------------
// Store.cs
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
///     A gear store, where the party can buy and sell gear, organized into categories.
/// </summary>
public class Store : WorldObject
{
    #region Content Type Reader

    /// <summary>
    ///     Reads an Store object from the content pipeline.
    /// </summary>
    public class StoreReader : ContentTypeReader<Store>
    {
        protected override Store Read(ContentReader input, Store existingInstance)
        {
            var store = existingInstance;
            if (store == null)
            {
                store = new Store();
            }

            store.Name = input.ReadString();
            store.BuyMultiplier = input.ReadSingle();
            store.SellMultiplier = input.ReadSingle();
            store.StoreCategories.AddRange(input.ReadObject<List<StoreCategory>>());
            store.WelcomeMessage = input.ReadString();
            store.ShopkeeperTextureName = input.ReadString();
            store.shopkeeperTexture = input.ContentManager.Load<Texture2D>(
                Path.Combine(@"Textures\Characters\Portraits",
                    store.ShopkeeperTextureName));

            return store;
        }
    }

    #endregion

    #region Shopping Data

    /// <summary>
    ///     A purchasing multiplier applied to the price of all gear.
    /// </summary>
    private float buyMultiplier;

    /// <summary>
    ///     A purchasing multiplier applied to the price of all gear.
    /// </summary>
    public float BuyMultiplier
    {
        get => buyMultiplier;
        set => buyMultiplier = value;
    }


    /// <summary>
    ///     A sell-back multiplier applied to the price of all gear.
    /// </summary>
    private float sellMultiplier;

    /// <summary>
    ///     A sell-back multiplier applied to the price of all gear.
    /// </summary>
    public float SellMultiplier
    {
        get => sellMultiplier;
        set => sellMultiplier = value;
    }


    /// <summary>
    ///     The categories of gear in this store.
    /// </summary>
    private List<StoreCategory> storeCategories = new();

    /// <summary>
    ///     The categories of gear in this store.
    /// </summary>
    public List<StoreCategory> StoreCategories
    {
        get => storeCategories;
        set => storeCategories = value;
    }

    #endregion


    #region Menu Messages

    /// <summary>
    ///     The message shown when the party enters the store.
    /// </summary>
    private string welcomeMessage;

    /// <summary>
    ///     The message shown when the party enters the store.
    /// </summary>
    public string WelcomeMessage
    {
        get => welcomeMessage;
        set => welcomeMessage = value;
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The content path and name of the texture for the shopkeeper.
    /// </summary>
    private string shopkeeperTextureName;

    /// <summary>
    ///     The content path and name of the texture for the shopkeeper.
    /// </summary>
    public string ShopkeeperTextureName
    {
        get => shopkeeperTextureName;
        set => shopkeeperTextureName = value;
    }


    /// <summary>
    ///     The texture for the shopkeeper.
    /// </summary>
    private Texture2D shopkeeperTexture;

    /// <summary>
    ///     The texture for the shopkeeper.
    /// </summary>
    [ContentSerializerIgnore]
    public Texture2D ShopkeeperTexture => shopkeeperTexture;

    #endregion
}
