#region File Description

//-----------------------------------------------------------------------------
// Session.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RolePlayingGameData;
using StorageReplacement;

#endregion

namespace RolePlaying;

internal class Session
{
    #region Singleton

    /// <summary>
    ///     The single Session instance that can be active at a time.
    /// </summary>
    private static Session singleton;

    #endregion


    #region Initialization

    /// <summary>
    ///     Private constructor of a Session object.
    /// </summary>
    /// <remarks>
    ///     The lack of public constructors forces the singleton model.
    /// </remarks>
    private Session(ScreenManager screenManager, GameplayScreen gameplayScreen)
    {
        // check the parameter
        if (screenManager == null)
        {
            throw new ArgumentNullException("screenManager");
        }

        if (gameplayScreen == null)
        {
            throw new ArgumentNullException("gameplayScreen");
        }

        // assign the parameter
        this.screenManager = screenManager;
        this.gameplayScreen = gameplayScreen;

        // create the HUD interface
        hud = new Hud(screenManager);
        hud.LoadContent();
    }

    #endregion


    #region State Data

    /// <summary>
    ///     Returns true if there is an active session.
    /// </summary>
    public static bool IsActive => singleton != null;

    #endregion


    #region Updating

    /// <summary>
    ///     Update the session for this frame.
    /// </summary>
    /// <remarks>This should only be called if there are no menus in use.</remarks>
    public static void Update(GameTime gameTime)
    {
        // check the singleton
        if (singleton == null)
        {
            return;
        }

        if (CombatEngine.IsActive)
        {
            CombatEngine.Update(gameTime);
        }
        else
        {
            singleton.UpdateQuest();
            TileEngine.Update(gameTime);
        }
    }

    #endregion


    #region Starting a New Session

    /// <summary>
    ///     Start a new session based on the data provided.
    /// </summary>
    public static void StartNewSession(GameStartDescription gameStartDescription,
        ScreenManager screenManager,
        GameplayScreen gameplayScreen)
    {
        // check the parameters
        if (gameStartDescription == null)
        {
            throw new ArgumentNullException("gameStartDescripton");
        }

        if (screenManager == null)
        {
            throw new ArgumentNullException("screenManager");
        }

        if (gameplayScreen == null)
        {
            throw new ArgumentNullException("gameplayScreen");
        }

        // end any existing session
        EndSession();

        // create a new singleton
        singleton = new Session(screenManager, gameplayScreen);

        // set up the initial map
        ChangeMap(gameStartDescription.MapContentName, null);

        // set up the initial party
        var content = singleton.screenManager.Game.Content;
        singleton.party = new Party(gameStartDescription, content);

        // load the quest line
        singleton.questLine = content.Load<QuestLine>(
            Path.Combine(@"Quests\QuestLines",
                gameStartDescription.QuestLineContentName)).Clone() as QuestLine;
    }

    #endregion


    #region Ending a Session

    /// <summary>
    ///     End the current session.
    /// </summary>
    public static void EndSession()
    {
        // exit the gameplay screen
        // -- store the gameplay session, for re-entrance
        if (singleton != null)
        {
            var gameplayScreen = singleton.gameplayScreen;
            singleton.gameplayScreen = null;

            // pop the music
            AudioManager.PopMusic();

            // clear the singleton
            singleton = null;

            if (gameplayScreen != null)
            {
                gameplayScreen.ExitScreen();
            }
        }
    }

    #endregion


    #region Party

    /// <summary>
    ///     The party that is playing the game right now.
    /// </summary>
    private Party party;

    /// <summary>
    ///     The party that is playing the game right now.
    /// </summary>
    public static Party Party => singleton == null ? null : singleton.party;

    #endregion


    #region Map

    /// <summary>
    ///     Change the current map, arriving at the given portal if any.
    /// </summary>
    /// <param name="contentName">The asset name of the new map.</param>
    /// <param name="originalPortal">The portal from the previous map.</param>
    public static void ChangeMap(string contentName, Portal originalPortal)
    {
        // make sure the content name is valid
        var mapContentName = contentName;
        if (!mapContentName.StartsWith(@"Maps\"))
        {
            mapContentName = Path.Combine(@"Maps", mapContentName);
        }

        // check for trivial movement - typically intra-map portals
        if (TileEngine.Map != null && TileEngine.Map.AssetName == mapContentName)
        {
            TileEngine.SetMap(TileEngine.Map, originalPortal == null ? null : TileEngine.Map.FindPortal(originalPortal.DestinationMapPortalName));
        }

        // load the map
        var content = singleton.screenManager.Game.Content;
        var map = content.Load<Map>(mapContentName).Clone() as Map;

        // modify the map based on the world changes (removed chests, etc.).
        singleton.ModifyMap(map);

        // start playing the music for the new map
        AudioManager.PlayMusic(map.MusicCueName);

        // set the new map into the tile engine
        TileEngine.SetMap(map, originalPortal == null ? null : map.FindPortal(originalPortal.DestinationMapPortalName));
    }


    /// <summary>
    ///     Perform any actions associated withe the given tile.
    /// </summary>
    /// <param name="mapPosition">The tile to check.</param>
    /// <returns>True if anything was encountered, false otherwise.</returns>
    public static bool EncounterTile(Point mapPosition)
    {
        // look for fixed-combats from the quest
        if (singleton.quest != null &&
            (singleton.quest.Stage == Quest.QuestStage.InProgress ||
             singleton.quest.Stage == Quest.QuestStage.RequirementsMet))
        {
            MapEntry<FixedCombat> fixedCombatEntry =
                singleton.quest.FixedCombatEntries.Find(
                    delegate(WorldEntry<FixedCombat> worldEntry)
                    {
                        return
                            TileEngine.Map.AssetName.EndsWith(
                                worldEntry.MapContentName) &&
                            worldEntry.MapPosition == mapPosition;
                    });
            if (fixedCombatEntry != null)
            {
                EncounterFixedCombat(fixedCombatEntry);
                return true;
            }
        }

        // look for fixed-combats from the map
        var fixedCombatMapEntry =
            TileEngine.Map.FixedCombatEntries.Find(
                delegate(MapEntry<FixedCombat> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (fixedCombatMapEntry != null)
        {
            EncounterFixedCombat(fixedCombatMapEntry);
            return true;
        }

        // look for chests from the current quest
        if (singleton.quest != null)
        {
            MapEntry<Chest> chestEntry = singleton.quest.ChestEntries.Find(
                delegate(WorldEntry<Chest> worldEntry)
                {
                    return
                        TileEngine.Map.AssetName.EndsWith(
                            worldEntry.MapContentName) &&
                        worldEntry.MapPosition == mapPosition;
                });
            if (chestEntry != null)
            {
                EncounterChest(chestEntry);
                return true;
            }
        }

        // look for chests from the map
        var chestMapEntry =
            TileEngine.Map.ChestEntries.Find(delegate(MapEntry<Chest> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (chestMapEntry != null)
        {
            EncounterChest(chestMapEntry);
            return true;
        }

        // look for player NPCs from the map
        var playerNpcEntry =
            TileEngine.Map.PlayerNpcEntries.Find(delegate(MapEntry<Player> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (playerNpcEntry != null)
        {
            EncounterPlayerNpc(playerNpcEntry);
            return true;
        }

        // look for quest NPCs from the map
        var questNpcEntry =
            TileEngine.Map.QuestNpcEntries.Find(delegate(MapEntry<QuestNpc> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (questNpcEntry != null)
        {
            EncounterQuestNpc(questNpcEntry);
            return true;
        }

        // look for portals from the map
        var portalEntry =
            TileEngine.Map.PortalEntries.Find(delegate(MapEntry<Portal> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (portalEntry != null)
        {
            EncounterPortal(portalEntry);
            return true;
        }

        // look for inns from the map
        var innEntry =
            TileEngine.Map.InnEntries.Find(delegate(MapEntry<Inn> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (innEntry != null)
        {
            EncounterInn(innEntry);
            return true;
        }

        // look for stores from the map
        var storeEntry =
            TileEngine.Map.StoreEntries.Find(delegate(MapEntry<Store> mapEntry) { return mapEntry.MapPosition == mapPosition; });
        if (storeEntry != null)
        {
            EncounterStore(storeEntry);
            return true;
        }

        // nothing encountered
        return false;
    }


    /// <summary>
    ///     Performs the actions associated with encountering a FixedCombat entry.
    /// </summary>
    public static void EncounterFixedCombat(MapEntry<FixedCombat> fixedCombatEntry)
    {
        // check the parameter
        if (fixedCombatEntry == null || fixedCombatEntry.Content == null)
        {
            throw new ArgumentNullException("fixedCombatEntry");
        }

        if (!CombatEngine.IsActive)
        {
            // start combat
            CombatEngine.StartNewCombat(fixedCombatEntry);
        }
    }


    /// <summary>
    ///     Performs the actions associated with encountering a Chest entry.
    /// </summary>
    public static void EncounterChest(MapEntry<Chest> chestEntry)
    {
        // check the parameter
        if (chestEntry == null || chestEntry.Content == null)
        {
            throw new ArgumentNullException("chestEntry");
        }

        // add the chest screen
        singleton.screenManager.AddScreen(new ChestScreen(chestEntry));
    }


    /// <summary>
    ///     Performs the actions associated with encountering a player-NPC entry.
    /// </summary>
    public static void EncounterPlayerNpc(MapEntry<Player> playerEntry)
    {
        // check the parameter
        if (playerEntry == null || playerEntry.Content == null)
        {
            throw new ArgumentNullException("playerEntry");
        }

        // add the player-NPC screen
        singleton.screenManager.AddScreen(new PlayerNpcScreen(playerEntry));
    }


    /// <summary>
    ///     Performs the actions associated with encountering a QuestNpc entry.
    /// </summary>
    public static void EncounterQuestNpc(MapEntry<QuestNpc> questNpcEntry)
    {
        // check the parameter
        if (questNpcEntry == null || questNpcEntry.Content == null)
        {
            throw new ArgumentNullException("questNpcEntry");
        }

        // add the quest-NPC screen
        singleton.screenManager.AddScreen(new QuestNpcScreen(questNpcEntry));
    }


    /// <summary>
    ///     Performs the actions associated with encountering an Inn entry.
    /// </summary>
    public static void EncounterInn(MapEntry<Inn> innEntry)
    {
        // check the parameter
        if (innEntry == null || innEntry.Content == null)
        {
            throw new ArgumentNullException("innEntry");
        }

        // add the inn screen
        singleton.screenManager.AddScreen(new InnScreen(innEntry.Content));
    }


    /// <summary>
    ///     Performs the actions associated with encountering a Store entry.
    /// </summary>
    public static void EncounterStore(MapEntry<Store> storeEntry)
    {
        // check the parameter
        if (storeEntry == null || storeEntry.Content == null)
        {
            throw new ArgumentNullException("storeEntry");
        }

        // add the store screen
        singleton.screenManager.AddScreen(new StoreScreen(storeEntry.Content));
    }


    /// <summary>
    ///     Performs the actions associated with encountering a Portal entry.
    /// </summary>
    public static void EncounterPortal(MapEntry<Portal> portalEntry)
    {
        // check the parameter
        if (portalEntry == null || portalEntry.Content == null)
        {
            throw new ArgumentNullException("portalEntry");
        }

        // change to the new map
        ChangeMap(portalEntry.Content.DestinationMapContentName,
            portalEntry.Content);
    }


    /// <summary>
    ///     Check if a random combat should start.  If so, start combat immediately.
    /// </summary>
    /// <returns>True if combat was started, false otherwise.</returns>
    public static bool CheckForRandomCombat(RandomCombat randomCombat)
    {
        // check the parameter
        if (randomCombat == null || randomCombat.CombatProbability <= 0)
        {
            return false;
        }

        // check to see if combat has already started
        if (CombatEngine.IsActive)
        {
            return false;
        }

        // check to see if the random combat starts
        var randomCombatCheck = random.Next(100);
        if (randomCombatCheck < randomCombat.CombatProbability)
        {
            // start combat immediately
            CombatEngine.StartNewCombat(randomCombat);
            return true;
        }

        // combat not started
        return false;
    }

    #endregion


    #region Quests

    /// <summary>
    ///     The main quest line for this session.
    /// </summary>
    private QuestLine questLine;

    /// <summary>
    ///     The main quest line for this session.
    /// </summary>
    public static QuestLine QuestLine => singleton == null ? null : singleton.questLine;


    /// <summary>
    ///     If true, the main quest line for this session is complete.
    /// </summary>
    public static bool IsQuestLineComplete
    {
        get
        {
            if (singleton == null || singleton.questLine == null ||
                singleton.questLine.QuestContentNames == null)
            {
                return false;
            }

            return singleton.currentQuestIndex >=
                   singleton.questLine.QuestContentNames.Count;
        }
    }


    /// <summary>
    ///     The current quest in this session.
    /// </summary>
    private Quest quest;

    /// <summary>
    ///     The current quest in this session.
    /// </summary>
    public static Quest Quest => singleton == null ? null : singleton.quest;


    /// <summary>
    ///     The index of the current quest into the quest line.
    /// </summary>
    private int currentQuestIndex;

    /// <summary>
    ///     The index of the current quest into the quest line.
    /// </summary>
    public static int CurrentQuestIndex => singleton == null ? -1 : singleton.currentQuestIndex;


    /// <summary>
    ///     Update the current quest and quest line for this session.
    /// </summary>
    public void UpdateQuest()
    {
        // check the singleton's state to see if we should care about quests
        if (party == null || questLine == null)
        {
            return;
        }

        // if we don't have a quest, then take the next one from teh list
        if (quest == null && questLine.Quests.Count > 0 &&
            !IsQuestLineComplete)
        {
            quest = questLine.Quests[currentQuestIndex];
            quest.Stage = Quest.QuestStage.NotStarted;
            // clear the monster-kill record
            party.MonsterKills.Clear();
            // clear the modified-quest lists
            modifiedQuestChests.Clear();
            removedQuestChests.Clear();
            removedQuestFixedCombats.Clear();
        }

        // handle quest-stage transitions
        if (quest != null && !IsQuestLineComplete)
        {
            switch (quest.Stage)
            {
                case Quest.QuestStage.NotStarted:
                    // start the new quest
                    quest.Stage = Quest.QuestStage.InProgress;
                    if (!quest.AreRequirementsMet)
                    {
                        // show the announcement of the quest and the requirements
                        ScreenManager.AddScreen(new QuestLogScreen(quest));
                    }

                    break;

                case Quest.QuestStage.InProgress:
                    // update monster requirements
                    foreach (var monsterRequirement in
                             quest.MonsterRequirements)
                    {
                        monsterRequirement.CompletedCount = 0;
                        var monster = monsterRequirement.Content;
                        if (party.MonsterKills.ContainsKey(monster.AssetName))
                        {
                            monsterRequirement.CompletedCount =
                                party.MonsterKills[monster.AssetName];
                        }
                    }

                    // update gear requirements
                    foreach (var gearRequirement in
                             quest.GearRequirements)
                    {
                        gearRequirement.CompletedCount = 0;
                        foreach (var entry in party.Inventory)
                        {
                            if (entry.Content == gearRequirement.Content)
                            {
                                gearRequirement.CompletedCount += entry.Count;
                            }
                        }
                    }

                    // check to see if the requirements have been met
                    if (quest.AreRequirementsMet)
                    {
                        // immediately remove the gear
                        foreach (var gearRequirement in
                                 quest.GearRequirements)
                        {
                            var gear = gearRequirement.Content;
                            party.RemoveFromInventory(gear,
                                gearRequirement.Count);
                        }

                        // check to see if there is a destination
                        if (string.IsNullOrEmpty(
                                quest.DestinationMapContentName))
                        {
                            // complete the quest
                            quest.Stage = Quest.QuestStage.Completed;
                            // show the completion dialogue
                            if (!string.IsNullOrEmpty(quest.CompletionMessage))
                            {
                                var dialogueScreen = new DialogueScreen();
                                dialogueScreen.TitleText = "Quest Complete";
                                dialogueScreen.BackText = string.Empty;
                                dialogueScreen.DialogueText =
                                    quest.CompletionMessage;
                                ScreenManager.AddScreen(dialogueScreen);
                            }
                        }
                        else
                        {
                            quest.Stage = Quest.QuestStage.RequirementsMet;
                            // remind the player about the destination
                            screenManager.AddScreen(new QuestLogScreen(quest));
                        }
                    }

                    break;

                case Quest.QuestStage.RequirementsMet:
                    break;

                case Quest.QuestStage.Completed:
                    // show the quest rewards screen
                    var rewardsScreen =
                        new RewardsScreen(RewardsScreen.RewardScreenMode.Quest,
                            Quest.ExperienceReward,
                            Quest.GoldReward,
                            Quest.GearRewards);
                    screenManager.AddScreen(rewardsScreen);
                    // advance to the next quest
                    currentQuestIndex++;
                    quest = null;
                    break;
            }
        }
    }

    #endregion


    #region Modified/Removed Content

    /// <summary>
    ///     The chests removed from the map asset by player actions.
    /// </summary>
    private List<WorldEntry<Chest>> removedMapChests = new();

    /// <summary>
    ///     The chests removed from the current quest asset by player actions.
    /// </summary>
    private List<WorldEntry<Chest>> removedQuestChests = new();

    /// <summary>
    ///     Remove the given chest entry from the current map or quest.
    /// </summary>
    public static void RemoveChest(MapEntry<Chest> mapEntry)
    {
        // check the parameter
        if (mapEntry == null)
        {
            return;
        }

        // check the map for the item first
        if (TileEngine.Map != null)
        {
            var removedEntries = TileEngine.Map.ChestEntries.RemoveAll(
                delegate(MapEntry<Chest> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition;
                });
            if (removedEntries > 0)
            {
                var worldEntry = new WorldEntry<Chest>();
                worldEntry.Content = mapEntry.Content;
                worldEntry.ContentName = mapEntry.ContentName;
                worldEntry.Count = mapEntry.Count;
                worldEntry.Direction = mapEntry.Direction;
                worldEntry.MapContentName = TileEngine.Map.AssetName;
                worldEntry.MapPosition = mapEntry.MapPosition;
                singleton.removedMapChests.Add(worldEntry);
                return;
            }
        }

        // look for the map entry within the quest
        if (singleton.quest != null)
        {
            var removedEntries = singleton.quest.ChestEntries.RemoveAll(
                delegate(WorldEntry<Chest> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition &&
                           TileEngine.Map.AssetName.EndsWith(entry.MapContentName);
                });
            if (removedEntries > 0)
            {
                var worldEntry = new WorldEntry<Chest>();
                worldEntry.Content = mapEntry.Content;
                worldEntry.ContentName = mapEntry.ContentName;
                worldEntry.Count = mapEntry.Count;
                worldEntry.Direction = mapEntry.Direction;
                worldEntry.MapContentName = TileEngine.Map.AssetName;
                worldEntry.MapPosition = mapEntry.MapPosition;
                singleton.removedQuestChests.Add(worldEntry);
            }
        }
    }


    /// <summary>
    ///     The fixed-combats removed from the map asset by player actions.
    /// </summary>
    private List<WorldEntry<FixedCombat>> removedMapFixedCombats = new();

    /// <summary>
    ///     The fixed-combats removed from the current quest asset by player actions.
    /// </summary>
    private List<WorldEntry<FixedCombat>> removedQuestFixedCombats = new();

    /// <summary>
    ///     Remove the given fixed-combat entry from the current map or quest.
    /// </summary>
    public static void RemoveFixedCombat(MapEntry<FixedCombat> mapEntry)
    {
        // check the parameter
        if (mapEntry == null)
        {
            return;
        }

        // check the map for the item first
        if (TileEngine.Map != null)
        {
            var removedEntries = TileEngine.Map.FixedCombatEntries.RemoveAll(
                delegate(MapEntry<FixedCombat> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition;
                });
            if (removedEntries > 0)
            {
                var worldEntry = new WorldEntry<FixedCombat>();
                worldEntry.Content = mapEntry.Content;
                worldEntry.ContentName = mapEntry.ContentName;
                worldEntry.Count = mapEntry.Count;
                worldEntry.Direction = mapEntry.Direction;
                worldEntry.MapContentName = TileEngine.Map.AssetName;
                worldEntry.MapPosition = mapEntry.MapPosition;
                singleton.removedMapFixedCombats.Add(worldEntry);
                return;
            }
        }

        // look for the map entry within the quest
        if (singleton.quest != null)
        {
            var removedEntries = singleton.quest.FixedCombatEntries.RemoveAll(
                delegate(WorldEntry<FixedCombat> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition &&
                           TileEngine.Map.AssetName.EndsWith(entry.MapContentName);
                });
            if (removedEntries > 0)
            {
                var worldEntry = new WorldEntry<FixedCombat>();
                worldEntry.Content = mapEntry.Content;
                worldEntry.ContentName = mapEntry.ContentName;
                worldEntry.Count = mapEntry.Count;
                worldEntry.Direction = mapEntry.Direction;
                worldEntry.MapContentName = TileEngine.Map.AssetName;
                worldEntry.MapPosition = mapEntry.MapPosition;
                singleton.removedQuestFixedCombats.Add(worldEntry);
            }
        }
    }


    /// <summary>
    ///     The player NPCs removed from the map asset by player actions.
    /// </summary>
    private List<WorldEntry<Player>> removedMapPlayerNpcs = new();

    /// <summary>
    ///     Remove the given player NPC entry from the current map or quest.
    /// </summary>
    public static void RemovePlayerNpc(MapEntry<Player> mapEntry)
    {
        // check the parameter
        if (mapEntry == null)
        {
            return;
        }

        // check the map for the item
        if (TileEngine.Map != null)
        {
            var removedEntries = TileEngine.Map.PlayerNpcEntries.RemoveAll(
                delegate(MapEntry<Player> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition;
                });
            if (removedEntries > 0)
            {
                var worldEntry = new WorldEntry<Player>();
                worldEntry.Content = mapEntry.Content;
                worldEntry.ContentName = mapEntry.ContentName;
                worldEntry.Count = mapEntry.Count;
                worldEntry.Direction = mapEntry.Direction;
                worldEntry.MapContentName = TileEngine.Map.AssetName;
                worldEntry.MapPosition = mapEntry.MapPosition;
                singleton.removedMapPlayerNpcs.Add(worldEntry);
            }
        }

        // quests don't have a list of player NPCs
    }


    /// <summary>
    ///     The chests that have been modified, but not emptied, by player action.
    /// </summary>
    private List<ModifiedChestEntry> modifiedMapChests = new();


    /// <summary>
    ///     The chests belonging to the current quest that have been modified,
    ///     but not emptied, by player action.
    /// </summary>
    private List<ModifiedChestEntry> modifiedQuestChests = new();


    /// <summary>
    ///     Stores the entry for a chest on the current map or quest that has been
    ///     modified but not emptied.
    /// </summary>
    public static void StoreModifiedChest(MapEntry<Chest> mapEntry)
    {
        // check the parameter
        if (mapEntry == null || mapEntry.Content == null)
        {
            throw new ArgumentNullException("mapEntry");
        }

        Predicate<ModifiedChestEntry> checkModifiedChests =
            delegate(ModifiedChestEntry entry)
            {
                return
                    TileEngine.Map.AssetName.EndsWith(
                        entry.WorldEntry.MapContentName) &&
                    entry.WorldEntry.ContentName == mapEntry.ContentName &&
                    entry.WorldEntry.MapPosition == mapEntry.MapPosition;
            };

        // check the map for the item first
        if (TileEngine.Map != null && TileEngine.Map.ChestEntries.Exists(
                delegate(MapEntry<Chest> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition;
                }))
        {
            singleton.modifiedMapChests.RemoveAll(checkModifiedChests);
            var modifiedChestEntry = new ModifiedChestEntry();
            modifiedChestEntry.WorldEntry.Content = mapEntry.Content;
            modifiedChestEntry.WorldEntry.ContentName = mapEntry.ContentName;
            modifiedChestEntry.WorldEntry.Count = mapEntry.Count;
            modifiedChestEntry.WorldEntry.Direction = mapEntry.Direction;
            modifiedChestEntry.WorldEntry.MapContentName =
                TileEngine.Map.AssetName;
            modifiedChestEntry.WorldEntry.MapPosition = mapEntry.MapPosition;
            var chest = mapEntry.Content;
            modifiedChestEntry.ChestEntries.AddRange(chest.Entries);
            modifiedChestEntry.Gold = chest.Gold;
            singleton.modifiedMapChests.Add(modifiedChestEntry);
            return;
        }


        // look for the map entry within the quest
        if (singleton.quest != null && singleton.quest.ChestEntries.Exists(
                delegate(WorldEntry<Chest> entry)
                {
                    return entry.ContentName == mapEntry.ContentName &&
                           entry.MapPosition == mapEntry.MapPosition &&
                           TileEngine.Map.AssetName.EndsWith(entry.MapContentName);
                }))
        {
            singleton.modifiedQuestChests.RemoveAll(checkModifiedChests);
            var modifiedChestEntry = new ModifiedChestEntry();
            modifiedChestEntry.WorldEntry.Content = mapEntry.Content;
            modifiedChestEntry.WorldEntry.ContentName = mapEntry.ContentName;
            modifiedChestEntry.WorldEntry.Count = mapEntry.Count;
            modifiedChestEntry.WorldEntry.Direction = mapEntry.Direction;
            modifiedChestEntry.WorldEntry.MapContentName = TileEngine.Map.AssetName;
            modifiedChestEntry.WorldEntry.MapPosition = mapEntry.MapPosition;
            var chest = mapEntry.Content;
            modifiedChestEntry.ChestEntries.AddRange(chest.Entries);
            modifiedChestEntry.Gold = chest.Gold;
            singleton.modifiedQuestChests.Add(modifiedChestEntry);
        }
    }


    /// <summary>
    ///     Remove the specified content from the map, typically from an earlier session.
    /// </summary>
    private void ModifyMap(Map map)
    {
        // check the parameter
        if (map == null)
        {
            throw new ArgumentNullException("map");
        }

        // remove all chests that were emptied already
        if (removedMapChests != null && removedMapChests.Count > 0)
        {
            // check each removed-content entry
            map.ChestEntries.RemoveAll(delegate(MapEntry<Chest> mapEntry)
            {
                return removedMapChests.Exists(
                    delegate(WorldEntry<Chest> removedEntry)
                    {
                        return
                            map.AssetName.EndsWith(removedEntry.MapContentName) &&
                            removedEntry.ContentName == mapEntry.ContentName &&
                            removedEntry.MapPosition == mapEntry.MapPosition;
                    });
            });
        }

        // remove all fixed-combats that were defeated already
        if (removedMapFixedCombats != null && removedMapFixedCombats.Count > 0)
        {
            // check each removed-content entry
            map.FixedCombatEntries.RemoveAll(delegate(MapEntry<FixedCombat> mapEntry)
            {
                return removedMapFixedCombats.Exists(
                    delegate(WorldEntry<FixedCombat> removedEntry)
                    {
                        return
                            map.AssetName.EndsWith(removedEntry.MapContentName) &&
                            removedEntry.ContentName == mapEntry.ContentName &&
                            removedEntry.MapPosition == mapEntry.MapPosition;
                    });
            });
        }

        // remove the player NPCs that have already joined the party
        if (removedMapPlayerNpcs != null && removedMapPlayerNpcs.Count > 0)
        {
            // check each removed-content entry
            map.PlayerNpcEntries.RemoveAll(delegate(MapEntry<Player> mapEntry)
            {
                return removedMapPlayerNpcs.Exists(
                    delegate(WorldEntry<Player> removedEntry)
                    {
                        return
                            map.AssetName.EndsWith(removedEntry.MapContentName) &&
                            removedEntry.ContentName == mapEntry.ContentName &&
                            removedEntry.MapPosition == mapEntry.MapPosition;
                    });
            });
        }

        // replace the chest entries of modified chests - they are already clones
        if (modifiedMapChests != null && modifiedMapChests.Count > 0)
        {
            foreach (var entry in map.ChestEntries)
            {
                var modifiedEntry = modifiedMapChests.Find(
                    delegate(ModifiedChestEntry modifiedTestEntry)
                    {
                        return
                            map.AssetName.EndsWith(
                                modifiedTestEntry.WorldEntry.MapContentName) &&
                            modifiedTestEntry.WorldEntry.ContentName ==
                            entry.ContentName &&
                            modifiedTestEntry.WorldEntry.MapPosition ==
                            entry.MapPosition;
                    });
                // if the chest has been modified, apply the changes
                if (modifiedEntry != null)
                {
                    ModifyChest(entry.Content, modifiedEntry);
                }
            }
        }
    }


    /// <summary>
    ///     Remove the specified content from the map, typically from an earlier session.
    /// </summary>
    private void ModifyQuest(Quest quest)
    {
        // check the parameter
        if (quest == null)
        {
            throw new ArgumentNullException("quest");
        }

        // remove all chests that were emptied arleady
        if (removedQuestChests != null && removedQuestChests.Count > 0)
        {
            // check each removed-content entry
            quest.ChestEntries.RemoveAll(
                delegate(WorldEntry<Chest> worldEntry)
                {
                    return removedQuestChests.Exists(
                        delegate(WorldEntry<Chest> removedEntry)
                        {
                            return
                                removedEntry.MapContentName.EndsWith(
                                    worldEntry.MapContentName) &&
                                removedEntry.ContentName ==
                                worldEntry.ContentName &&
                                removedEntry.MapPosition ==
                                worldEntry.MapPosition;
                        });
                });
        }

        // remove all of the fixed-combats that have already been defeated
        if (removedQuestFixedCombats != null &&
            removedQuestFixedCombats.Count > 0)
        {
            // check each removed-content entry
            quest.FixedCombatEntries.RemoveAll(
                delegate(WorldEntry<FixedCombat> worldEntry)
                {
                    return removedQuestFixedCombats.Exists(
                        delegate(WorldEntry<FixedCombat> removedEntry)
                        {
                            return
                                removedEntry.MapContentName.EndsWith(
                                    worldEntry.MapContentName) &&
                                removedEntry.ContentName ==
                                worldEntry.ContentName &&
                                removedEntry.MapPosition ==
                                worldEntry.MapPosition;
                        });
                });
        }

        // replace the chest entries of modified chests - they are already clones
        if (modifiedQuestChests != null && modifiedQuestChests.Count > 0)
        {
            foreach (var entry in quest.ChestEntries)
            {
                var modifiedEntry = modifiedQuestChests.Find(
                    delegate(ModifiedChestEntry modifiedTestEntry)
                    {
                        return
                            modifiedTestEntry.WorldEntry.MapContentName ==
                            entry.MapContentName &&
                            modifiedTestEntry.WorldEntry.ContentName ==
                            entry.ContentName &&
                            modifiedTestEntry.WorldEntry.MapPosition ==
                            entry.MapPosition;
                    });
                // if the chest has been modified, apply the changes
                if (modifiedEntry != null)
                {
                    ModifyChest(entry.Content, modifiedEntry);
                }
            }
        }
    }


    /// <summary>
    ///     Modify a Chest object based on the data in a ModifiedChestEntry object.
    /// </summary>
    private void ModifyChest(Chest chest, ModifiedChestEntry modifiedChestEntry)
    {
        // check the parameters
        if (chest == null || modifiedChestEntry == null)
        {
            return;
        }

        // set the new gold amount
        chest.Gold = modifiedChestEntry.Gold;

        // remove all contents not found in the modified version
        chest.Entries.RemoveAll(delegate(ContentEntry<Gear> contentEntry)
        {
            return !modifiedChestEntry.ChestEntries.Exists(
                delegate(ContentEntry<Gear> modifiedTestEntry)
                {
                    return contentEntry.ContentName ==
                           modifiedTestEntry.ContentName;
                });
        });

        // set the new counts on the remaining content items
        foreach (var contentEntry in chest.Entries)
        {
            var modifiedGearEntry =
                modifiedChestEntry.ChestEntries.Find(
                    delegate(ContentEntry<Gear> modifiedTestEntry)
                    {
                        return contentEntry.ContentName ==
                               modifiedTestEntry.ContentName;
                    });
            if (modifiedGearEntry != null)
            {
                contentEntry.Count = modifiedGearEntry.Count;
            }
        }
    }

    #endregion


    #region User Interface Data

    /// <summary>
    ///     The ScreenManager used to manage all UI in the game.
    /// </summary>
    private readonly ScreenManager screenManager;

    /// <summary>
    ///     The ScreenManager used to manage all UI in the game.
    /// </summary>
    public static ScreenManager ScreenManager => singleton == null ? null : singleton.screenManager;


    /// <summary>
    ///     The GameplayScreen object that created this session.
    /// </summary>
    private GameplayScreen gameplayScreen;


    /// <summary>
    ///     The heads-up-display menu shown on the map and combat screens.
    /// </summary>
    private readonly Hud hud;

    /// <summary>
    ///     The heads-up-display menu shown on the map and combat screens.
    /// </summary>
    public static Hud Hud => singleton == null ? null : singleton.hud;

    #endregion


    #region Drawing

    /// <summary>
    ///     Draws the session environment to the screen
    /// </summary>
    public static void Draw(GameTime gameTime)
    {
        var spriteBatch = singleton.screenManager.SpriteBatch;

        if (CombatEngine.IsActive)
        {
            // draw the combat background
            if (TileEngine.Map.CombatTexture != null)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(TileEngine.Map.CombatTexture,
                    Vector2.Zero,
                    Color.White);
                spriteBatch.End();
            }

            // draw the combat itself
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            CombatEngine.Draw(gameTime);
            spriteBatch.End();
        }
        else
        {
            singleton.DrawNonCombat(gameTime);
        }

        singleton.hud.Draw();
    }


    /// <summary>
    ///     Draws everything related to the non-combat part of the screen
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values</param>
    private void DrawNonCombat(GameTime gameTime)
    {
        var spriteBatch = screenManager.SpriteBatch;

        // draw the background
        spriteBatch.Begin();
        if (TileEngine.Map.Texture != null)
        {
            // draw the ground layer
            TileEngine.DrawLayers(spriteBatch, true, true, false);
            // draw the character shadows
            DrawShadows(spriteBatch);
        }

        spriteBatch.End();

        // Sort the object layer and player according to depth
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

        var elapsedSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;

        // draw the party leader
        {
            var player = party.Players[0];
            var position = TileEngine.PartyLeaderPosition.ScreenPosition;
            player.Direction = TileEngine.PartyLeaderPosition.Direction;
            player.ResetAnimation(TileEngine.PartyLeaderPosition.IsMoving);
            switch (player.State)
            {
                case Character.CharacterState.Idle:
                    if (player.MapSprite != null)
                    {
                        player.MapSprite.UpdateAnimation(elapsedSeconds);
                        player.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;

                case Character.CharacterState.Walking:
                    if (player.WalkingSprite != null)
                    {
                        player.WalkingSprite.UpdateAnimation(elapsedSeconds);
                        player.WalkingSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }
                    else if (player.MapSprite != null)
                    {
                        player.MapSprite.UpdateAnimation(elapsedSeconds);
                        player.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;
            }
        }

        // draw the player NPCs
        foreach (var playerEntry in TileEngine.Map.PlayerNpcEntries)
        {
            if (playerEntry.Content == null)
            {
                continue;
            }

            var position =
                TileEngine.GetScreenPosition(playerEntry.MapPosition);
            playerEntry.Content.ResetAnimation(false);
            switch (playerEntry.Content.State)
            {
                case Character.CharacterState.Idle:
                    if (playerEntry.Content.MapSprite != null)
                    {
                        playerEntry.Content.MapSprite.UpdateAnimation(
                            elapsedSeconds);
                        playerEntry.Content.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;

                case Character.CharacterState.Walking:
                    if (playerEntry.Content.WalkingSprite != null)
                    {
                        playerEntry.Content.WalkingSprite.UpdateAnimation(
                            elapsedSeconds);
                        playerEntry.Content.WalkingSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }
                    else if (playerEntry.Content.MapSprite != null)
                    {
                        playerEntry.Content.MapSprite.UpdateAnimation(
                            elapsedSeconds);
                        playerEntry.Content.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;
            }
        }

        // draw the quest NPCs
        foreach (var questNpcEntry in TileEngine.Map.QuestNpcEntries)
        {
            if (questNpcEntry.Content == null)
            {
                continue;
            }

            var position =
                TileEngine.GetScreenPosition(questNpcEntry.MapPosition);
            questNpcEntry.Content.ResetAnimation(false);
            switch (questNpcEntry.Content.State)
            {
                case Character.CharacterState.Idle:
                    if (questNpcEntry.Content.MapSprite != null)
                    {
                        questNpcEntry.Content.MapSprite.UpdateAnimation(
                            elapsedSeconds);
                        questNpcEntry.Content.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;

                case Character.CharacterState.Walking:
                    if (questNpcEntry.Content.WalkingSprite != null)
                    {
                        questNpcEntry.Content.WalkingSprite.UpdateAnimation(
                            elapsedSeconds);
                        questNpcEntry.Content.WalkingSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }
                    else if (questNpcEntry.Content.MapSprite != null)
                    {
                        questNpcEntry.Content.MapSprite.UpdateAnimation(
                            elapsedSeconds);
                        questNpcEntry.Content.MapSprite.Draw(spriteBatch,
                            position,
                            1f - position.Y / TileEngine.Viewport.Height);
                    }

                    break;
            }
        }

        // draw the fixed-combat monsters NPCs from the TileEngine.Map
        // -- since there may be many of the same FixedCombat object
        //    on the TileEngine.Map, but their animations are handled differently
        foreach (var fixedCombatEntry in
                 TileEngine.Map.FixedCombatEntries)
        {
            if (fixedCombatEntry.Content == null ||
                fixedCombatEntry.Content.Entries.Count <= 0)
            {
                continue;
            }

            var position =
                TileEngine.GetScreenPosition(fixedCombatEntry.MapPosition);
            fixedCombatEntry.MapSprite.UpdateAnimation(elapsedSeconds);
            fixedCombatEntry.MapSprite.Draw(spriteBatch,
                position,
                1f - position.Y / TileEngine.Viewport.Height);
        }

        // draw the fixed-combat monsters NPCs from the current quest
        // -- since there may be many of the same FixedCombat object
        //    on the TileEngine.Map, their animations are handled differently
        if (quest != null && (quest.Stage == Quest.QuestStage.InProgress ||
                              quest.Stage == Quest.QuestStage.RequirementsMet))
        {
            foreach (var fixedCombatEntry
                     in quest.FixedCombatEntries)
            {
                if (fixedCombatEntry.Content == null ||
                    fixedCombatEntry.Content.Entries.Count <= 0 ||
                    !TileEngine.Map.AssetName.EndsWith(
                        fixedCombatEntry.MapContentName))
                {
                    continue;
                }

                var position =
                    TileEngine.GetScreenPosition(fixedCombatEntry.MapPosition);
                fixedCombatEntry.MapSprite.UpdateAnimation(elapsedSeconds);
                fixedCombatEntry.MapSprite.Draw(spriteBatch,
                    position,
                    1f - position.Y / TileEngine.Viewport.Height);
            }
        }

        // draw the chests from the TileEngine.Map
        foreach (var chestEntry in TileEngine.Map.ChestEntries)
        {
            if (chestEntry.Content == null)
            {
                continue;
            }

            var position = TileEngine.GetScreenPosition(chestEntry.MapPosition);
            spriteBatch.Draw(chestEntry.Content.Texture,
                position,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                MathHelper.Clamp(1f - position.Y /
                    TileEngine.Viewport.Height,
                    0f,
                    1f));
        }

        // draw the chests from the current quest
        if (quest != null && (quest.Stage == Quest.QuestStage.InProgress ||
                              quest.Stage == Quest.QuestStage.RequirementsMet))
        {
            foreach (var chestEntry in quest.ChestEntries)
            {
                if (chestEntry.Content == null ||
                    !TileEngine.Map.AssetName.EndsWith(chestEntry.MapContentName))
                {
                    continue;
                }

                var position =
                    TileEngine.GetScreenPosition(chestEntry.MapPosition);
                spriteBatch.Draw(chestEntry.Content.Texture,
                    position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    MathHelper.Clamp(1f - position.Y /
                        TileEngine.Viewport.Height,
                        0f,
                        1f));
            }
        }

        spriteBatch.End();

        // draw the foreground
        spriteBatch.Begin();
        if (TileEngine.Map.Texture != null)
        {
            TileEngine.DrawLayers(spriteBatch, false, false, true);
        }

        spriteBatch.End();
    }


    /// <summary>
    ///     Draw the shadows that appear under all characters.
    /// </summary>
    private void DrawShadows(SpriteBatch spriteBatch)
    {
        // draw the shadow of the party leader
        var player = party.Players[0];
        if (player.ShadowTexture != null)
        {
            spriteBatch.Draw(player.ShadowTexture,
                TileEngine.PartyLeaderPosition.ScreenPosition,
                null,
                Color.White,
                0f,
                new Vector2(
                    (player.ShadowTexture.Width - TileEngine.Map.TileSize.X) / 2,
                    (player.ShadowTexture.Height - TileEngine.Map.TileSize.Y) / 2 -
                    player.ShadowTexture.Height / 6),
                1f,
                SpriteEffects.None,
                1f);
        }

        // draw the player NPCs' shadows
        foreach (var playerEntry in TileEngine.Map.PlayerNpcEntries)
        {
            if (playerEntry.Content == null)
            {
                continue;
            }

            if (playerEntry.Content.ShadowTexture != null)
            {
                var position =
                    TileEngine.GetScreenPosition(playerEntry.MapPosition);
                spriteBatch.Draw(playerEntry.Content.ShadowTexture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(
                        (playerEntry.Content.ShadowTexture.Width -
                         TileEngine.Map.TileSize.X) / 2,
                        (playerEntry.Content.ShadowTexture.Height -
                         TileEngine.Map.TileSize.Y) / 2 -
                        playerEntry.Content.ShadowTexture.Height / 6),
                    1f,
                    SpriteEffects.None,
                    1f);
            }
        }

        // draw the quest NPCs' shadows
        foreach (var questNpcEntry in TileEngine.Map.QuestNpcEntries)
        {
            if (questNpcEntry.Content == null)
            {
                continue;
            }

            if (questNpcEntry.Content.ShadowTexture != null)
            {
                var position =
                    TileEngine.GetScreenPosition(questNpcEntry.MapPosition);
                spriteBatch.Draw(questNpcEntry.Content.ShadowTexture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(
                        (questNpcEntry.Content.ShadowTexture.Width -
                         TileEngine.Map.TileSize.X) / 2,
                        (questNpcEntry.Content.ShadowTexture.Height -
                         TileEngine.Map.TileSize.Y) / 2 -
                        questNpcEntry.Content.ShadowTexture.Height / 6),
                    1f,
                    SpriteEffects.None,
                    1f);
            }
        }

        // draw the fixed-combat monsters NPCs' shadows
        foreach (var fixedCombatEntry in
                 TileEngine.Map.FixedCombatEntries)
        {
            if (fixedCombatEntry.Content == null ||
                fixedCombatEntry.Content.Entries.Count <= 0)
            {
                continue;
            }

            var monster = fixedCombatEntry.Content.Entries[0].Content;
            if (monster.ShadowTexture != null)
            {
                var position =
                    TileEngine.GetScreenPosition(fixedCombatEntry.MapPosition);
                spriteBatch.Draw(monster.ShadowTexture,
                    position,
                    null,
                    Color.White,
                    0f,
                    new Vector2(
                        (monster.ShadowTexture.Width - TileEngine.Map.TileSize.X) / 2,
                        (monster.ShadowTexture.Height - TileEngine.Map.TileSize.Y) / 2 -
                        monster.ShadowTexture.Height / 6),
                    1f,
                    SpriteEffects.None,
                    1f);
            }
        }
    }

    #endregion


    #region Loading a Session

    /// <summary>
    ///     Start a new session, using the data in the given save game.
    /// </summary>
    /// <param name="saveGameDescription">The description of the save game.</param>
    /// <param name="screenManager">The ScreenManager for the new session.</param>
    public static void LoadSession(SaveGameDescription saveGameDescription,
        ScreenManager screenManager,
        GameplayScreen gameplayScreen)
    {
        // check the parameters
        if (saveGameDescription == null)
        {
            throw new ArgumentNullException("saveGameDescription");
        }

        if (screenManager == null)
        {
            throw new ArgumentNullException("screenManager");
        }

        if (gameplayScreen == null)
        {
            throw new ArgumentNullException("gameplayScreen");
        }

        // end any existing session
        EndSession();

        // create the new session
        singleton = new Session(screenManager, gameplayScreen);

        // get the storage device and load the session
        GetStorageDevice(
            delegate(StorageDevice storageDevice) { LoadSessionResult(storageDevice, saveGameDescription); });
    }


    /// <summary>
    ///     Receives the storage device and starts a new session,
    ///     using the data in the given save game.
    /// </summary>
    /// <remarks>The new session is created in LoadSessionResult.</remarks>
    /// <param name="storageDevice">The chosen storage device.</param>
    /// <param name="saveGameDescription">The description of the save game.</param>
    public static void LoadSessionResult(StorageDevice storageDevice,
        SaveGameDescription saveGameDescription)
    {
        // check the parameters
        if (saveGameDescription == null)
        {
            throw new ArgumentNullException("saveGameDescription");
        }

        // check the parameter
        if (storageDevice == null || !storageDevice.IsConnected)
        {
            return;
        }

        // open the container
        using (var storageContainer = OpenContainer(storageDevice))
        {
            using (var stream =
                   storageContainer.OpenFile(saveGameDescription.FileName, FileMode.Open))
            {
                using (var xmlReader = XmlReader.Create(stream))
                {
                    // <rolePlayingGameData>
                    xmlReader.ReadStartElement("rolePlayingGameSaveData");

                    // read the map information
                    xmlReader.ReadStartElement("mapData");
                    var mapAssetName =
                        xmlReader.ReadElementString("mapContentName");
                    var playerPosition = new XmlSerializer(
                            typeof(PlayerPosition)).Deserialize(xmlReader)
                        as PlayerPosition;
                    singleton.removedMapChests = new XmlSerializer(
                            typeof(List<WorldEntry<Chest>>)).Deserialize(xmlReader)
                        as List<WorldEntry<Chest>>;
                    singleton.removedMapFixedCombats = new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Deserialize(xmlReader)
                        as List<WorldEntry<FixedCombat>>;
                    singleton.removedMapPlayerNpcs = new XmlSerializer(
                            typeof(List<WorldEntry<Player>>)).Deserialize(xmlReader)
                        as List<WorldEntry<Player>>;
                    singleton.modifiedMapChests = new XmlSerializer(
                            typeof(List<ModifiedChestEntry>)).Deserialize(xmlReader)
                        as List<ModifiedChestEntry>;
                    ChangeMap(mapAssetName, null);
                    TileEngine.PartyLeaderPosition = playerPosition;
                    xmlReader.ReadEndElement();

                    // read the quest information
                    var content = ScreenManager.Game.Content;
                    xmlReader.ReadStartElement("questData");
                    singleton.questLine = content.Load<QuestLine>(
                            xmlReader.ReadElementString("questLineContentName")).Clone()
                        as QuestLine;
                    singleton.currentQuestIndex = Convert.ToInt32(
                        xmlReader.ReadElementString("currentQuestIndex"));
                    for (var i = 0; i < singleton.currentQuestIndex; i++)
                    {
                        singleton.questLine.Quests[i].Stage =
                            Quest.QuestStage.Completed;
                    }

                    singleton.removedQuestChests = new XmlSerializer(
                            typeof(List<WorldEntry<Chest>>)).Deserialize(xmlReader)
                        as List<WorldEntry<Chest>>;
                    singleton.removedQuestFixedCombats = new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Deserialize(xmlReader)
                        as List<WorldEntry<FixedCombat>>;
                    singleton.modifiedQuestChests = new XmlSerializer(
                            typeof(List<ModifiedChestEntry>)).Deserialize(xmlReader)
                        as List<ModifiedChestEntry>;
                    var questStage = (Quest.QuestStage) Enum.Parse(
                        typeof(Quest.QuestStage),
                        xmlReader.ReadElementString("currentQuestStage"),
                        true);
                    if (singleton.questLine != null && !IsQuestLineComplete)
                    {
                        singleton.quest =
                            singleton.questLine.Quests[CurrentQuestIndex];
                        singleton.ModifyQuest(singleton.quest);
                        singleton.quest.Stage = questStage;
                    }

                    xmlReader.ReadEndElement();

                    // read the party data
                    singleton.party = new Party(new XmlSerializer(
                                typeof(PartySaveData)).Deserialize(xmlReader)
                            as PartySaveData,
                        content);

                    // </rolePlayingGameSaveData>
                    xmlReader.ReadEndElement();
                }
            }
        }
    }

    #endregion


    #region Saving the Session

    /// <summary>
    ///     Save the current state of the session.
    /// </summary>
    /// <param name="overwriteDescription">
    ///     The description of the save game to over-write, if any.
    /// </param>
    public static void SaveSession(SaveGameDescription overwriteDescription)
    {
        // retrieve the storage device, asynchronously
        GetStorageDevice(delegate(StorageDevice storageDevice) { SaveSessionResult(storageDevice, overwriteDescription); });
    }


    /// <summary>
    ///     Save the current state of the session, with the given storage device.
    /// </summary>
    /// <param name="storageDevice">The chosen storage device.</param>
    /// <param name="overwriteDescription">
    ///     The description of the save game to over-write, if any.
    /// </param>
    private static void SaveSessionResult(StorageDevice storageDevice,
        SaveGameDescription overwriteDescription)
    {
        // check the parameter
        if (storageDevice == null || !storageDevice.IsConnected)
        {
            return;
        }

        // open the container
        using (var storageContainer =
               OpenContainer(storageDevice))
        {
            string filename;
            string descriptionFilename;
            // get the filenames
            if (overwriteDescription == null)
            {
                var saveGameIndex = 0;
                string testFilename;
                do
                {
                    saveGameIndex++;
                    testFilename = "SaveGame" + saveGameIndex + ".xml";
                } while (storageContainer.FileExists(testFilename));

                filename = testFilename;
                descriptionFilename = "SaveGameDescription" +
                                      saveGameIndex + ".xml";
            }
            else
            {
                filename = overwriteDescription.FileName;
                descriptionFilename = "SaveGameDescription" +
                                      Path.GetFileNameWithoutExtension(
                                          overwriteDescription.FileName).Substring(8) + ".xml";
            }

            using (var stream = storageContainer.OpenFile(filename, FileMode.Create))
            {
                using (var xmlWriter = XmlWriter.Create(stream))
                {
                    // <rolePlayingGameData>
                    xmlWriter.WriteStartElement("rolePlayingGameSaveData");

                    // write the map information
                    xmlWriter.WriteStartElement("mapData");
                    xmlWriter.WriteElementString("mapContentName",
                        TileEngine.Map.AssetName);
                    new XmlSerializer(typeof(PlayerPosition)).Serialize(
                        xmlWriter,
                        TileEngine.PartyLeaderPosition);
                    new XmlSerializer(typeof(List<WorldEntry<Chest>>)).Serialize(
                        xmlWriter,
                        singleton.removedMapChests);
                    new XmlSerializer(
                        typeof(List<WorldEntry<FixedCombat>>)).Serialize(
                        xmlWriter,
                        singleton.removedMapFixedCombats);
                    new XmlSerializer(typeof(List<WorldEntry<Player>>)).Serialize(
                        xmlWriter,
                        singleton.removedMapPlayerNpcs);
                    new XmlSerializer(typeof(List<ModifiedChestEntry>)).Serialize(
                        xmlWriter,
                        singleton.modifiedMapChests);
                    xmlWriter.WriteEndElement();

                    // write the quest information
                    xmlWriter.WriteStartElement("questData");
                    xmlWriter.WriteElementString("questLineContentName",
                        singleton.questLine.AssetName);
                    xmlWriter.WriteElementString("currentQuestIndex",
                        singleton.currentQuestIndex.ToString());
                    new XmlSerializer(typeof(List<WorldEntry<Chest>>)).Serialize(
                        xmlWriter,
                        singleton.removedQuestChests);
                    new XmlSerializer(
                        typeof(List<WorldEntry<FixedCombat>>)).Serialize(
                        xmlWriter,
                        singleton.removedQuestFixedCombats);
                    new XmlSerializer(typeof(List<ModifiedChestEntry>)).Serialize(
                        xmlWriter,
                        singleton.modifiedQuestChests);
                    xmlWriter.WriteElementString("currentQuestStage",
                        IsQuestLineComplete ? Quest.QuestStage.NotStarted.ToString() : singleton.quest.Stage.ToString());
                    xmlWriter.WriteEndElement();

                    // write the party data
                    new XmlSerializer(typeof(PartySaveData)).Serialize(xmlWriter,
                        new PartySaveData(singleton.party));

                    // </rolePlayingGameSaveData>
                    xmlWriter.WriteEndElement();
                }
            }

            // create the save game description
            var description = new SaveGameDescription();
            description.FileName = Path.GetFileName(filename);
            description.ChapterName = IsQuestLineComplete ? "Quest Line Complete" : Quest.Name;
            description.Description = DateTime.Now.ToString();
            using (var stream =
                   storageContainer.OpenFile(descriptionFilename, FileMode.Create))
            {
                new XmlSerializer(typeof(SaveGameDescription)).Serialize(stream,
                    description);
            }
        }
    }

    #endregion


    #region Deleting a Save Game

    /// <summary>
    ///     Delete the save game specified by the description.
    /// </summary>
    /// <param name="saveGameDescription">The description of the save game.</param>
    public static void DeleteSaveGame(SaveGameDescription saveGameDescription)
    {
        // check the parameters
        if (saveGameDescription == null)
        {
            throw new ArgumentNullException("saveGameDescription");
        }

        // get the storage device and load the session
        GetStorageDevice(
            delegate(StorageDevice storageDevice) { DeleteSaveGameResult(storageDevice, saveGameDescription); });
    }


    /// <summary>
    ///     Delete the save game specified by the description.
    /// </summary>
    /// <param name="storageDevice">The chosen storage device.</param>
    /// <param name="saveGameDescription">The description of the save game.</param>
    public static void DeleteSaveGameResult(StorageDevice storageDevice,
        SaveGameDescription saveGameDescription)
    {
        // check the parameters
        if (saveGameDescription == null)
        {
            throw new ArgumentNullException("saveGameDescription");
        }

        // check the parameter
        if (storageDevice == null || !storageDevice.IsConnected)
        {
            return;
        }

        // open the container
        using (var storageContainer =
               OpenContainer(storageDevice))
        {
            storageContainer.DeleteFile(saveGameDescription.FileName);
            storageContainer.DeleteFile("SaveGameDescription" +
                                        Path.GetFileNameWithoutExtension(
                                            saveGameDescription.FileName).Substring(8) + ".xml");
        }

        // refresh the save game descriptions
        RefreshSaveGameDescriptions();
    }

    #endregion


    #region Save Game Descriptions

    /// <summary>
    ///     Save game descriptions for the current set of save games.
    /// </summary>
    private static List<SaveGameDescription> saveGameDescriptions;

    /// <summary>
    ///     Save game descriptions for the current set of save games.
    /// </summary>
    public static List<SaveGameDescription> SaveGameDescriptions => SaveGameDescriptions;


    /// <summary>
    ///     The maximum number of save-game descriptions that the list may hold.
    /// </summary>
    public const int MaximumSaveGameDescriptions = 5;


    /// <summary>
    ///     XML serializer for SaveGameDescription objects.
    /// </summary>
    private static readonly XmlSerializer saveGameDescriptionSerializer = new(typeof(SaveGameDescription));


    /// <summary>
    ///     Refresh the list of save-game descriptions.
    /// </summary>
    public static void RefreshSaveGameDescriptions()
    {
        // clear the list
        saveGameDescriptions = null;

        // retrieve the storage device, asynchronously
        GetStorageDevice(RefreshSaveGameDescriptionsResult);
    }


    /// <summary>
    ///     Asynchronous storage-device callback for
    ///     refreshing the save-game descriptions.
    /// </summary>
    private static void RefreshSaveGameDescriptionsResult(
        StorageDevice storageDevice)
    {
        // check the parameter
        if (storageDevice == null || !storageDevice.IsConnected)
        {
            return;
        }

        // open the container
        using (var storageContainer =
               OpenContainer(storageDevice))
        {
            saveGameDescriptions = new List<SaveGameDescription>();
            // get the description list
            var filenames =
                storageContainer.GetFileNames("SaveGameDescription*.xml");
            // add each entry to the list
            foreach (var filename in filenames)
            {
                SaveGameDescription saveGameDescription;

                // check the size of the list
                if (saveGameDescriptions.Count >= MaximumSaveGameDescriptions)
                {
                    break;
                }

                // open the file stream
                using (var fileStream = storageContainer.OpenFile(filename, FileMode.Open))
                {
                    // deserialize the object
                    saveGameDescription =
                        saveGameDescriptionSerializer.Deserialize(fileStream)
                            as SaveGameDescription;
                    // if it's valid, add it to the list
                    if (saveGameDescription != null)
                    {
                        saveGameDescriptions.Add(saveGameDescription);
                    }
                }
            }
        }
    }

    #endregion


    #region Storage

    /// <summary>
    ///     The stored StorageDevice object.
    /// </summary>
    private static StorageDevice storageDevice;

    /// <summary>
    ///     The container name used for save games.
    /// </summary>
    public static string SaveGameContainerName = "RolePlayingGame";


    /// <summary>
    ///     A delegate for receiving StorageDevice objects.
    /// </summary>
    public delegate void StorageDeviceDelegate(StorageDevice storageDevice);

    /// <summary>
    ///     Asynchronously retrieve a storage device.
    /// </summary>
    /// <param name="retrievalDelegate">
    ///     The delegate called when the device is available.
    /// </param>
    /// <remarks>
    ///     If there is a suitable cached storage device,
    ///     the delegate may be called directly by this function.
    /// </remarks>
    public static async void GetStorageDevice(StorageDeviceDelegate retrievalDelegate)
    {
        // check the parameter
        if (retrievalDelegate == null)
        {
            throw new ArgumentNullException("retrievalDelegate");
        }

        // check the stored storage device
        if (storageDevice != null && storageDevice.IsConnected)
        {
            retrievalDelegate(storageDevice);
            return;
        }

        // Reset the device
        storageDevice = null;
        StorageDevice.ShowSelector();
    }

    /// <summary>
    ///     Synchronously opens storage container
    /// </summary>
    private static StorageContainer OpenContainer(StorageDevice storageDevice)
    {
        return storageDevice.OpenAsync(SaveGameContainerName).Result;
    }

    #endregion


    #region Random

    /// <summary>
    ///     The random-number generator used with game events.
    /// </summary>
    private static readonly Random random = new();

    /// <summary>
    ///     The random-number generator used with game events.
    /// </summary>
    public static Random Random => Random;

    #endregion
}
