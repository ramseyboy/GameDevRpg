#region File Description

//-----------------------------------------------------------------------------
// Character.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGameData.Animation;

#endregion

namespace RolePlayingGameData.Characters;

/// <summary>
///     A character in the game world.
/// </summary>
#if !XBOX
[DebuggerDisplay("Name = {name}")]
#endif
public abstract class Character : WorldObject
{
    #region Character State

    /// <summary>
    ///     The state of a character.
    /// </summary>
    public enum CharacterState
    {
        /// <summary>
        ///     Ready to perform an action, and playing the idle animation
        /// </summary>
        Idle,

        /// <summary>
        ///     Walking in the world.
        /// </summary>
        Walking,

        /// <summary>
        ///     In defense mode
        /// </summary>
        Defending,

        /// <summary>
        ///     Performing Dodge Animation
        /// </summary>
        Dodging,

        /// <summary>
        ///     Performing Hit Animation
        /// </summary>
        Hit,

        /// <summary>
        ///     Dead, but still playing the dying animation.
        /// </summary>
        Dying,

        /// <summary>
        ///     Dead, with the dead animation.
        /// </summary>
        Dead
    }


    /// <summary>
    ///     The state of this character.
    /// </summary>
    private CharacterState state = CharacterState.Idle;

    /// <summary>
    ///     The state of this character.
    /// </summary>
    [ContentSerializerIgnore]
    public CharacterState State
    {
        get => state;
        set => state = value;
    }


    /// <summary>
    ///     Returns true if the character is dead or dying.
    /// </summary>
    public bool IsDeadOrDying =>
        State == CharacterState.Dying ||
        State == CharacterState.Dead;

    #endregion


    #region Map Data

    /// <summary>
    ///     The position of this object on the map.
    /// </summary>
    private Point mapPosition;

    /// <summary>
    ///     The position of this object on the map.
    /// </summary>
    [ContentSerializerIgnore]
    public Point MapPosition
    {
        get => mapPosition;
        set => mapPosition = value;
    }


    /// <summary>
    ///     The orientation of this object on the map.
    /// </summary>
    private Direction direction;

    /// <summary>
    ///     The orientation of this object on the map.
    /// </summary>
    [ContentSerializerIgnore]
    public Direction Direction
    {
        get => direction;
        set => direction = value;
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The animating sprite for the map view of this character.
    /// </summary>
    private AnimatingSprite mapSprite;

    /// <summary>
    ///     The animating sprite for the map view of this character.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public AnimatingSprite MapSprite
    {
        get => mapSprite;
        set => mapSprite = value;
    }


    /// <summary>
    ///     The animating sprite for the map view of this character as it walks.
    /// </summary>
    /// <remarks>
    ///     If this object is null, then the animations are taken from MapSprite.
    /// </remarks>
    private AnimatingSprite walkingSprite;

    /// <summary>
    ///     The animating sprite for the map view of this character as it walks.
    /// </summary>
    /// <remarks>
    ///     If this object is null, then the animations are taken from MapSprite.
    /// </remarks>
    [ContentSerializer(Optional = true)]
    public AnimatingSprite WalkingSprite
    {
        get => walkingSprite;
        set => walkingSprite = value;
    }


    /// <summary>
    ///     Reset the animations for this character.
    /// </summary>
    public virtual void ResetAnimation(bool isWalking)
    {
        state = isWalking ? CharacterState.Walking : CharacterState.Idle;
        if (mapSprite != null)
        {
            if (isWalking && mapSprite["Walk" + Direction] != null)
            {
                mapSprite.PlayAnimation("Walk", Direction);
            }
            else
            {
                mapSprite.PlayAnimation("Idle", Direction);
            }
        }

        if (walkingSprite != null)
        {
            if (isWalking && walkingSprite["Walk" + Direction] != null)
            {
                walkingSprite.PlayAnimation("Walk", Direction);
            }
            else
            {
                walkingSprite.PlayAnimation("Idle", Direction);
            }
        }
    }


    /// <summary>
    ///     The small blob shadow that is rendered under the characters.
    /// </summary>
    private Texture2D shadowTexture;

    /// <summary>
    ///     The small blob shadow that is rendered under the characters.
    /// </summary>
    [ContentSerializerIgnore]
    public Texture2D ShadowTexture
    {
        get => shadowTexture;
        set => shadowTexture = value;
    }

    #endregion


    #region Standard Animation Data

    /// <summary>
    ///     The default idle-animation interval for the animating map sprite.
    /// </summary>
    private int mapIdleAnimationInterval = 200;

    /// <summary>
    ///     The default idle-animation interval for the animating map sprite.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public int MapIdleAnimationInterval
    {
        get => mapIdleAnimationInterval;
        set => mapIdleAnimationInterval = value;
    }


    /// <summary>
    ///     Add the standard character idle animations to this character.
    /// </summary>
    public void AddStandardCharacterIdleAnimations()
    {
        if (mapSprite != null)
        {
            mapSprite.AddAnimation(new Animation.Animation("IdleSouth",
                1,
                6,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleSouthwest",
                7,
                12,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleWest",
                13,
                18,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleNorthwest",
                19,
                24,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleNorth",
                25,
                30,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleNortheast",
                31,
                36,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleEast",
                37,
                42,
                MapIdleAnimationInterval,
                true));
            mapSprite.AddAnimation(new Animation.Animation("IdleSoutheast",
                43,
                48,
                MapIdleAnimationInterval,
                true));
        }
    }


    /// <summary>
    ///     The default walk-animation interval for the animating map sprite.
    /// </summary>
    private int mapWalkingAnimationInterval = 80;

    /// <summary>
    ///     The default walk-animation interval for the animating map sprite.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public int MapWalkingAnimationInterval
    {
        get => mapWalkingAnimationInterval;
        set => mapWalkingAnimationInterval = value;
    }


    /// <summary>
    ///     Add the standard character walk animations to this character.
    /// </summary>
    public void AddStandardCharacterWalkingAnimations()
    {
        var sprite = walkingSprite == null ? mapSprite : walkingSprite;
        if (sprite != null)
        {
            sprite.AddAnimation(new Animation.Animation("WalkSouth",
                1,
                6,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkSouthwest",
                7,
                12,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkWest",
                13,
                18,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkNorthwest",
                19,
                24,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkNorth",
                25,
                30,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkNortheast",
                31,
                36,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkEast",
                37,
                42,
                MapWalkingAnimationInterval,
                true));
            sprite.AddAnimation(new Animation.Animation("WalkSoutheast",
                43,
                48,
                MapWalkingAnimationInterval,
                true));
        }
    }

    #endregion
}
