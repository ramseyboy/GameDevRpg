#region File Description

//-----------------------------------------------------------------------------
// StatisticsScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGame.ScreenManager;
using RolePlayingGameData.Animation;
using RolePlayingGameData.Characters;
using RolePlayingGameData.Data;
using RolePlayingGameData.Gear;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Draws the statistics for a particular Player.
/// </summary>
internal class StatisticsScreen : GameScreen
{
    /// <summary>
    ///     This is used ass the Player NPC to display statistics of the player
    /// </summary>
    private Player player;


    #region Updating

    /// <summary>
    ///     Handle user input.
    /// </summary>
    public override void HandleInput()
    {
        // exit the screen
        if (InputManager.IsActionTriggered(InputManager.Action.Back))
        {
            ExitScreen();
            return;
        }
        // shows the spells for this player

        if (InputManager.IsActionTriggered(InputManager.Action.Ok))
        {
            ScreenManager.AddScreen(new SpellbookScreen(player,
                player.CharacterStatistics));
            return;
        }
        // show the equipment for this player, allowing the user to unequip

        if (InputManager.IsActionTriggered(InputManager.Action.TakeView))
        {
            ScreenManager.AddScreen(new EquipmentScreen(player));
            return;
        }

        if (Session.Session.Party.Players.Contains(player)) // player is in the party
        {
            // move to the previous screen
            if (InputManager.IsActionTriggered(InputManager.Action.PageLeft))
            {
                ExitScreen();
                ScreenManager.AddScreen(new QuestLogScreen(null));
                return;
            }
            // move to the next screen

            if (InputManager.IsActionTriggered(InputManager.Action.PageRight))
            {
                ExitScreen();
                ScreenManager.AddScreen(new InventoryScreen(true));
                return;
            }
            // move to the previous party member

            if (InputManager.IsActionTriggered(InputManager.Action.CursorUp))
            {
                playerIndex--;
                if (playerIndex < 0)
                {
                    playerIndex = Session.Session.Party.Players.Count - 1;
                }

                var newPlayer = Session.Session.Party.Players[playerIndex];
                if (newPlayer != player)
                {
                    player = newPlayer;
                    screenAnimation = new AnimatingSprite();
                    screenAnimation.FrameDimensions =
                        player.MapSprite.FrameDimensions;
                    screenAnimation.FramesPerRow = player.MapSprite.FramesPerRow;
                    screenAnimation.SourceOffset = player.MapSprite.SourceOffset;
                    screenAnimation.Texture = player.MapSprite.Texture;
                    screenAnimation.AddAnimation(
                        new Animation("Idle", 43, 48, 150, true));
                    screenAnimation.PlayAnimation(0);
                }
            }
            // move to the next party member
            else if (InputManager.IsActionTriggered(InputManager.Action.CursorDown))
            {
                playerIndex++;
                if (playerIndex >= Session.Session.Party.Players.Count)
                {
                    playerIndex = 0;
                }

                var newPlayer = Session.Session.Party.Players[playerIndex];
                if (newPlayer != player)
                {
                    player = newPlayer;
                    screenAnimation = new AnimatingSprite();
                    screenAnimation.FrameDimensions =
                        player.MapSprite.FrameDimensions;
                    screenAnimation.FramesPerRow = player.MapSprite.FramesPerRow;
                    screenAnimation.SourceOffset = player.MapSprite.SourceOffset;
                    screenAnimation.Texture = player.MapSprite.Texture;
                    screenAnimation.AddAnimation(
                        new Animation("Idle", 43, 48, 150, true));
                    screenAnimation.PlayAnimation(0);
                }
            }
        }
    }

    #endregion


    #region Graphics Data

    private Texture2D statisticsScreen;
    private Texture2D selectButton;
    private Texture2D backButton;
    private Texture2D dropButton;
    private Texture2D statisticsBorder;
    private Texture2D plankTexture;
    private Texture2D fadeTexture;
    private Texture2D goldIcon;
    private Texture2D scoreBoardTexture;
    private Texture2D rightTriggerButton;
    private Texture2D leftTriggerButton;
    private Texture2D borderLine;

    private Rectangle screenRectangle;
    private const int intervalBetweenEachInfo = 30;
    private int playerIndex;
    private AnimatingSprite screenAnimation;

    #endregion


    #region Positions

    private readonly Vector2 playerTextPosition = new(515, 200);
    private Vector2 currentTextPosition;
    private readonly Vector2 scoreBoardPosition = new(1160, 354);
    private Vector2 placeTextMid;
    private Vector2 statisticsNamePosition;
    private readonly Vector2 shieldPosition = new(1124, 253);
    private readonly Vector2 idlePortraitPosition = new(300f, 380f);
    private readonly Vector2 dropButtonPosition = new(900, 640);
    private readonly Vector2 statisticsBorderPosition = new(180, 147);
    private readonly Vector2 borderLinePosition = new(184, 523);
    private readonly Vector2 characterNamePosition = new(330, 180);
    private readonly Vector2 classNamePosition = new(330, 465);
    private Vector2 plankPosition;
    private readonly Vector2 goldIconPosition = new(490, 640);
    private readonly Vector2 weaponPosition = new(790, 220);
    private readonly Vector2 armorPosition = new(790, 346);
    private readonly Vector2 weaponTextPosition = new(790, 285);
    private readonly Vector2 leftTriggerPosition = new(340, 50);
    private readonly Vector2 rightTriggerPosition = new(900, 50);
    private readonly Vector2 iconPosition = new(100, 200);
    private readonly Vector2 selectButtonPosition = new(1150, 640);
    private readonly Vector2 backButtonPosition = new(80, 640);
    private readonly Vector2 goldPosition = new(565, 648);

    #endregion


    #region Initialization

    /// <summary>
    ///     Creates a new StatisticsScreen object for the first party member.
    /// </summary>
    public StatisticsScreen() : this(Session.Session.Party.Players[0])
    {
    }


    /// <summary>
    ///     Creates a new StatisticsScreen object for the given player.
    /// </summary>
    public StatisticsScreen(Player player)
    {
        IsPopup = true;
        this.player = player;

        screenAnimation = new AnimatingSprite();
        screenAnimation.FrameDimensions = player.MapSprite.FrameDimensions;
        screenAnimation.FramesPerRow = player.MapSprite.FramesPerRow;
        screenAnimation.SourceOffset = player.MapSprite.SourceOffset;
        screenAnimation.Texture = player.MapSprite.Texture;
        screenAnimation.AddAnimation(new Animation("Idle", 43, 48, 150, true));
        screenAnimation.PlayAnimation(0);
    }


    /// <summary>
    ///     Loads graphics content from the content manager.
    /// </summary>
    public override void LoadContent()
    {
        var content = ScreenManager.Game.Content;
        var viewport = ScreenManager.GraphicsDevice.Viewport;

        statisticsScreen =
            content.Load<Texture2D>(@"Textures\GameScreens\GameScreenBkgd");
        plankTexture =
            content.Load<Texture2D>(@"Textures\MainMenu\MainMenuPlank03");
        scoreBoardTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\CountShieldWithArrow");
        leftTriggerButton =
            content.Load<Texture2D>(@"Textures\Buttons\LeftTriggerButton");
        rightTriggerButton =
            content.Load<Texture2D>(@"Textures\Buttons\RightTriggerButton");
        backButton =
            content.Load<Texture2D>(@"Textures\Buttons\BButton");
        selectButton =
            content.Load<Texture2D>(@"Textures\Buttons\AButton");
        dropButton =
            content.Load<Texture2D>(@"Textures\Buttons\YButton");
        statisticsBorder =
            content.Load<Texture2D>(@"Textures\GameScreens\StatsBorderTable");
        borderLine =
            content.Load<Texture2D>(@"Textures\GameScreens\LineBorder");
        goldIcon =
            content.Load<Texture2D>(@"Textures\GameScreens\GoldIcon");
        fadeTexture =
            content.Load<Texture2D>(@"Textures\GameScreens\FadeScreen");

        screenRectangle = new Rectangle(viewport.X,
            viewport.Y,
            viewport.Width,
            viewport.Height);

        statisticsNamePosition.X = (viewport.Width -
                                    Fonts.HeaderFont.MeasureString("Statistics").X) / 2;
        statisticsNamePosition.Y = 90f;

        plankPosition.X = (viewport.Width - plankTexture.Width) / 2;
        plankPosition.Y = 67f;
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draws the screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        screenAnimation.UpdateAnimation(
            (float) gameTime.ElapsedGameTime.TotalSeconds);

        ScreenManager.SpriteBatch.Begin();
        DrawStatistics();
        ScreenManager.SpriteBatch.End();
    }


    /// <summary>
    ///     Draws the player statistics.
    /// </summary>
    private void DrawStatistics()
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        // Draw faded screen
        spriteBatch.Draw(fadeTexture, screenRectangle, Color.White);

        // Draw the Statistics Screen
        spriteBatch.Draw(statisticsScreen, screenRectangle, Color.White);
        spriteBatch.Draw(plankTexture, plankPosition, Color.White);
        spriteBatch.Draw(statisticsBorder, statisticsBorderPosition, Color.White);

        spriteBatch.Draw(borderLine, borderLinePosition, Color.White);

        spriteBatch.Draw(screenAnimation.Texture,
            idlePortraitPosition,
            screenAnimation.SourceRectangle,
            Color.White,
            0f,
            new Vector2(screenAnimation.SourceOffset.X,
                screenAnimation.SourceOffset.Y),
            1f,
            SpriteEffects.None,
            0f);

        spriteBatch.DrawString(Fonts.HeaderFont,
            "Statistics",
            statisticsNamePosition,
            Fonts.TitleColor);
        DrawPlayerDetails();
        DrawButtons();
    }


    /// <summary>
    ///     D
    /// </summary>
    private void DrawButtons()
    {
        if (!IsActive)
        {
            return;
        }

        var spriteBatch = ScreenManager.SpriteBatch;
        var position = new Vector2();

        if (Session.Session.Party.Players.Contains(player))
        {
            // Left Trigger
            position = leftTriggerPosition;
            spriteBatch.Draw(leftTriggerButton, position, Color.White);

            // Draw Left Trigger Information
            position.Y += leftTriggerButton.Height;
            placeTextMid = Fonts.MainFont.MeasureString("Quest");
            position.X += leftTriggerButton.Width / 2 - placeTextMid.X / 2;
            spriteBatch.DrawString(Fonts.MainFont,
                "Quest",
                position,
                Color.Black);

            // Right Trigger
            position = rightTriggerPosition;
            spriteBatch.Draw(rightTriggerButton, position, Color.White);

            // Draw Right Trigger Information
            position.Y += rightTriggerButton.Height;
            placeTextMid = Fonts.MainFont.MeasureString("Items");
            position.X += leftTriggerButton.Width / 2 - placeTextMid.X / 2;
            spriteBatch.DrawString(Fonts.MainFont,
                "Items",
                position,
                Color.Black);
        }

        // Back Button
        spriteBatch.Draw(backButton, backButtonPosition, Color.White);

        spriteBatch.Draw(selectButton, selectButtonPosition, Color.White);
        position = selectButtonPosition;
        position.X -= Fonts.MainFont.MeasureString("Spellbook").X + 10f;
        position.Y += 5;
        spriteBatch.DrawString(Fonts.MainFont,
            "Spellbook",
            position,
            Color.White);

        // Draw Back
        position = backButtonPosition;
        position.X += backButton.Width + 10f;
        position.Y += 5;
        spriteBatch.DrawString(Fonts.MainFont,
            "Back",
            position,
            Color.White);

        // Draw drop Button
        spriteBatch.Draw(dropButton, dropButtonPosition, Color.White);
        position = dropButtonPosition;
        position.X -= Fonts.MainFont.MeasureString("Equipment").X + 10;
        position.Y += 5;
        spriteBatch.DrawString(Fonts.MainFont,
            "Equipment",
            position,
            Color.White);

        // Draw Gold Icon
        spriteBatch.Draw(goldIcon, goldIconPosition, Color.White);
    }


    /// <summary>
    ///     Draws player information.
    /// </summary>
    private void DrawPlayerDetails()
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        if (player != null)
        {
            currentTextPosition.X = playerTextPosition.X;
            currentTextPosition.Y = playerTextPosition.Y;

            // Current Level
            spriteBatch.DrawString(Fonts.MainFont,
                "Level: " +
                player.CharacterLevel,
                currentTextPosition,
                Color.Black);

            // Health Points
            currentTextPosition.Y += intervalBetweenEachInfo;
            spriteBatch.DrawString(Fonts.MainFont,
                "Health Points: " +
                player.CurrentStatistics.HealthPoints + "/" +
                player.CharacterStatistics.HealthPoints,
                currentTextPosition,
                Color.Black);

            // Magic Points
            currentTextPosition.Y += intervalBetweenEachInfo;
            spriteBatch.DrawString(Fonts.MainFont,
                "Magic Points: " +
                player.CurrentStatistics.MagicPoints + "/" +
                player.CharacterStatistics.MagicPoints,
                currentTextPosition,
                Color.Black);

            // Experience Details
            currentTextPosition.Y += intervalBetweenEachInfo;
            if (player.IsMaximumCharacterLevel)
            {
                spriteBatch.DrawString(Fonts.MainFont,
                    "Experience: Maximum",
                    currentTextPosition,
                    Color.Black);
            }
            else
            {
                spriteBatch.DrawString(Fonts.MainFont,
                    "Experience: " +
                    player.Experience + "/" +
                    player.ExperienceForNextLevel,
                    currentTextPosition,
                    Color.Black);
            }

            DrawEquipmentInfo(player);

            DrawModifiers(player);

            DrawEquipmentStatistics(player);

            if (player == null)
            {
                DrawPlayerCount();
            }
        }

        // Draw Gold
        spriteBatch.DrawString(Fonts.MainFont,
            Fonts.GetGoldString(Session.Session.Party.PartyGold),
            goldPosition,
            Color.White);
    }


    /// <summary>
    ///     Draws Current Selected Player Count to Total Player count
    /// </summary>
    private void DrawPlayerCount()
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var position = new Vector2();

        // Draw the ScoreBoard
        spriteBatch.Draw(scoreBoardTexture,
            shieldPosition,
            Color.White);

        position = scoreBoardPosition;
        position.X = scoreBoardPosition.X -
                     Fonts.GearInfoFont.MeasureString((playerIndex + 1).ToString()).X / 2;

        // Draw Current Selected Player Count
        spriteBatch.DrawString(Fonts.GearInfoFont,
            (playerIndex + 1).ToString(),
            position,
            Fonts.CountColor);

        position.X = scoreBoardPosition.X - Fonts.GearInfoFont.MeasureString(
            Session.Session.Party.Players.Count.ToString()).X / 2;
        position.Y += 30;

        // Draw Total Player count
        spriteBatch.DrawString(Fonts.GearInfoFont,
            Session.Session.Party.Players.Count.ToString(),
            position,
            Fonts.CountColor);
    }


    /// <summary>
    ///     Draw Equipment Info of the player selected
    /// </summary>
    /// <param name="selectedPlayer">The selected player</param>
    private void DrawEquipmentInfo(Player selectedPlayer)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        // Character name
        currentTextPosition = characterNamePosition;

        currentTextPosition.X -=
            Fonts.HeaderFont.MeasureString(selectedPlayer.Name).X / 2;
        spriteBatch.DrawString(Fonts.HeaderFont,
            selectedPlayer.Name,
            currentTextPosition,
            Fonts.TitleColor);

        // Class name
        currentTextPosition = classNamePosition;
        currentTextPosition.X -= Fonts.GearInfoFont.MeasureString(
            selectedPlayer.CharacterClass.Name).X / 2;
        spriteBatch.DrawString(Fonts.GearInfoFont,
            selectedPlayer.CharacterClass.Name,
            currentTextPosition,
            Color.Black);
    }


    /// <summary>
    ///     Draw Base Amount Plus any Modifiers
    /// </summary>
    /// <param name="selectedPlayer">The selected player</param>
    private void DrawModifiers(Player selectedPlayer)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        currentTextPosition.X = playerTextPosition.X;
        currentTextPosition.Y = playerTextPosition.Y + 150;

        // PO + Modifiers
        spriteBatch.DrawString(Fonts.MainFont,
            "Physical Offense: " +
            selectedPlayer.CurrentStatistics.PhysicalOffense,
            currentTextPosition,
            Color.Black);

        // PD + Modifiers
        currentTextPosition.Y += intervalBetweenEachInfo;
        spriteBatch.DrawString(Fonts.MainFont,
            "Physical Defense: " +
            selectedPlayer.CurrentStatistics.PhysicalDefense,
            currentTextPosition,
            Color.Black);

        // MO + Modifiers
        currentTextPosition.Y += intervalBetweenEachInfo;
        spriteBatch.DrawString(Fonts.MainFont,
            "Magical Offense: " +
            selectedPlayer.CurrentStatistics.MagicalOffense,
            currentTextPosition,
            Color.Black);

        // MD + Modifiers
        currentTextPosition.Y += intervalBetweenEachInfo;
        spriteBatch.DrawString(Fonts.MainFont,
            "Magical Defense: " +
            selectedPlayer.CurrentStatistics.MagicalDefense,
            currentTextPosition,
            Color.Black);
    }


    /// <summary>
    ///     Draw the equipment statistics
    /// </summary>
    /// <param name="selectedPlayer">The selected Player</param>
    private void DrawEquipmentStatistics(Player selectedPlayer)
    {
        var spriteBatch = ScreenManager.SpriteBatch;

        var position = weaponPosition;
        var healthDamageRange = new Int32Range();
        healthDamageRange.Minimum = healthDamageRange.Maximum =
            selectedPlayer.CurrentStatistics.PhysicalOffense;
        var weapon = selectedPlayer.GetEquippedWeapon();
        if (weapon != null)
        {
            weapon.DrawIcon(ScreenManager.SpriteBatch, position);
            healthDamageRange += weapon.TargetDamageRange;
        }

        position = armorPosition;
        var healthDefenseRange = new Int32Range();
        healthDefenseRange.Minimum = healthDefenseRange.Maximum =
            selectedPlayer.CurrentStatistics.PhysicalDefense;
        var magicDefenseRange = new Int32Range();
        magicDefenseRange.Minimum = magicDefenseRange.Maximum =
            selectedPlayer.CurrentStatistics.MagicalDefense;
        for (var i = 0; i < 4; i++)
        {
            var armor = selectedPlayer.GetEquippedArmor((Armor.ArmorSlot) i);
            if (armor != null)
            {
                armor.DrawIcon(ScreenManager.SpriteBatch, position);
                healthDefenseRange += armor.OwnerHealthDefenseRange;
                magicDefenseRange += armor.OwnerMagicDefenseRange;
            }

            position.X += 68;
        }

        position = weaponTextPosition;
        spriteBatch.DrawString(Fonts.MainFont,
            "Weapon Attack: " + "(" +
            healthDamageRange.Minimum + "," +
            healthDamageRange.Maximum + ")",
            position,
            Color.Black);

        position.Y += 130;
        spriteBatch.DrawString(Fonts.MainFont,
            "Weapon Defense: " + "(" +
            healthDefenseRange.Minimum + "," +
            healthDefenseRange.Maximum + ")",
            position,
            Color.Black);

        position.Y += 30;
        spriteBatch.DrawString(Fonts.MainFont,
            "Spell Defense: " + "(" +
            magicDefenseRange.Minimum + "," +
            magicDefenseRange.Maximum + ")",
            position,
            Color.Black);
    }

    #endregion
}
