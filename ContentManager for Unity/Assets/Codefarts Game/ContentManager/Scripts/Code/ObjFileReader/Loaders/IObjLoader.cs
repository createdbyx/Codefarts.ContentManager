using System.IO;

namespace ObjLoader.Loader.Loaders
{
    using System.Collections;

    public interface IObjLoader
    {
        LoadResult Load(Stream lineStream);
        IEnumerator LoadAsync(Stream lineStream);
    }
}