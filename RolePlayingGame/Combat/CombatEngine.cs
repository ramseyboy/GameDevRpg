#region File Description

//-----------------------------------------------------------------------------
// CombatEngine.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RolePlayingGame.Combat.Actions;
using RolePlayingGame.GameScreens;
using RolePlayingGameData;
using RolePlayingGameData.Animation;
using RolePlayingGameData.Characters;
using RolePlayingGameData.Data;
using RolePlayingGameData.Gear;
using RolePlayingGameData.Map;

#endregion

namespace RolePlayingGame.Combat;

/// <summary>
///     The runtime execution engine for the combat system.
/// </summary>
internal class CombatEngine
{
    #region Singleton

    /// <summary>
    ///     The singleton of the combat engine.
    /// </summary>
    private static CombatEngine singleton;


    /// <summary>
    ///     Check to see if there is a combat going on, and throw an exception if not.
    /// </summary>
    private static void CheckSingleton()
    {
        if (singleton == null)
        {
            throw new InvalidOperationException(
                "There is no active combat at this time.");
        }
    }

    #endregion


    #region State

    /// <summary>
    ///     If true, the combat engine is active and the user is in combat.
    /// </summary>
    public static bool IsActive => singleton != null;


    /// <summary>
    ///     If true, it is currently the players' turn.
    /// </summary>
    private bool isPlayersTurn;

    /// <summary>
    ///     If true, it is currently the players' turn.
    /// </summary>
    public static bool IsPlayersTurn
    {
        get
        {
            CheckSingleton();
            return singleton.isPlayersTurn;
        }
    }

    #endregion


    #region Rewards

    /// <summary>
    ///     The fixed combat used to generate this fight, if any.
    /// </summary>
    /// <remarks>
    ///     Used for rewards.  Null means it was a random fight with no special rewards.
    /// </remarks>
    private MapEntry<FixedCombat> fixedCombatEntry;

    /// <summary>
    ///     The fixed combat used to generate this fight, if any.
    /// </summary>
    /// <remarks>
    ///     Used for rewards.  Null means it was a random fight with no special rewards.
    /// </remarks>
    public static MapEntry<FixedCombat> FixedCombatEntry => singleton == null ? null : singleton.fixedCombatEntry;

    #endregion


    #region Players

    /// <summary>
    ///     The players involved in the current combat.
    /// </summary>
    private readonly List<CombatantPlayer> players;

    /// <summary>
    ///     The players involved in the current combat.
    /// </summary>
    public static List<CombatantPlayer> Players
    {
        get
        {
            CheckSingleton();
            return singleton.players;
        }
    }


    private int highlightedPlayer;


    /// <summary>
    ///     The positions of the players on screen.
    /// </summary>
    private static readonly Vector2[] PlayerPositions = new Vector2[5]
    {
        new(850f, 345f),
        new(980f, 260f),
        new(940f, 440f),
        new(1100f, 200f),
        new(1100f, 490f)
    };


    /// <summary>
    ///     Start the given player's combat turn.
    /// </summary>
    private void BeginPlayerTurn(CombatantPlayer player)
    {
        // check the parameter
        if (player == null)
        {
            throw new ArgumentNullException("player");
        }

        // set the highlight sprite
        highlightedCombatant = player;
        primaryTargetedCombatant = null;
        secondaryTargetedCombatants.Clear();

        Session.Session.Hud.ActionText = "Choose an Action";
    }


    /// <summary>
    ///     Begin the players' turn in this combat round.
    /// </summary>
    private void BeginPlayersTurn()
    {
        // set the player-turn
        isPlayersTurn = true;

        // reset each player for the next combat turn
        foreach (var player in players)
        {
            // reset the animation of living players
            if (!player.IsDeadOrDying)
            {
                player.State = Character.CharacterState.Idle;
            }

            // reset the turn-taken flag
            player.IsTurnTaken = false;
            // clear the combat action
            player.CombatAction = null;
            // advance each player
            player.AdvanceRound();
        }

        // set the action text on the HUD
        Session.Session.Hud.ActionText = "Your Party's Turn";

        // find the first player who is alive
        highlightedPlayer = 0;
        var firstPlayer = players[highlightedPlayer];
        while (firstPlayer.IsTurnTaken || firstPlayer.IsDeadOrDying)
        {
            highlightedPlayer = (highlightedPlayer + 1) % players.Count;
            firstPlayer = players[highlightedPlayer];
        }

        // start the first player's turn
        BeginPlayerTurn(firstPlayer);
    }


    /// <summary>
    ///     Check for whether all players have taken their turn.
    /// </summary>
    private bool IsPlayersTurnComplete
    {
        get { return players.TrueForAll(delegate(CombatantPlayer player) { return player.IsTurnTaken || player.IsDeadOrDying; }); }
    }


    /// <summary>
    ///     Check for whether the players have been wiped out and defeated.
    /// </summary>
    private bool ArePlayersDefeated
    {
        get { return players.TrueForAll(delegate(CombatantPlayer player) { return player.State == Character.CharacterState.Dead; }); }
    }


    /// <summary>
    ///     Retrieves the first living player, if any.
    /// </summary>
    private CombatantPlayer FirstPlayerTarget
    {
        get
        {
            // if there are no living players, then this is moot
            if (ArePlayersDefeated)
            {
                return null;
            }

            var playerIndex = 0;
            while (playerIndex < players.Count &&
                   players[playerIndex].IsDeadOrDying)
            {
                playerIndex++;
            }

            return players[playerIndex];
        }
    }

    #endregion


    #region Monsters

    /// <summary>
    ///     The monsters involved in the current combat.
    /// </summary>
    private readonly List<CombatantMonster> monsters;

    /// <summary>
    ///     The monsters involved in the current combat.
    /// </summary>
    public static List<CombatantMonster> Monsters
    {
        get
        {
            CheckSingleton();
            return singleton.monsters;
        }
    }


    /// <summary>
    ///     The positions of the monsters on the screen.
    /// </summary>
    private static readonly Vector2[] MonsterPositions = new Vector2[5]
    {
        new(480f, 345f),
        new(345f, 260f),
        new(370f, 440f),
        new(225f, 200f),
        new(225f, 490f)
    };


    /// <summary>
    ///     Start the given player's combat turn.
    /// </summary>
    private void BeginMonsterTurn(CombatantMonster monster)
    {
        // if it's null, find a random living monster who has yet to take their turn
        if (monster == null)
        {
            // don't bother if all monsters have finished
            if (IsMonstersTurnComplete)
            {
                return;
            }

            // pick random living monsters who haven't taken their turn
            do
            {
                monster = monsters[Session.Session.Random.Next(monsters.Count)];
            } while (monster.IsTurnTaken || monster.IsDeadOrDying);
        }

        // set the highlight sprite
        highlightedCombatant = monster;
        primaryTargetedCombatant = null;
        secondaryTargetedCombatants.Clear();

        // choose the action immediate
        monster.CombatAction = monster.ArtificialIntelligence.ChooseAction();
    }


    /// <summary>
    ///     Begin the monsters' turn in this combat round.
    /// </summary>
    private void BeginMonstersTurn()
    {
        // set the monster-turn
        isPlayersTurn = false;

        // reset each monster for the next combat turn
        foreach (var monster in monsters)
        {
            // reset the animations back to idle
            monster.Character.State = Character.CharacterState.Idle;
            // reset the turn-taken flag
            monster.IsTurnTaken = false;
            // clear the combat action
            monster.CombatAction = null;
            // advance the combatants
            monster.AdvanceRound();
        }

        // set the action text on the HUD
        Session.Session.Hud.ActionText = "Enemy Party's Turn";

        // start a Session.Random monster's turn
        BeginMonsterTurn(null);
    }


    /// <summary>
    ///     Check for whether all monsters have taken their turn.
    /// </summary>
    private bool IsMonstersTurnComplete
    {
        get { return monsters.TrueForAll(delegate(CombatantMonster monster) { return monster.IsTurnTaken || monster.IsDeadOrDying; }); }
    }


    /// <summary>
    ///     Check for whether the monsters have been wiped out and defeated.
    /// </summary>
    private bool AreMonstersDefeated
    {
        get { return monsters.TrueForAll(delegate(CombatantMonster monster) { return monster.State == Character.CharacterState.Dead; }); }
    }


    /// <summary>
    ///     Retrieves the first living monster, if any.
    /// </summary>
    private CombatantMonster FirstMonsterTarget
    {
        get
        {
            // if there are no living monsters, then this is moot
            if (AreMonstersDefeated)
            {
                return null;
            }

            var monsterIndex = 0;
            while (monsterIndex < monsters.Count &&
                   monsters[monsterIndex].IsDeadOrDying)
            {
                monsterIndex++;
            }

            return monsters[monsterIndex];
        }
    }

    #endregion


    #region Targeting

    /// <summary>
    ///     The currently highlighted player, if any.
    /// </summary>
    private Combatant highlightedCombatant;

    /// <summary>
    ///     The currently highlighted player, if any.
    /// </summary>
    public static Combatant HighlightedCombatant
    {
        get
        {
            CheckSingleton();
            return singleton.highlightedCombatant;
        }
    }


    /// <summary>
    ///     The current primary target, if any.
    /// </summary>
    private Combatant primaryTargetedCombatant;

    /// <summary>
    ///     The current primary target, if any.
    /// </summary>
    public static Combatant PrimaryTargetedCombatant
    {
        get
        {
            CheckSingleton();
            return singleton.primaryTargetedCombatant;
        }
    }


    /// <summary>
    ///     The current secondary targets, if any.
    /// </summary>
    private readonly List<Combatant> secondaryTargetedCombatants = new();

    /// <summary>
    ///     The current secondary targets, if any.
    /// </summary>
    public static List<Combatant> SecondaryTargetedCombatants
    {
        get
        {
            CheckSingleton();
            return singleton.secondaryTargetedCombatants;
        }
    }


    /// <summary>
    ///     Retrieves the first living enemy, if any.
    /// </summary>
    public static Combatant FirstEnemyTarget
    {
        get
        {
            CheckSingleton();

            if (IsPlayersTurn)
            {
                return singleton.FirstMonsterTarget;
            }

            return singleton.FirstPlayerTarget;
        }
    }


    /// <summary>
    ///     Retrieves the first living ally, if any.
    /// </summary>
    public static Combatant FirstAllyTarget
    {
        get
        {
            CheckSingleton();

            if (IsPlayersTurn)
            {
                return singleton.FirstPlayerTarget;
            }

            return singleton.FirstMonsterTarget;
        }
    }


    /// <summary>
    ///     Set the primary and any secondary targets.
    /// </summary>
    /// <param name="primaryTarget">The desired primary target.</param>
    /// <param name="adjacentTargets">
    ///     The number of simultaneous, adjacent targets affected by this spell.
    /// </param>
    private void SetTargets(Combatant primaryTarget, int adjacentTargets)
    {
        // set the primary target
        primaryTargetedCombatant = primaryTarget;

        // set any secondary targets
        secondaryTargetedCombatants.Clear();
        if (primaryTarget != null && adjacentTargets > 0)
        {
            // find out which side is targeted
            var isPlayerTarget = primaryTarget is CombatantPlayer;
            // find the index
            var primaryTargetIndex = 0;
            if (isPlayerTarget)
            {
                primaryTargetIndex = players.FindIndex(
                    delegate(CombatantPlayer player) { return player == primaryTarget; });
            }
            else
            {
                primaryTargetIndex = monsters.FindIndex(
                    delegate(CombatantMonster monster) { return monster == primaryTarget; });
            }

            // add the surrounding indices
            for (var i = 1; i <= adjacentTargets; i++)
            {
                var leftIndex = primaryTargetIndex - i;
                if (leftIndex >= 0)
                {
                    secondaryTargetedCombatants.Add(
                        isPlayerTarget ? players[leftIndex] : monsters[leftIndex]);
                }

                var rightIndex = primaryTargetIndex + i;
                if (rightIndex < (isPlayerTarget ? players.Count : monsters.Count))
                {
                    secondaryTargetedCombatants.Add(
                        isPlayerTarget ? players[rightIndex] : monsters[rightIndex]);
                }
            }
        }
    }

    #endregion


    #region Damage Sprites

    /// <summary>
    ///     A combat effect sprite, typically used for damage or healing numbers.
    /// </summary>
    private class CombatEffect
    {
        #region Updating

        /// <summary>
        ///     Updates the combat effect.
        /// </summary>
        /// <param name="elapsedSeconds">
        ///     The number of seconds elapsed since the last update.
        /// </param>
        public virtual void Update(float elapsedSeconds)
        {
            if (!isRiseComplete)
            {
                Rise += risePerSecond * elapsedSeconds;
                if (Rise > riseMaximum)
                {
                    Rise = riseMaximum;
                    isRiseComplete = true;
                }

                position = new Vector2(
                    OriginalPosition.X,
                    OriginalPosition.Y - Rise);
            }
        }

        #endregion


        #region Drawing

        /// <summary>
        ///     Draw the combat effect.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used to draw.</param>
        /// <param name="texture">The texture for the effect.</param>
        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            // check the parameter
            if (spriteBatch == null)
            {
                return;
            }

            // draw the texture
            if (texture != null)
            {
                spriteBatch.Draw(texture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(texture.Width / 2, texture.Height / 2),
                    1f,
                    SpriteEffects.None,
                    0.3f * Rise / 200f);
            }

            // draw the text
            if (!string.IsNullOrEmpty(Text))
            {
                spriteBatch.DrawString(Fonts.DamageFont,
                    text,
                    position,
                    Color.White,
                    0f,
                    new Vector2(textOrigin.X, textOrigin.Y),
                    1f,
                    SpriteEffects.None,
                    0.2f * Rise / 200f);
            }
        }

        #endregion

        #region Position

        /// <summary>
        ///     The starting position of the effect on the screen.
        /// </summary>
        public Vector2 OriginalPosition;


        /// <summary>
        ///     The current position of the effect on the screen.
        /// </summary>
        protected Vector2 position;

        /// <summary>
        ///     The current position of the effect on the screen.
        /// </summary>
        public Vector2 Position => position;

        #endregion


        #region Text

        /// <summary>
        ///     The text that appears on top of the effect.
        /// </summary>
        protected string text = string.Empty;

        /// <summary>
        ///     The text that appears on top of the effect.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                text = value;
                // recalculate the origin
                if (string.IsNullOrEmpty(text))
                {
                    textOrigin = Vector2.Zero;
                }
                else
                {
                    var textSize = Fonts.DamageFont.MeasureString(text);
                    textOrigin = new Vector2(
                        (float) Math.Ceiling(textSize.X / 2f),
                        (float) Math.Ceiling(textSize.Y / 2f));
                }
            }
        }

        /// <summary>
        ///     The drawing origin of the text on the effect.
        /// </summary>
        private Vector2 textOrigin = Vector2.Zero;

        #endregion


        #region Rise Animation

        /// <summary>
        ///     The speed at which the effect rises on the screen.
        /// </summary>
        private const int risePerSecond = 100;

        /// <summary>
        ///     The amount which the effect rises on the screen.
        /// </summary>
        private const int riseMaximum = 80;


        /// <summary>
        ///     The amount which the effect has already risen on the screen.
        /// </summary>
        public float Rise;


        /// <summary>
        ///     If true, the effect has finished rising.
        /// </summary>
        private bool isRiseComplete;

        /// <summary>
        ///     If true, the effect has finished rising.
        /// </summary>
        public bool IsRiseComplete => isRiseComplete;

        #endregion
    }


    #region Damage Effects

    /// <summary>
    ///     The sprite texture for all damage combat effects.
    /// </summary>
    private Texture2D damageCombatEffectTexture;


    /// <summary>
    ///     All current damage combat effects.
    /// </summary>
    private readonly List<CombatEffect> damageCombatEffects = new();


    /// <summary>
    ///     Adds a new damage combat effect to the scene.
    /// </summary>
    /// <param name="position">The position that the effect starts at.</param>
    /// <param name="damage">The damage statistics.</param>
    public static void AddNewDamageEffects(Vector2 position,
        StatisticsValue damage)
    {
        var startingRise = 0;

        CheckSingleton();

        if (damage.HealthPoints != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "HP\n" + damage.HealthPoints;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }

        if (damage.MagicPoints != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MP\n" + damage.MagicPoints;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }

        if (damage.PhysicalOffense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "PO\n" + damage.PhysicalOffense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }

        if (damage.PhysicalDefense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "PD\n" + damage.PhysicalDefense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }

        if (damage.MagicalOffense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MO\n" + damage.MagicalOffense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }

        if (damage.MagicalDefense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MD\n" + damage.MagicalDefense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.damageCombatEffects.Add(combatEffect);
        }
    }

    #endregion


    #region Healing Combat Effects

    /// <summary>
    ///     The sprite texture for all healing combat effects.
    /// </summary>
    private Texture2D healingCombatEffectTexture;


    /// <summary>
    ///     All current healing combat effects.
    /// </summary>
    private readonly List<CombatEffect> healingCombatEffects = new();


    /// <summary>
    ///     Adds a new healing combat effect to the scene.
    /// </summary>
    /// <param name="position">The position that the effect starts at.</param>
    /// <param name="damage">The healing statistics.</param>
    public static void AddNewHealingEffects(Vector2 position,
        StatisticsValue healing)
    {
        var startingRise = 0;

        CheckSingleton();

        if (healing.HealthPoints != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "HP\n" + healing.HealthPoints;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }

        if (healing.MagicPoints != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MP\n" + healing.MagicPoints;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }

        if (healing.PhysicalOffense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "PO\n" + healing.PhysicalOffense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }

        if (healing.PhysicalDefense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "PD\n" + healing.PhysicalDefense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }

        if (healing.MagicalOffense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MO\n" + healing.MagicalOffense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }

        if (healing.MagicalDefense != 0)
        {
            var combatEffect = new CombatEffect();
            combatEffect.OriginalPosition = position;
            combatEffect.Text = "MD\n" + healing.MagicalDefense;
            combatEffect.Rise = startingRise;
            startingRise -= 5;
            singleton.healingCombatEffects.Add(combatEffect);
        }
    }

    #endregion


    /// <summary>
    ///     Load the graphics data for the combat effect sprites.
    /// </summary>
    private void CreateCombatEffectSprites()
    {
        var content = Session.Session.ScreenManager.Game.Content;

        damageCombatEffectTexture =
            content.Load<Texture2D>(@"Textures\Combat\DamageIcon");
        healingCombatEffectTexture =
            content.Load<Texture2D>(@"Textures\Combat\HealingIcon");
    }


    /// <summary>
    ///     Draw all combat effect sprites.
    /// </summary>
    private void DrawCombatEffects(GameTime gameTime)
    {
        var elapsedSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
        var spriteBatch = Session.Session.ScreenManager.SpriteBatch;

        // update all effects
        foreach (var combatEffect in damageCombatEffects)
        {
            combatEffect.Update(elapsedSeconds);
        }

        foreach (var combatEffect in healingCombatEffects)
        {
            combatEffect.Update(elapsedSeconds);
        }

        // draw the damage effects
        if (damageCombatEffectTexture != null)
        {
            foreach (var combatEffect in damageCombatEffects)
            {
                combatEffect.Draw(spriteBatch, damageCombatEffectTexture);
            }
        }

        // draw the healing effects
        if (healingCombatEffectTexture != null)
        {
            foreach (var combatEffect in healingCombatEffects)
            {
                combatEffect.Draw(spriteBatch, healingCombatEffectTexture);
            }
        }

        // remove all complete effects
        Predicate<CombatEffect> removeCompleteEffects =
            delegate(CombatEffect combatEffect) { return combatEffect.IsRiseComplete; };
        damageCombatEffects.RemoveAll(removeCompleteEffects);
        healingCombatEffects.RemoveAll(removeCompleteEffects);
    }

    #endregion


    #region Selection Sprites

    /// <summary>
    ///     The animating sprite that draws over the highlighted character.
    /// </summary>
    private readonly AnimatingSprite highlightForegroundSprite = new();

    /// <summary>
    ///     The animating sprite that draws behind the highlighted character.
    /// </summary>
    private readonly AnimatingSprite highlightBackgroundSprite = new();

    /// <summary>
    ///     The animating sprite that draws behind the primary target character.
    /// </summary>
    private readonly AnimatingSprite primaryTargetSprite = new();

    /// <summary>
    ///     The animating sprite that draws behind any secondary target characters.
    /// </summary>
    private readonly AnimatingSprite secondaryTargetSprite = new();


    /// <summary>
    ///     Create the selection sprite objects.
    /// </summary>
    private void CreateSelectionSprites()
    {
        var content = Session.Session.ScreenManager.Game.Content;

        var frameDimensions = new Point(76, 58);
        highlightForegroundSprite.FramesPerRow = 6;
        highlightForegroundSprite.FrameDimensions = frameDimensions;
        highlightForegroundSprite.AddAnimation(
            new Animation("Selection", 1, 4, 100, true));
        highlightForegroundSprite.PlayAnimation(0);
        highlightForegroundSprite.SourceOffset =
            new Vector2(frameDimensions.X / 2f, 40f);
        highlightForegroundSprite.Texture =
            content.Load<Texture2D>(@"Textures\Combat\TilesheetSprangles");

        frameDimensions = new Point(102, 54);
        highlightBackgroundSprite.FramesPerRow = 4;
        highlightBackgroundSprite.FrameDimensions = frameDimensions;
        highlightBackgroundSprite.AddAnimation(
            new Animation("Selection", 1, 4, 100, true));
        highlightBackgroundSprite.PlayAnimation(0);
        highlightBackgroundSprite.SourceOffset =
            new Vector2(frameDimensions.X / 2f, frameDimensions.Y / 2f);
        highlightBackgroundSprite.Texture =
            content.Load<Texture2D>(@"Textures\Combat\CharSelectionRing");

        primaryTargetSprite.FramesPerRow = 4;
        primaryTargetSprite.FrameDimensions = frameDimensions;
        primaryTargetSprite.AddAnimation(
            new Animation("Selection", 1, 4, 100, true));
        primaryTargetSprite.PlayAnimation(0);
        primaryTargetSprite.SourceOffset =
            new Vector2(frameDimensions.X / 2f, frameDimensions.Y / 2f);
        primaryTargetSprite.Texture =
            content.Load<Texture2D>(@"Textures\Combat\Target1SelectionRing");

        secondaryTargetSprite.FramesPerRow = 4;
        secondaryTargetSprite.FrameDimensions = frameDimensions;
        secondaryTargetSprite.AddAnimation(
            new Animation("Selection", 1, 4, 100, true));
        secondaryTargetSprite.PlayAnimation(0);
        secondaryTargetSprite.SourceOffset =
            new Vector2(frameDimensions.X / 2f, frameDimensions.Y / 2f);
        secondaryTargetSprite.Texture =
            content.Load<Texture2D>(@"Textures\Combat\Target2SelectionRing");
    }


    /// <summary>
    ///     Draw the highlight sprites.
    /// </summary>
    private void DrawSelectionSprites(GameTime gameTime)
    {
        var spriteBatch = Session.Session.ScreenManager.SpriteBatch;
        var viewport = Session.Session.ScreenManager.GraphicsDevice.Viewport;

        // update the animations
        var elapsedSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
        highlightForegroundSprite.UpdateAnimation(elapsedSeconds);
        highlightBackgroundSprite.UpdateAnimation(elapsedSeconds);
        primaryTargetSprite.UpdateAnimation(elapsedSeconds);
        secondaryTargetSprite.UpdateAnimation(elapsedSeconds);

        // draw the highlighted-player sprite, if any
        if (highlightedCombatant != null)
        {
            highlightBackgroundSprite.Draw(spriteBatch,
                highlightedCombatant.Position,
                1f - (highlightedCombatant.Position.Y - 1) / viewport.Height);
            highlightForegroundSprite.Draw(spriteBatch,
                highlightedCombatant.Position,
                1f - (highlightedCombatant.Position.Y + 1) / viewport.Height);
        }

        // draw the primary target sprite and name, if any
        if (primaryTargetedCombatant != null)
        {
            primaryTargetSprite.Draw(spriteBatch,
                primaryTargetedCombatant.Position,
                1f - (primaryTargetedCombatant.Position.Y - 1) / viewport.Height);
            if (primaryTargetedCombatant.Character is Monster)
            {
                Fonts.DrawCenteredText(spriteBatch,
                    Fonts.DamageFont,
#if DEBUG
                    primaryTargetedCombatant.Character.Name + "\n" +
                    primaryTargetedCombatant.Statistics.HealthPoints + "/" +
                    primaryTargetedCombatant.Character.CharacterStatistics.HealthPoints,
#else
                        primaryTargetedCombatant.Character.Name,
#endif
                    primaryTargetedCombatant.Position + new Vector2(0f, 42f),
                    Color.White);
            }
        }

        // draw the secondary target sprites on live enemies, if any
        foreach (var combatant in secondaryTargetedCombatants)
        {
            if (combatant.IsDeadOrDying)
            {
                continue;
            }

            secondaryTargetSprite.Draw(spriteBatch,
                combatant.Position,
                1f - (combatant.Position.Y - 1) / viewport.Height);
            if (combatant.Character is Monster)
            {
                Fonts.DrawCenteredText(spriteBatch,
                    Fonts.DamageFont,
#if DEBUG
                    combatant.Character.Name + "\n" +
                    combatant.Statistics.HealthPoints + "/" +
                    combatant.Character.CharacterStatistics.HealthPoints,
#else
                        combatant.Character.Name,
#endif
                    combatant.Position + new Vector2(0f, 42f),
                    Color.White);
            }
        }
    }

    #endregion


    #region Delays

    /// <summary>
    ///     Varieties of delays that are interspersed throughout the combat flow.
    /// </summary>
    private enum DelayType
    {
        /// <summary>
        ///     No delay at this time.
        /// </summary>
        NoDelay,

        /// <summary>
        ///     Delay at the start of combat.
        /// </summary>
        StartCombat,

        /// <summary>
        ///     Delay when one side turn's ends before the other side begins.
        /// </summary>
        EndRound,

        /// <summary>
        ///     Delay at the end of a character's turn before the next one begins.
        /// </summary>
        EndCharacterTurn,

        /// <summary>
        ///     Delay before a flee is attempted.
        /// </summary>
        FleeAttempt,

        /// <summary>
        ///     Delay when the party has fled from combat before combat ends.
        /// </summary>
        FleeSuccessful
    }

    /// <summary>
    ///     The current delay, if any (otherwise NoDelay).
    /// </summary>
    private DelayType delayType = DelayType.NoDelay;


    /// <summary>
    ///     Returns true if the combat engine is delaying for any reason.
    /// </summary>
    public static bool IsDelaying =>
        singleton == null ? false : singleton.delayType != DelayType.NoDelay;


    /// <summary>
    ///     The duration for all kinds of delays, in milliseconds.
    /// </summary>
    private const int totalDelay = 1000;


    /// <summary>
    ///     The duration of the delay so far.
    /// </summary>
    private int currentDelay;


    /// <summary>
    ///     Update any delays in the combat system.
    /// </summary>
    /// <remarks>
    ///     This function may cause combat to end, setting the singleton to null.
    /// </remarks>
    private void UpdateDelay(int elapsedMilliseconds)
    {
        if (delayType == DelayType.NoDelay)
        {
            return;
        }

        // increment the delay
        currentDelay += elapsedMilliseconds;

        // if the delay is ongoing, then we're done
        if (currentDelay < totalDelay)
        {
            return;
        }

        currentDelay = 0;

        // the delay has ended, so the operation implied by the DelayType happens
        switch (delayType)
        {
            case DelayType.StartCombat:
                // determine who goes first and start combat
                var whoseTurn = Session.Session.Random.Next(2);
                if (whoseTurn == 0)
                {
                    BeginPlayersTurn();
                }
                else
                {
                    BeginMonstersTurn();
                }

                delayType = DelayType.NoDelay;
                break;

            case DelayType.EndCharacterTurn:
                if (IsPlayersTurn)
                {
                    // check to see if the players' turn is complete
                    if (IsPlayersTurnComplete)
                    {
                        delayType = DelayType.EndRound;
                        break;
                    }

                    // find the next player
                    var highlightedIndex = players.FindIndex(
                        delegate(CombatantPlayer player)
                        {
                            return player ==
                                   highlightedCombatant as CombatantPlayer;
                        });
                    var nextIndex = (highlightedIndex + 1) % players.Count;
                    while (players[nextIndex].IsDeadOrDying ||
                           players[nextIndex].IsTurnTaken)
                    {
                        nextIndex = (nextIndex + 1) % players.Count;
                    }

                    BeginPlayerTurn(players[nextIndex]);
                }
                else
                {
                    // check to see if the monsters' turn is complete
                    if (IsMonstersTurnComplete)
                    {
                        delayType = DelayType.EndRound;
                        break;
                    }

                    // find the next monster
                    BeginMonsterTurn(null);
                }

                delayType = DelayType.NoDelay;
                break;

            case DelayType.EndRound:
                // check for turn completion
                if (IsPlayersTurn && IsPlayersTurnComplete)
                {
                    BeginMonstersTurn();
                }
                else if (!IsPlayersTurn && IsMonstersTurnComplete)
                {
                    BeginPlayersTurn();
                }

                delayType = DelayType.NoDelay;
                break;

            case DelayType.FleeAttempt:
                if (fleeThreshold <= 0)
                {
                    delayType = DelayType.EndCharacterTurn;
                    Session.Session.Hud.ActionText = "This Fight Cannot Be Escaped...";
                    if (highlightedCombatant != null)
                    {
                        highlightedCombatant.IsTurnTaken = true;
                    }
                }
                else if (CalculateFleeAttempt())
                {
                    delayType = DelayType.FleeSuccessful;
                    Session.Session.Hud.ActionText = "Your Party Has Fled!";
                }
                else
                {
                    delayType = DelayType.EndCharacterTurn;
                    Session.Session.Hud.ActionText = "Your Party Failed to Escape!";
                    if (highlightedCombatant != null)
                    {
                        highlightedCombatant.IsTurnTaken = true;
                    }
                }

                break;

            case DelayType.FleeSuccessful:
                EndCombat(CombatEndingState.Fled);
                delayType = DelayType.NoDelay;
                break;
        }
    }

    #endregion


    #region Starting Combat

    /// <summary>
    ///     Generates a list of CombatantPlayer objects from the party members.
    /// </summary>
    private static List<CombatantPlayer> GenerateCombatantsFromParty()
    {
        var generatedPlayers = new List<CombatantPlayer>();

        foreach (var player in Session.Session.Party.Players)
        {
            if (generatedPlayers.Count <= PlayerPositions.Length)
            {
                generatedPlayers.Add(new CombatantPlayer(player));
            }
        }

        return generatedPlayers;
    }

    /// <summary>
    ///     Start a new combat from the given FixedCombat object.
    /// </summary>
    public static void StartNewCombat(MapEntry<FixedCombat> fixedCombatEntry)
    {
        // check the parameter
        if (fixedCombatEntry == null)
        {
            throw new ArgumentNullException("fixedCombatEntry");
        }

        var fixedCombat = fixedCombatEntry.Content;
        if (fixedCombat == null)
        {
            throw new ArgumentException("fixedCombatEntry has no content.");
        }

        // generate the monster combatant list
        var generatedMonsters = new List<CombatantMonster>();
        foreach (var entry in fixedCombat.Entries)
        {
            for (var i = 0; i < entry.Count; i++)
            {
                generatedMonsters.Add(
                    new CombatantMonster(entry.Content));
            }
        }

        // randomize the list of monsters
        var randomizedMonsters = new List<CombatantMonster>();
        while (generatedMonsters.Count > 0 &&
               randomizedMonsters.Count <= MonsterPositions.Length)
        {
            var index = Session.Session.Random.Next(generatedMonsters.Count);
            randomizedMonsters.Add(generatedMonsters[index]);
            generatedMonsters.RemoveAt(index);
        }

        // start the combat
        StartNewCombat(GenerateCombatantsFromParty(), randomizedMonsters, 0);
        singleton.fixedCombatEntry = fixedCombatEntry;
    }


    /// <summary>
    ///     Start a new combat from the given RandomCombat object.
    /// </summary>
    public static void StartNewCombat(RandomCombat randomCombat)
    {
        // check the parameter
        if (randomCombat == null)
        {
            throw new ArgumentNullException("randomCombat");
        }

        // determine how many monsters will be in the combat
        var monsterCount =
            randomCombat.MonsterCountRange.GenerateValue(Session.Session.Random);

        // determine the total probability
        var totalWeight = 0;
        foreach (var entry in randomCombat.Entries)
        {
            totalWeight += entry.Weight;
        }

        // generate each monster
        var generatedMonsters = new List<CombatantMonster>();
        for (var i = 0; i < monsterCount; i++)
        {
            var monsterChoice = Session.Session.Random.Next(totalWeight);
            foreach (var entry in randomCombat.Entries)
            {
                if (monsterChoice < entry.Weight)
                {
                    generatedMonsters.Add(
                        new CombatantMonster(entry.Content));
                    break;
                }

                monsterChoice -= entry.Weight;
            }
        }

        // randomize the list of monsters
        var randomizedMonsters = new List<CombatantMonster>();
        while (generatedMonsters.Count > 0 &&
               randomizedMonsters.Count <= MonsterPositions.Length)
        {
            var index = Session.Session.Random.Next(generatedMonsters.Count);
            randomizedMonsters.Add(generatedMonsters[index]);
            generatedMonsters.RemoveAt(index);
        }

        // start the combat
        StartNewCombat(GenerateCombatantsFromParty(),
            randomizedMonsters,
            randomCombat.FleeProbability);
    }


    /// <summary>
    ///     Start a new combat between the party and a group of monsters.
    /// </summary>
    /// <param name="players">The player combatants.</param>
    /// <param name="monsters">The monster combatants.</param>
    /// <param name="fleeThreshold">The odds of success when fleeing.</param>
    private static void StartNewCombat(List<CombatantPlayer> players,
        List<CombatantMonster> monsters,
        int fleeThreshold)
    {
        // check if we are already in combat
        if (singleton != null)
        {
            throw new InvalidOperationException(
                "There can only be one combat at a time.");
        }

        // create the new CombatEngine object
        singleton = new CombatEngine(players, monsters, fleeThreshold);
    }


    /// <summary>
    ///     Construct a new CombatEngine object.
    /// </summary>
    /// <param name="players">The player combatants.</param>
    /// <param name="monsters">The monster combatants.</param>
    /// <param name="fleeThreshold">The odds of success when fleeing.</param>
    private CombatEngine(List<CombatantPlayer> players,
        List<CombatantMonster> monsters,
        int fleeThreshold)
    {
        // check the parameters
        if (players == null || players.Count <= 0 ||
            players.Count > PlayerPositions.Length)
        {
            throw new ArgumentException("players");
        }

        if (monsters == null || monsters.Count <= 0 ||
            monsters.Count > MonsterPositions.Length)
        {
            throw new ArgumentException("monsters");
        }

        // assign the parameters
        this.players = players;
        this.monsters = monsters;
        this.fleeThreshold = fleeThreshold;

        // assign positions
        for (var i = 0; i < players.Count; i++)
        {
            if (i >= PlayerPositions.Length)
            {
                break;
            }

            players[i].Position =
                players[i].OriginalPosition =
                    PlayerPositions[i];
        }

        for (var i = 0; i < monsters.Count; i++)
        {
            if (i >= MonsterPositions.Length)
            {
                break;
            }

            monsters[i].Position =
                monsters[i].OriginalPosition =
                    MonsterPositions[i];
        }

        // sort the monsters by the y coordinates, descending
        monsters.Sort(delegate(CombatantMonster monster1, CombatantMonster monster2)
        {
            return monster2.OriginalPosition.Y.CompareTo(
                monster1.OriginalPosition.Y);
        });

        // create the selection sprites
        CreateSelectionSprites();

        // create the combat effect sprites
        CreateCombatEffectSprites();

        // start the first combat turn after a delay
        delayType = DelayType.StartCombat;

        // start the combat music
        AudioManager.PushMusic(TileEngine.TileEngine.Map.CombatMusicCueName);
    }

    #endregion


    #region Fleeing Combat

    public static void AttemptFlee()
    {
        CheckSingleton();

        if (!IsPlayersTurn)
        {
            throw new InvalidOperationException("Only the players may flee.");
        }


        singleton.delayType = DelayType.FleeAttempt;
        Session.Session.Hud.ActionText = "Attempting to Escape...";
    }


    /// <summary>
    ///     The odds of being able to flee this combat, from 0 to 100.
    /// </summary>
    private readonly int fleeThreshold;


    /// <summary>
    ///     Calculate an attempted escape from the combat.
    /// </summary>
    /// <returns>If true, the escape succeeds.</returns>
    private bool CalculateFleeAttempt()
    {
        return Session.Session.Random.Next(100) < fleeThreshold;
    }

    #endregion


    #region Ending Combat

    /// <summary>
    ///     End the combat
    /// </summary>
    /// <param name="combatEndState"></param>
    private void EndCombat(CombatEndingState combatEndingState)
    {
        // go back to the non-combat music
        AudioManager.PopMusic();

        switch (combatEndingState)
        {
            case CombatEndingState.Victory:
                var experienceReward = 0;
                var goldReward = 0;
                var gearRewards = new List<Gear>();
                var gearRewardNames = new List<string>();
                // calculate the rewards from the monsters
                foreach (var combatantMonster in monsters)
                {
                    var monster = combatantMonster.Monster;
                    Session.Session.Party.AddMonsterKill(monster);
                    experienceReward +=
                        monster.CalculateExperienceReward(Session.Session.Random);
                    goldReward += monster.CalculateGoldReward(Session.Session.Random);
                    gearRewardNames.AddRange(
                        monster.CalculateGearDrop(Session.Session.Random));
                }

                foreach (var gearRewardName in gearRewardNames)
                {
                    gearRewards.Add(Session.Session.ScreenManager.Game.Content.Load<Gear>(
                        Path.Combine(@"Gear", gearRewardName)));
                }

                // add the reward screen
                Session.Session.ScreenManager.AddScreen(new RewardsScreen(
                    RewardsScreen.RewardScreenMode.Combat,
                    experienceReward,
                    goldReward,
                    gearRewards));
                // remove the fixed combat entry, if this wasn't a random fight
                if (FixedCombatEntry != null)
                {
                    Session.Session.RemoveFixedCombat(FixedCombatEntry);
                }

                break;

            case CombatEndingState.Loss: // game over
                var screenManager = Session.Session.ScreenManager;
                // end the session
                Session.Session.EndSession();
                // add the game-over screen
                screenManager.AddScreen(new GameOverScreen());
                break;

            case CombatEndingState.Fled:
                break;
        }

        // clear the singleton
        singleton = null;
    }


    /// <summary>
    ///     Ensure that there is no combat happening right now.
    /// </summary>
    public static void ClearCombat()
    {
        // clear the singleton
        if (singleton != null)
        {
            singleton = null;
        }
    }

    #endregion


    #region Updating

    /// <summary>
    ///     Update the combat engine for this frame.
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        // if there is no active combat, then there's nothing to update
        // -- this will be called every frame, so there should be no exception for
        //    calling this method outside of combat
        if (singleton == null)
        {
            return;
        }

        // update the singleton
        singleton.UpdateCombatEngine(gameTime);
    }


    /// <summary>
    ///     Update the combat engine for this frame.
    /// </summary>
    private void UpdateCombatEngine(GameTime gameTime)
    {
        // check for the end of combat
        if (ArePlayersDefeated)
        {
            EndCombat(CombatEndingState.Loss);
            return;
        }

        if (AreMonstersDefeated)
        {
            EndCombat(CombatEndingState.Victory);
            return;
        }

        // update the target selections
        if (highlightedCombatant != null &&
            highlightedCombatant.CombatAction != null)
        {
            SetTargets(highlightedCombatant.CombatAction.Target,
                highlightedCombatant.CombatAction.AdjacentTargets);
        }

        // update the delay
        UpdateDelay(gameTime.ElapsedGameTime.Milliseconds);
        // UpdateDelay might cause combat to end due to a successful escape,
        // which will set the singleton to null.
        if (singleton == null)
        {
            return;
        }

        // update the players
        foreach (var player in players)
        {
            player.Update(gameTime);
        }

        // update the monsters
        foreach (var monster in monsters)
        {
            monster.Update(gameTime);
        }

        // check for completion of the highlighted combatant
        if (delayType == DelayType.NoDelay &&
            highlightedCombatant != null && highlightedCombatant.IsTurnTaken)
        {
            delayType = DelayType.EndCharacterTurn;
        }

        // handle any player input
        HandleInput();
    }


    /// <summary>
    ///     Handle player input that affects the combat engine.
    /// </summary>
    private void HandleInput()
    {
        // only accept input during the players' turn
        // -- exit game, etc. is handled by GameplayScreen
        if (!IsPlayersTurn || IsPlayersTurnComplete ||
            highlightedCombatant == null)
        {
            return;
        }

#if DEBUG
        // cheat key
        if (InputManager.IsGamePadRightShoulderTriggered() ||
            InputManager.IsKeyTriggered(Keys.W))
        {
            EndCombat(CombatEndingState.Victory);
            return;
        }
#endif
        // handle input while choosing an action
        if (highlightedCombatant.CombatAction != null)
        {
            // skip if its turn is over or the action is already going
            if (highlightedCombatant.IsTurnTaken ||
                highlightedCombatant.CombatAction.Stage !=
                CombatAction.CombatActionStage.NotStarted)
            {
                return;
            }

            // back out of the action
            if (InputManager.IsActionTriggered(InputManager.Action.Back))
            {
                highlightedCombatant.CombatAction = null;
                SetTargets(null, 0);
                return;
            }

            // start the action
            if (InputManager.IsActionTriggered(InputManager.Action.Ok))
            {
                highlightedCombatant.CombatAction.Start();
                return;
            }

            // go to the next target
            if (InputManager.IsActionTriggered(InputManager.Action.TargetUp))
            {
                // cycle through monsters or party members
                if (highlightedCombatant.CombatAction.IsOffensive)
                {
                    // find the index of the current target
                    var newIndex = monsters.FindIndex(
                        delegate(CombatantMonster monster) { return primaryTargetedCombatant == monster; });
                    // find the next living target
                    do
                    {
                        newIndex = (newIndex + 1) % monsters.Count;
                    } while (monsters[newIndex].IsDeadOrDying);

                    // set the new target
                    highlightedCombatant.CombatAction.Target = monsters[newIndex];
                }
                else
                {
                    // find the index of the current target
                    var newIndex = players.FindIndex(
                        delegate(CombatantPlayer player) { return primaryTargetedCombatant == player; });
                    // find the next active, living target
                    do
                    {
                        newIndex = (newIndex + 1) % players.Count;
                    } while (players[newIndex].IsDeadOrDying);

                    // set the new target
                    highlightedCombatant.CombatAction.Target = players[newIndex];
                }

                return;
            }
            // go to the previous target

            if (InputManager.IsActionTriggered(InputManager.Action.TargetDown))
            {
                // cycle through monsters or party members
                if (highlightedCombatant.CombatAction.IsOffensive)
                {
                    // find the index of the current target
                    var newIndex = monsters.FindIndex(
                        delegate(CombatantMonster monster) { return primaryTargetedCombatant == monster; });
                    // find the previous active, living target
                    do
                    {
                        newIndex--;
                        while (newIndex < 0)
                        {
                            newIndex += monsters.Count;
                        }
                    } while (monsters[newIndex].IsDeadOrDying);

                    // set the new target
                    highlightedCombatant.CombatAction.Target = monsters[newIndex];
                }
                else
                {
                    // find the index of the current target
                    var newIndex = players.FindIndex(
                        delegate(CombatantPlayer player) { return primaryTargetedCombatant == player; });
                    // find the previous living target
                    do
                    {
                        newIndex--;
                        while (newIndex < 0)
                        {
                            newIndex += players.Count;
                        }
                    } while (players[newIndex].IsDeadOrDying);

                    // set the new target
                    highlightedCombatant.CombatAction.Target = players[newIndex];
                }
            }
        }
        else // choosing which character will act
        {
            // move to the previous living character
            if (InputManager.IsActionTriggered(
                    InputManager.Action.ActiveCharacterLeft))
            {
                var newHighlightedPlayer = highlightedPlayer;
                do
                {
                    newHighlightedPlayer--;
                    while (newHighlightedPlayer < 0)
                    {
                        newHighlightedPlayer += players.Count;
                    }
                } while (players[newHighlightedPlayer].IsDeadOrDying ||
                         players[newHighlightedPlayer].IsTurnTaken);

                if (newHighlightedPlayer != highlightedPlayer)
                {
                    highlightedPlayer = newHighlightedPlayer;
                    BeginPlayerTurn(players[highlightedPlayer]);
                }

                return;
            }
            // move to the next living character

            if (InputManager.IsActionTriggered(
                    InputManager.Action.ActiveCharacterRight))
            {
                var newHighlightedPlayer = highlightedPlayer;
                do
                {
                    newHighlightedPlayer =
                        (newHighlightedPlayer + 1) % players.Count;
                } while (players[newHighlightedPlayer].IsDeadOrDying ||
                         players[newHighlightedPlayer].IsTurnTaken);

                if (newHighlightedPlayer != highlightedPlayer)
                {
                    highlightedPlayer = newHighlightedPlayer;
                    BeginPlayerTurn(players[highlightedPlayer]);
                }

                return;
            }

            Session.Session.Hud.UpdateActionsMenu();
        }
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the combat for this frame.
    /// </summary>
    public static void Draw(GameTime gameTime)
    {
        // if there is no active combat, then there's nothing to draw
        // -- this will be called every frame, so there should be no exception for
        //    calling this method outside of combat
        if (singleton == null)
        {
            return;
        }

        // update the singleton
        singleton.DrawCombatEngine(gameTime);
    }


    /// <summary>
    ///     Draw the combat for this frame.
    /// </summary>
    private void DrawCombatEngine(GameTime gameTime)
    {
        // draw the players
        foreach (var player in players)
        {
            player.Draw(gameTime);
        }

        // draw the monsters
        foreach (var monster in monsters)
        {
            monster.Draw(gameTime);
        }

        // draw the selection animations
        DrawSelectionSprites(gameTime);

        // draw the combat effects
        DrawCombatEffects(gameTime);
    }

    #endregion
}
