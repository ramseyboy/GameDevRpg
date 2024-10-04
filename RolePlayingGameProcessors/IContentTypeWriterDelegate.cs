using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace RolePlayingGameProcessors;

public interface IContentTypeWriterDelegate<TContent>
{
    public void Write(ContentWriter output, TContent value);
}
