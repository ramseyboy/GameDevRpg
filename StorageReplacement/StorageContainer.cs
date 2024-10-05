using Microsoft.Xna.Framework;

namespace StorageReplacement;

public class StorageContainer : IDisposable
{
    private readonly string _path;

    public StorageContainer(string path)
    {
        _path = path;
    }

    public void Dispose()
    {
    }

    public void DeleteFile(string fileName)
    {
        File.Delete(Path.Combine(_path, fileName));
    }

    public bool FileExists(string testFilename)
    {
        return File.Exists(Path.Combine(_path, testFilename));
    }

    public string[] GetFileNames(string pattern)
    {
        return Directory.GetFiles(_path, pattern);
    }

    public Stream OpenFile(string fileName, FileMode mode)
    {
        return File.Open(Path.Combine(_path, fileName), mode);
    }
}

public class StorageDevice
{
    public StorageDevice(string path)
    {
        Path = path;
    }

    public string Path { get; }
    public bool IsConnected => true;

    public Task<StorageContainer> OpenAsync(string saveGameContainerName)
    {
        return Task.FromResult(new StorageContainer(System.IO.Path.Combine(Path, saveGameContainerName)));
    }

    public static void ShowSelector()
    {
    }
}

public class GamerServicesComponent : GameComponent
{
    public GamerServicesComponent(Game game) : base(game)
    {
    }
}
