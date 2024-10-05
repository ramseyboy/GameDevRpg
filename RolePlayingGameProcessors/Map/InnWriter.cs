#region File Description

//-----------------------------------------------------------------------------
// InnWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RolePlayingGameData;

#endregion

namespace RolePlayingGameProcessors;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class InnWriter : ContentTypeWriter<Inn>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(Inn.InnReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, Inn value)
    {
        output.Write(value.Name);
        output.Write(value.ChargePerPlayer);
        output.Write(value.WelcomeMessage);
        output.Write(value.PaidMessage);
        output.Write(value.NotEnoughGoldMessage);
        output.Write(value.ShopkeeperTextureName);
    }
}
