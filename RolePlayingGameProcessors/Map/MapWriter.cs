#region File Description

//-----------------------------------------------------------------------------
// MapWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

#endregion

namespace RolePlayingGameProcessors.Map;

/// <summary>
///     This class will be instantiated by the XNA Framework Content Pipeline
///     to write the specified data type into binary .xnb format.
///     This should be part of a Content Pipeline Extension Library project.
/// </summary>
[ContentTypeWriter]
public class MapWriter : ContentTypeWriter<RolePlayingGameData.Map.Map>
{
    /// <inheritdoc />
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
        return typeof(RolePlayingGameData.Map.Map.MapReader).AssemblyQualifiedName ?? string.Empty;
    }

    protected override void Write(ContentWriter output, RolePlayingGameData.Map.Map value)
    {
        // validate the map first
        if (value.MapDimensions.X <= 0 ||
            value.MapDimensions.Y <= 0)
        {
            throw new InvalidContentException("Invalid map dimensions.");
        }

        var totalTiles = value.MapDimensions.X * value.MapDimensions.Y;
        if (value.BaseLayer.Length != totalTiles)
        {
            throw new InvalidContentException("Base layer was " +
                                              value.BaseLayer.Length +
                                              " tiles, but the dimensions specify " +
                                              totalTiles + ".");
        }

        if (value.FringeLayer.Length != totalTiles)
        {
            throw new InvalidContentException("Fringe layer was " +
                                              value.FringeLayer.Length +
                                              " tiles, but the dimensions specify " +
                                              totalTiles + ".");
        }

        if (value.ObjectLayer.Length != totalTiles)
        {
            throw new InvalidContentException("Object layer was " +
                                              value.ObjectLayer.Length +
                                              " tiles, but the dimensions specify " +
                                              totalTiles + ".");
        }

        if (value.CollisionLayer.Length != totalTiles)
        {
            throw new InvalidContentException("Collision layer was " +
                                              value.CollisionLayer.Length +
                                              " tiles, but the dimensions specify " +
                                              totalTiles + ".");
        }

        output.Write(value.Name);
        output.WriteObject(value.MapDimensions);
        output.WriteObject(value.TileSize);
        output.WriteObject(value.SpawnMapPosition);
        output.Write(value.TextureName);
        output.Write(value.CombatTextureName);
        output.Write(value.MusicCueName);
        output.Write(value.CombatMusicCueName);
        output.WriteObject(value.BaseLayer);
        output.WriteObject(value.FringeLayer);
        output.WriteObject(value.ObjectLayer);
        output.WriteObject(value.CollisionLayer);
        output.WriteObject(value.Portals);
        output.WriteObject(value.PortalEntries);
        output.WriteObject(value.ChestEntries);
        output.WriteObject(value.FixedCombatEntries);
        output.WriteObject(value.RandomCombat);
        output.WriteObject(value.QuestNpcEntries);
        output.WriteObject(value.PlayerNpcEntries);
        output.WriteObject(value.InnEntries);
        output.WriteObject(value.StoreEntries);
    }
}
