#region File Description

//-----------------------------------------------------------------------------
// ContentEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData;

/// <summary>
///     A description of a piece of content and quantity for various purposes.
/// </summary>
public class ContentEntry<T> where T : ContentObject
{
    /// <summary>
    ///     The content referred to by this entry.
    /// </summary>
    /// <remarks>
    ///     This will not be automatically loaded, as the content path may be incomplete.
    /// </remarks>
    private T content;

    /// <summary>
    ///     The content name for the content involved.
    /// </summary>
    private string contentName;


    /// <summary>
    ///     The quantity of this content.
    /// </summary>
    private int count = 1;

    /// <summary>
    ///     The content name for the content involved.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public string ContentName
    {
        get => contentName;
        set => contentName = value;
    }

    /// <summary>
    ///     The content referred to by this entry.
    /// </summary>
    /// <remarks>
    ///     This will not be automatically loaded, as the content path may be incomplete.
    /// </remarks>
    [ContentSerializerIgnore]
    [XmlIgnore]
    public T Content
    {
        get => content;
        set => content = value;
    }

    /// <summary>
    ///     The quantity of this content.
    /// </summary>
    [ContentSerializer(Optional = true)]
    public int Count
    {
        get => count;
        set => count = value;
    }
}
