#region File Description

//-----------------------------------------------------------------------------
// Inn.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace RolePlayingGameData.Map;

/// <summary>
///     An inn in the game world, where the party can rest and restore themselves.
/// </summary>
public class Inn : WorldObject
{
    #region Content Type Reader

    /// <summary>
    ///     Reads an Inn object from the content pipeline.
    /// </summary>
    public class InnReader : ContentTypeReader<Inn>
    {
        protected override Inn Read(ContentReader input, Inn existingInstance)
        {
            var inn = existingInstance;
            if (inn == null)
            {
                inn = new Inn();
            }

            inn.Name = input.ReadString();
            inn.ChargePerPlayer = input.ReadInt32();
            inn.WelcomeMessage = input.ReadString();
            inn.PaidMessage = input.ReadString();
            inn.NotEnoughGoldMessage = input.ReadString();
            inn.ShopkeeperTextureName = input.ReadString();
            inn.shopkeeperTexture = input.ContentManager.Load<Texture2D>(
                Path.Combine(@"Textures\Characters\Portraits",
                    inn.ShopkeeperTextureName));

            return inn;
        }
    }

    #endregion

    #region Operation

    /// <summary>
    ///     The amount that the party has to pay for each member to stay.
    /// </summary>
    private int chargePerPlayer;

    /// <summary>
    ///     The amount that the party has to pay for each member to stay.
    /// </summary>
    public int ChargePerPlayer
    {
        get => chargePerPlayer;
        set => chargePerPlayer = value;
    }

    #endregion


    #region Menu Message Data

    /// <summary>
    ///     The message shown when the player enters the inn.
    /// </summary>
    private string welcomeMessage;

    /// <summary>
    ///     The message shown when the player enters the inn.
    /// </summary>
    public string WelcomeMessage
    {
        get => welcomeMessage;
        set => welcomeMessage = value;
    }


    /// <summary>
    ///     The message shown when the party successfully pays to stay the night.
    /// </summary>
    private string paidMessage;

    /// <summary>
    ///     The message shown when the party successfully pays to stay the night.
    /// </summary>
    public string PaidMessage
    {
        get => paidMessage;
        set => paidMessage = value;
    }


    /// <summary>
    ///     The message shown when the party tries to stay but lacks the funds.
    /// </summary>
    private string notEnoughGoldMessage;

    /// <summary>
    ///     The message shown when the party tries to stay but lacks the funds.
    /// </summary>
    public string NotEnoughGoldMessage
    {
        get => notEnoughGoldMessage;
        set => notEnoughGoldMessage = value;
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The content name of the texture for the shopkeeper.
    /// </summary>
    private string shopkeeperTextureName;

    /// <summary>
    ///     The content name of the texture for the shopkeeper.
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
