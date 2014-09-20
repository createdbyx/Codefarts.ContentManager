using System.IO;

namespace ObjLoader.Loader.Loaders
{
    using System;
    using System.Collections;

    public interface IObjLoader
    {
        LoadResult Load(Stream lineStream);
        void LoadAsync(Stream lineStream, Action<float, Exception, LoadResult> progress);
    }
}