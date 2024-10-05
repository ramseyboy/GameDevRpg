using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public class ContentEntryReader<T> : IContentTypeReaderDelegate<ContentEntry<T>>
    where T : ContentObject
{
    public void ReadContent(ContentReader input,
        ContentEntry<T> existingInstance)
    {
        var member = existingInstance;
        if (member == null)
        {
            member = new ContentEntry<T>();
        }

        member.ContentName = input.ReadString();
        member.Count = input.ReadInt32();
    }
}
