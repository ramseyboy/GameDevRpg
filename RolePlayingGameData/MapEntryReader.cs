using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

/// <summary>
/// Read a MapEntry object from the content pipeline.
/// </summary>
public class MapEntryReader<T> : ContentTypeReader<MapEntry<T>>, IContentTypeReaderDelegate<MapEntry<T>> where T : ContentObject
{
    private readonly IContentTypeReaderDelegate<ContentEntry<T>> reader = new ContentEntryReader<T>();

    /// <summary>
    /// Read a MapEntry object from the content pipeline.
    /// </summary>
    protected override MapEntry<T> Read(ContentReader input,
        MapEntry<T> existingInstance)
    {
        MapEntry<T> desc = existingInstance;
        if (desc == null)
        {
            desc = new MapEntry<T>();
        }

        reader.ReadContent(input, desc);
        desc.MapPosition = input.ReadObject<Point>();
        desc.Direction = (Direction)input.ReadInt32();

        return desc;
    }

    public void ReadContent(ContentReader input, MapEntry<T> existingInstance)
    {
        Read(input, existingInstance);
    }
}
