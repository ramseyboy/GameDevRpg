using Microsoft.Xna.Framework.Content;

namespace RolePlayingGameData;

public interface IContentTypeReaderDelegate<in TContent>
{
    public void ReadContent(ContentReader input, TContent existingInstance);
}
