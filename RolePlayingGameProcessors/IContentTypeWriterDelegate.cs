using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace RolePlayingGameProcessors;

public interface IContentTypeWriterDelegate<TContent>
{
    public void WriteContent(ContentWriter output, TContent value);
}
