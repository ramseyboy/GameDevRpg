using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public interface IContentTypeReaderDelegate<TContent>
{
    public TContent Read(ContentReader input, TContent existingInstance);
}
