#region File Description

//-----------------------------------------------------------------------------
// Weapon.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData.Animation;
using RolePlayingGameData.Data;

#endregion

namespace RolePlayingGameData.Gear;

/// <summary>
///     Equipment that can be equipped on a FightingCharacter
///     to improve their physical damage.
/// </summary>
public class Weapon : Equipment
{
    #region Description Data

    /// <summary>
    ///     Builds and returns a string describing the power of this weapon.
    /// </summary>
    public override string GetPowerText()
    {
        return "Weapon Attack: " + TargetDamageRange;
    }

    #endregion


    #region Content Type Reader

    /// <summary>
    ///     Read the Weapon type from the content pipeline.
    /// </summary>
    public class WeaponReader : ContentTypeReader<Weapon>
    {
        private readonly IContentTypeReaderDelegate<Equipment> equipmentReader = new EquipmentReader();

        /// <summary>
        ///     Read the Weapon type from the content pipeline.
        /// </summary>
        protected override Weapon Read(ContentReader input, Weapon existingInstance)
        {
            var weapon = existingInstance;

            if (weapon == null)
            {
                weapon = new Weapon();
            }

            // read the gear settings
            equipmentReader.ReadContent(input, weapon);

            // read the weapon settings
            weapon.TargetDamageRange = input.ReadObject<Int32Range>();
            weapon.SwingCueName = input.ReadString();
            weapon.HitCueName = input.ReadString();
            weapon.BlockCueName = input.ReadString();
            weapon.Overlay = input.ReadObject<AnimatingSprite>();
            weapon.Overlay.SourceOffset = new Vector2(
                weapon.Overlay.FrameDimensions.X / 2,
                weapon.Overlay.FrameDimensions.Y);

            return weapon;
        }
    }

    #endregion


    #region Target Damage Data

    /// <summary>
    ///     The range of health damage applied by this weapon to its target.
    /// </summary>
    /// <remarks>Damage range values are positive, and will be subtracted.</remarks>
    private Int32Range targetDamageRange;

    /// <summary>
    ///     The range of health damage applied by this weapon to its target.
    /// </summary>
    /// <remarks>Damage range values are positive, and will be subtracted.</remarks>
    public Int32Range TargetDamageRange
    {
        get => targetDamageRange;
        set => targetDamageRange = value;
    }

    #endregion


    #region Sound Effects Data

    /// <summary>
    ///     The name of the sound effect cue played when the weapon is swung.
    /// </summary>
    private string swingCueName;

    /// <summary>
    ///     The name of the sound effect cue played when the weapon is swung.
    /// </summary>
    public string SwingCueName
    {
        get => swingCueName;
        set => swingCueName = value;
    }


    /// <summary>
    ///     The name of the sound effect cue played when the weapon hits its target.
    /// </summary>
    private string hitCueName;

    /// <summary>
    ///     The name of the sound effect cue played when the weapon hits its target.
    /// </summary>
    public string HitCueName
    {
        get => hitCueName;
        set => hitCueName = value;
    }


    /// <summary>
    ///     The name of the sound effect cue played when the weapon is blocked.
    /// </summary>
    private string blockCueName;

    /// <summary>
    ///     The name of the sound effect cue played when the weapon is blocked.
    /// </summary>
    public string BlockCueName
    {
        get => blockCueName;
        set => blockCueName = value;
    }

    #endregion


    #region Graphics Data

    /// <summary>
    ///     The overlay sprite for this weapon.
    /// </summary>
    private AnimatingSprite overlay;

    /// <summary>
    ///     The overlay sprite for this weapon.
    /// </summary>
    public AnimatingSprite Overlay
    {
        get => overlay;
        set => overlay = value;
    }

    #endregion
}
