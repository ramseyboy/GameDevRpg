#region File Description

//-----------------------------------------------------------------------------
// QuestLogScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using RolePlayingGameData.Quests;

#endregion

namespace RolePlayingGame.GameScreens;

/// <summary>
///     Displays all of the quests completed by the party
/// </summary>
internal class QuestLogScreen : ListScreen<Quest>
{
    #region Initial Detail Quest

    /// <summary>
    ///     The quest that is shown when the screen is created.
    /// </summary>
    /// <remarks>
    ///     Stored because new screens can't be added until the first update.
    /// </remarks>
    private Quest initialDetailQuest;

    #endregion


    #region Initialization

    /// <summary>
    ///     Creates a new EquipmentScreen object for the given player.
    /// </summary>
    public QuestLogScreen(Quest initialDetailQuest)
    {
        // assign the parameter - null is permitted
        this.initialDetailQuest = initialDetailQuest;

        // configure the menu text
        titleText = Session.Session.QuestLine.Name;
        selectButtonText = "Select";
        backButtonText = "Back";
        xButtonText = string.Empty;
        yButtonText = string.Empty;
        leftTriggerText = "Equipment";
        rightTriggerText = "Statistics";

        // select the current quest
        SelectedIndex = Session.Session.CurrentQuestIndex;
    }

    #endregion


    #region Data Access

    /// <summary>
    ///     Get the list that this screen displays.
    /// </summary>
    public override ReadOnlyCollection<Quest> GetDataList()
    {
        var quests = new List<Quest>();
        for (var i = 0; i <= Session.Session.CurrentQuestIndex; i++)
        {
            if (i < Session.Session.QuestLine.Quests.Count)
            {
                quests.Add(Session.Session.QuestLine.Quests[i]);
            }
        }

        return quests.AsReadOnly();
    }

    #endregion


    #region Columns

    protected string nameColumnText = "Name";
    private const int nameColumnInterval = 20;

    protected string stageColumnText = "Stage";
    private const int stageColumnInterval = 450;

    #endregion


    #region Input Handling

    /// <summary>
    ///     Handle user input.
    /// </summary>
    public override void HandleInput()
    {
        // open the initial QuestDetailScreen, if any
        // -- this is the first opportunity to add another screen
        if (initialDetailQuest != null)
        {
            ScreenManager.AddScreen(new QuestDetailsScreen(initialDetailQuest));
            // if the selected quest is in the list, make sure it's visible
            SelectedIndex = Session.Session.QuestLine.Quests.IndexOf(initialDetailQuest);
            // only open the screen once
            initialDetailQuest = null;
        }

        base.HandleInput();
    }


    /// <summary>
    ///     Respond to the triggering of the X button (and related key).
    /// </summary>
    protected override void SelectTriggered(Quest entry)
    {
        ScreenManager.AddScreen(new QuestDetailsScreen(entry));
    }


    /// <summary>
    ///     Switch to the screen to the "left" of this one in the UI.
    /// </summary>
    protected override void PageScreenLeft()
    {
        ExitScreen();
        ScreenManager.AddScreen(new InventoryScreen(false));
    }


    /// <summary>
    ///     Switch to the screen to the "right" of this one in the UI.
    /// </summary>
    protected override void PageScreenRight()
    {
        ExitScreen();
        ScreenManager.AddScreen(new StatisticsScreen(Session.Session.Party.Players[0]));
    }

    #endregion


    #region Drawing

    /// <summary>
    ///     Draw the quest at the given position in the list.
    /// </summary>
    /// <param name="contentEntry">The quest to draw.</param>
    /// <param name="position">The position to draw the entry at.</param>
    /// <param name="isSelected">If true, this item is selected.</param>
    protected override void DrawEntry(Quest entry,
        Vector2 position,
        bool isSelected)
    {
        // check the parameter
        if (entry == null)
        {
            throw new ArgumentNullException("entry");
        }

        var spriteBatch = ScreenManager.SpriteBatch;
        var drawPosition = position;

        // draw the name
        var color = isSelected ? Fonts.HighlightColor : Fonts.DisplayColor;
        drawPosition.Y += listLineSpacing / 4;
        drawPosition.X += nameColumnInterval;
        spriteBatch.DrawString(Fonts.GearInfoFont, entry.Name, drawPosition, color);

        // draw the stage
        drawPosition.X += stageColumnInterval;
        var stageText = string.Empty;
        switch (entry.Stage)
        {
            case Quest.QuestStage.Completed:
                stageText = "Completed";
                break;

            case Quest.QuestStage.InProgress:
                stageText = "In Progress";
                break;

            case Quest.QuestStage.NotStarted:
                stageText = "Not Started";
                break;

            case Quest.QuestStage.RequirementsMet:
                stageText = "Requirements Met";
                break;
        }

        spriteBatch.DrawString(Fonts.GearInfoFont, stageText, drawPosition, color);

        // turn on or off the select button
        if (isSelected)
        {
            selectButtonText = "Select";
        }
    }


    /// <summary>
    ///     Draw the description of the selected item.
    /// </summary>
    protected override void DrawSelectedDescription(Quest entry)
    {
    }


    /// <summary>
    ///     Draw the column headers above the list.
    /// </summary>
    protected override void DrawColumnHeaders()
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var position = listEntryStartPosition;

        position.X += nameColumnInterval;
        if (!string.IsNullOrEmpty(nameColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                nameColumnText,
                position,
                Fonts.CaptionColor);
        }

        position.X += stageColumnInterval;
        if (!string.IsNullOrEmpty(stageColumnText))
        {
            spriteBatch.DrawString(Fonts.MainFont,
                stageColumnText,
                position,
                Fonts.CaptionColor);
        }
    }

    #endregion
}
