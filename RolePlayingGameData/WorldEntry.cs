#region File Description

//-----------------------------------------------------------------------------
// WorldEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using Microsoft.Xna.Framework.Content;

#endregion

namespace RolePlayingGameData;

/// <summary>
///     A description of a piece of content, including the name of the map it's on.
/// </summary>
public class WorldEntry<T> : MapEntry<T> where T : ContentObject
{
    /// <summary>
    ///     The name of the map where the content is added.
    /// </summary>
    private string mapContentName;

    /// <summary>
    ///     The name of the map where the content is added.
    /// </summary>
    public string MapContentName
    {
        get => mapContentName;
        set => mapContentName = value;
    }


    #region Content Type Reader

    /// <summary>
    ///     Reads a WorldEntry object from the content pipeline.
    /// </summary>
    public class WorldEntryReader : ContentTypeReader<WorldEntry<T>>
    {
        private readonly IContentTypeReaderDelegate<MapEntry<T>> mapEntryReader = new MapEntryReader<T>();

        /// <summary>
        ///     Reads a WorldEntry object from the content pipeline.
        /// </summary>
        protected override WorldEntry<T> Read(ContentReader input,
            WorldEntry<T> existingInstance)
        {
            var desc = existingInstance;
            if (desc == null)
            {
                desc = new WorldEntry<T>();
            }

            mapEntryReader.ReadContent(input, desc);
            desc.MapContentName = input.ReadString();

            return desc;
        }
    }

    #endregion
}
