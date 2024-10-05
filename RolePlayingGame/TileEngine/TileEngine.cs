#region File Description

//-----------------------------------------------------------------------------
// PlayerPosition.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGameData;

#endregion

namespace RolePlaying;

/// <summary>
///     Static class for a tileable map
/// </summary>
internal static class TileEngine
{
    #region Map

    /// <summary>
    ///     The map being used by the tile engine.
    /// </summary>
    private static Map map;

    /// <summary>
    ///     The map being used by the tile engine.
    /// </summary>
    public static Map Map => Map;


    /// <summary>
    ///     The position of the outside 0,0 corner of the map, in pixels.
    /// </summary>
    private static Vector2 mapOriginPosition;


    /// <summary>
    ///     Calculate the screen position of a given map location (in tiles).
    /// </summary>
    /// <param name="mapPosition">A map location, in tiles.</param>
    /// <returns>The current screen position of that location.</returns>
    public static Vector2 GetScreenPosition(Point mapPosition)
    {
        return new Vector2(
            mapOriginPosition.X + mapPosition.X * map.TileSize.X,
            mapOriginPosition.Y + mapPosition.Y * map.TileSize.Y);
    }


    /// <summary>
    ///     Set the map in use by the tile engine.
    /// </summary>
    /// <param name="map">The new map for the tile engine.</param>
    /// <param name="portal">The portal the party is entering on, if any.</param>
    public static void SetMap(Map newMap, MapEntry<Portal> portalEntry)
    {
        // check the parameter
        if (newMap == null)
        {
            throw new ArgumentNullException("newMap");
        }

        // assign the new map
        map = newMap;

        // reset the map origin, which will be recalculate on the first update
        mapOriginPosition = Vector2.Zero;

        // move the party to its initial position
        if (portalEntry == null)
        {
            // no portal - use the spawn position
            PartyLeaderPosition.TilePosition = map.SpawnMapPosition;
            PartyLeaderPosition.TileOffset = Vector2.Zero;
            PartyLeaderPosition.Direction = Direction.South;
        }
        else
        {
            // use the portal provided, which may include automatic movement
            PartyLeaderPosition.TilePosition = portalEntry.MapPosition;
            PartyLeaderPosition.TileOffset = Vector2.Zero;
            PartyLeaderPosition.Direction = portalEntry.Direction;
            autoPartyLeaderMovement = Vector2.Multiply(
                new Vector2(map.TileSize.X, map.TileSize.Y),
                new Vector2(
                    portalEntry.Content.LandingMapPosition.X -
                    PartyLeaderPosition.TilePosition.X,
                    portalEntry.Content.LandingMapPosition.Y -
                    PartyLeaderPosition.TilePosition.Y));
        }
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The viewport that the tile engine is rendering within.
    /// </summary>
    private static Viewport viewport;

    /// <summary>
    ///     The viewport that the tile engine is rendering within.
    /// </summary>
    public static Viewport Viewport
    {
        get => viewport;
        set
        {
            viewport = value;
            viewportCenter = new Vector2(
                viewport.X + viewport.Width / 2f,
                viewport.Y + viewport.Height / 2f);
        }
    }


    /// <summary>
    ///     The center of the current viewport.
    /// </summary>
    private static Vector2 viewportCenter;

    #endregion


    #region Party

    /// <summary>
    ///     The speed of the party leader, in units per second.
    /// </summary>
    /// <remarks>
    ///     The movementCollisionTolerance constant should be a multiple of this number.
    /// </remarks>
    private const float partyLeaderMovementSpeed = 3f;


    /// <summary>
    ///     The current position of the party leader.
    /// </summary>
    public static PlayerPosition PartyLeaderPosition { get; set; } = new();


    /// <summary>
    ///     The automatic movement remaining for the party leader.
    /// </summary>
    /// <remarks>
    ///     This is typically used for automatic movement when spawning on a map.
    /// </remarks>
    private static Vector2 autoPartyLeaderMovement = Vector2.Zero;


    /// <summary>
    ///     Updates the automatic movement of the party.
    /// </summary>
    /// <returns>The automatic movement for this update.</returns>
    private static Vector2 UpdatePartyLeaderAutoMovement(GameTime gameTime)
    {
        // check for any remaining auto-movement
        if (autoPartyLeaderMovement == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        // get the remaining-movement direction
        var autoMovementDirection = Vector2.Normalize(autoPartyLeaderMovement);

        // calculate the potential movement vector
        var movement = Vector2.Multiply(autoMovementDirection,
            partyLeaderMovementSpeed);

        // limit the potential movement vector by the remaining auto-movement
        movement.X = Math.Sign(movement.X) * MathHelper.Min(Math.Abs(movement.X),
            Math.Abs(autoPartyLeaderMovement.X));
        movement.Y = Math.Sign(movement.Y) * MathHelper.Min(Math.Abs(movement.Y),
            Math.Abs(autoPartyLeaderMovement.Y));

        // remove the movement from the total remaining auto-movement
        autoPartyLeaderMovement -= movement;

        return movement;
    }


    /// <summary>
    ///     Update the user-controlled movement of the party.
    /// </summary>
    /// <returns>The controlled movement for this update.</returns>
    private static Vector2 UpdateUserMovement(GameTime gameTime)
    {
        var desiredMovement = Vector2.Zero;

        // accumulate the desired direction from user input
        if (InputManager.IsActionPressed(InputManager.Action.MoveCharacterUp))
        {
            if (CanPartyLeaderMoveUp())
            {
                desiredMovement.Y -= partyLeaderMovementSpeed;
            }
        }

        if (InputManager.IsActionPressed(InputManager.Action.MoveCharacterDown))
        {
            if (CanPartyLeaderMoveDown())
            {
                desiredMovement.Y += partyLeaderMovementSpeed;
            }
        }

        if (InputManager.IsActionPressed(InputManager.Action.MoveCharacterLeft))
        {
            if (CanPartyLeaderMoveLeft())
            {
                desiredMovement.X -= partyLeaderMovementSpeed;
            }
        }

        if (InputManager.IsActionPressed(InputManager.Action.MoveCharacterRight))
        {
            if (CanPartyLeaderMoveRight())
            {
                desiredMovement.X += partyLeaderMovementSpeed;
            }
        }

        // if there is no desired movement, then we can't determine a direction
        if (desiredMovement == Vector2.Zero)
        {
            return Vector2.Zero;
        }

        return desiredMovement;
    }

    #endregion


    #region Collision

    /// <summary>
    ///     The number of pixels that characters should be allowed to move into
    ///     blocking tiles.
    /// </summary>
    /// <remarks>
    ///     The partyMovementSpeed constant should cleanly divide this number.
    /// </remarks>
    private const int movementCollisionTolerance = 12;


    /// <summary>
    ///     Returns true if the player can move up from their current position.
    /// </summary>
    private static bool CanPartyLeaderMoveUp()
    {
        // if they're not within the tolerance of the next tile, then this is moot
        if (PartyLeaderPosition.TileOffset.Y > -movementCollisionTolerance)
        {
            return true;
        }

        // if the player is at the outside left and right edges, 
        // then check the diagonal tiles
        if (PartyLeaderPosition.TileOffset.X < -movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X - 1,
                    PartyLeaderPosition.TilePosition.Y - 1)))
            {
                return false;
            }
        }
        else if (PartyLeaderPosition.TileOffset.X > movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X + 1,
                    PartyLeaderPosition.TilePosition.Y - 1)))
            {
                return false;
            }
        }

        // check the tile above the current one
        return !map.IsBlocked(new Point(
            PartyLeaderPosition.TilePosition.X,
            PartyLeaderPosition.TilePosition.Y - 1));
    }


    /// <summary>
    ///     Returns true if the player can move down from their current position.
    /// </summary>
    private static bool CanPartyLeaderMoveDown()
    {
        // if they're not within the tolerance of the next tile, then this is moot
        if (PartyLeaderPosition.TileOffset.Y < movementCollisionTolerance)
        {
            return true;
        }

        // if the player is at the outside left and right edges, 
        // then check the diagonal tiles
        if (PartyLeaderPosition.TileOffset.X < -movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X - 1,
                    PartyLeaderPosition.TilePosition.Y + 1)))
            {
                return false;
            }
        }
        else if (PartyLeaderPosition.TileOffset.X > movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X + 1,
                    PartyLeaderPosition.TilePosition.Y + 1)))
            {
                return false;
            }
        }

        // check the tile below the current one
        return !map.IsBlocked(new Point(
            PartyLeaderPosition.TilePosition.X,
            PartyLeaderPosition.TilePosition.Y + 1));
    }


    /// <summary>
    ///     Returns true if the player can move left from their current position.
    /// </summary>
    private static bool CanPartyLeaderMoveLeft()
    {
        // if they're not within the tolerance of the next tile, then this is moot
        if (PartyLeaderPosition.TileOffset.X > -movementCollisionTolerance)
        {
            return true;
        }

        // if the player is at the outside left and right edges, 
        // then check the diagonal tiles
        if (PartyLeaderPosition.TileOffset.Y < -movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X - 1,
                    PartyLeaderPosition.TilePosition.Y - 1)))
            {
                return false;
            }
        }
        else if (PartyLeaderPosition.TileOffset.Y > movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X - 1,
                    PartyLeaderPosition.TilePosition.Y + 1)))
            {
                return false;
            }
        }

        // check the tile to the left of the current one
        return !map.IsBlocked(new Point(
            PartyLeaderPosition.TilePosition.X - 1,
            PartyLeaderPosition.TilePosition.Y));
    }


    /// <summary>
    ///     Returns true if the player can move right from their current position.
    /// </summary>
    private static bool CanPartyLeaderMoveRight()
    {
        // if they're not within the tolerance of the next tile, then this is moot
        if (PartyLeaderPosition.TileOffset.X < movementCollisionTolerance)
        {
            return true;
        }

        // if the player is at the outside left and right edges, 
        // then check the diagonal tiles
        if (PartyLeaderPosition.TileOffset.Y < -movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X + 1,
                    PartyLeaderPosition.TilePosition.Y - 1)))
            {
                return false;
            }
        }
        else if (PartyLeaderPosition.TileOffset.Y > movementCollisionTolerance)
        {
            if (map.IsBlocked(new Point(
                    PartyLeaderPosition.TilePosition.X + 1,
                    PartyLeaderPosition.TilePosition.Y + 1)))
            {
                return false;
            }
        }

        // check the tile to the right of the current one
        return !map.IsBlocked(new Point(
            PartyLeaderPosition.TilePosition.X + 1,
            PartyLeaderPosition.TilePosition.Y));
    }

    #endregion


    #region Updating

    /// <summary>
    ///     Update the tile engine.
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        // check for auto-movement
        var autoMovement = UpdatePartyLeaderAutoMovement(gameTime);

        // if there is no auto-movement, handle user controls
        var userMovement = Vector2.Zero;
        if (autoMovement == Vector2.Zero)
        {
            userMovement = UpdateUserMovement(gameTime);
            // calculate the desired position
            if (userMovement != Vector2.Zero)
            {
                var desiredTilePosition = PartyLeaderPosition.TilePosition;
                var desiredTileOffset = PartyLeaderPosition.TileOffset;
                PlayerPosition.CalculateMovement(
                    Vector2.Multiply(userMovement, 15f),
                    ref desiredTilePosition,
                    ref desiredTileOffset);
                // check for collisions or encounters in the new tile
                if (PartyLeaderPosition.TilePosition != desiredTilePosition &&
                    !MoveIntoTile(desiredTilePosition))
                {
                    userMovement = Vector2.Zero;
                }
            }
        }

        // move the party
        var oldPartyLeaderTilePosition = PartyLeaderPosition.TilePosition;
        PartyLeaderPosition.Move(autoMovement + userMovement);

        // if the tile position has changed, check for random combat
        if (autoMovement == Vector2.Zero &&
            PartyLeaderPosition.TilePosition != oldPartyLeaderTilePosition)
        {
            Session.CheckForRandomCombat(Map.RandomCombat);
        }

        // adjust the map origin so that the party is at the center of the viewport
        mapOriginPosition += viewportCenter - (PartyLeaderPosition.ScreenPosition +
                                               Session.Party.Players[0].MapSprite.SourceOffset);

        // make sure the boundaries of the map are never inside the viewport
        mapOriginPosition.X = MathHelper.Min(mapOriginPosition.X, viewport.X);
        mapOriginPosition.Y = MathHelper.Min(mapOriginPosition.Y, viewport.Y);
        mapOriginPosition.X += MathHelper.Max(
            viewport.X + viewport.Width -
            (mapOriginPosition.X + map.MapDimensions.X * map.TileSize.X),
            0f);
        mapOriginPosition.Y += MathHelper.Max(
            viewport.Y + viewport.Height - Hud.HudHeight -
            (mapOriginPosition.Y + map.MapDimensions.Y * map.TileSize.Y),
            0f);
    }


    /// <summary>
    ///     Performs any actions associated with moving into a new tile.
    /// </summary>
    /// <returns>True if the character can move into the tile.</returns>
    private static bool MoveIntoTile(Point mapPosition)
    {
        // if the tile is blocked, then this is simple
        if (map.IsBlocked(mapPosition))
        {
            return false;
        }

        // check for anything that might be in the tile
        if (Session.EncounterTile(mapPosition))
        {
            return false;
        }

        // nothing stops the party from moving into the tile
        return true;
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the visible tiles in the given map layers.
    /// </summary>
    public static void DrawLayers(SpriteBatch spriteBatch,
        bool drawBase,
        bool drawFringe,
        bool drawObject)
    {
        // check the parameters
        if (spriteBatch == null)
        {
            throw new ArgumentNullException("spriteBatch");
        }

        if (!drawBase && !drawFringe && !drawObject)
        {
            return;
        }

        var destinationRectangle =
            new Rectangle(0, 0, map.TileSize.X, map.TileSize.Y);

        for (var y = 0; y < map.MapDimensions.Y; y++)
        {
            for (var x = 0; x < map.MapDimensions.X; x++)
            {
                destinationRectangle.X =
                    (int) mapOriginPosition.X + x * map.TileSize.X;
                destinationRectangle.Y =
                    (int) mapOriginPosition.Y + y * map.TileSize.Y;

                // If the tile is inside the screen
                if (CheckVisibility(destinationRectangle))
                {
                    var mapPosition = new Point(x, y);
                    if (drawBase)
                    {
                        var sourceRectangle =
                            map.GetBaseLayerSourceRectangle(mapPosition);
                        if (sourceRectangle != Rectangle.Empty)
                        {
                            spriteBatch.Draw(map.Texture,
                                destinationRectangle,
                                sourceRectangle,
                                Color.White);
                        }
                    }

                    if (drawFringe)
                    {
                        var sourceRectangle =
                            map.GetFringeLayerSourceRectangle(mapPosition);
                        if (sourceRectangle != Rectangle.Empty)
                        {
                            spriteBatch.Draw(map.Texture,
                                destinationRectangle,
                                sourceRectangle,
                                Color.White);
                        }
                    }

                    if (drawObject)
                    {
                        var sourceRectangle =
                            map.GetObjectLayerSourceRectangle(mapPosition);
                        if (sourceRectangle != Rectangle.Empty)
                        {
                            spriteBatch.Draw(map.Texture,
                                destinationRectangle,
                                sourceRectangle,
                                Color.White);
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    ///     Returns true if the given rectangle is within the viewport.
    /// </summary>
    public static bool CheckVisibility(Rectangle screenRectangle)
    {
        return screenRectangle.X > viewport.X - screenRectangle.Width &&
               screenRectangle.Y > viewport.Y - screenRectangle.Height &&
               screenRectangle.X < viewport.X + viewport.Width &&
               screenRectangle.Y < viewport.Y + viewport.Height;
    }

    #endregion
}
