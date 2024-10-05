#region File Description

//-----------------------------------------------------------------------------
// StoreWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData.Map;

#endregion

namespace RolePlayingGameProcessors.Map;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class StoreWriter : ContentTypeWriter<Store>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Store.StoreReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Store value)
    {
        output.Write(value.Name);
        output.Write(value.BuyMultiplier);
        output.Write(value.SellMultiplier);
        output.WriteObject(value.StoreCategories);
        output.Write(value.WelcomeMessage);
        output.Write(value.ShopkeeperTextureName);
    }
}
