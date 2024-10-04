using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public class ContentEntryReader<T>: IContentTypeReaderDelegate<ContentEntry<T>>
    where T : ContentObject
{
    public ContentEntry<T> Read(ContentReader input,
        ContentEntry<T> existingInstance)
    {
        ContentEntry<T> member = existingInstance;
        if (member == null)
        {
            member = new ContentEntry<T>();
        }

        member.ContentName = input.ReadString();
        member.Count = input.ReadInt32();

        return member;
    }
}
