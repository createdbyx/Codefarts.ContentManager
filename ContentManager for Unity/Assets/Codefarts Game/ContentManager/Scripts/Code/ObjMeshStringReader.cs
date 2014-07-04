// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using ObjLoader.Loader.Loaders;

    using UnityEngine;
#if USEOBJECTPOOLING
    using Codefarts.ObjectPooling;
#endif

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class ObjMeshStringReader : IReader<string>
    {
        /// <summary>
        /// Gets the <see cref="IReader{T}.Type"/> that this reader implementation returns.
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(Mesh);
            }
        }

        /// <summary>
        /// Reads a file and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns a type representing the data.</returns>
        public object Read(string key, ContentManager<string> content)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());

            var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
            LoadResult result;
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                result = objLoader.Load(fileStream);
            }

            return this.BuildMesh(null, result, null, null);
        }

        private IEnumerator BuildMesh(Action<Mesh> setMeshCallback, LoadResult result, ReadAsyncArgs<string, object> args, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            var mesh = new Mesh();
            var normals = new Vector3[result.Vertices.Count];
            var vertexes = new Vector3[result.Vertices.Count];
            var uvs = new Vector2[result.Vertices.Count];


            for (var groupIndex = 0; groupIndex < result.Groups.Count; groupIndex++)
            {
                var meshGroup = result.Groups[groupIndex];
                for (var i = 0; i < meshGroup.Faces.Count; i++)
                {
                    var face = meshGroup.Faces[i];
                    for (var faceIndex = 0; faceIndex < face.Count; faceIndex++)
                    {
                        var faceInfo = face[faceIndex];
                        var vertexIndex = faceInfo.VertexIndex - 1;
                        var vertex = result.Vertices[vertexIndex];
                        var normal = result.Normals[faceInfo.NormalIndex - 1];
                        var uv = result.Textures[faceInfo.TextureIndex - 1];
                        vertexes[vertexIndex] = new Vector3(vertex.X, vertex.Y, vertex.Z);
                        normals[vertexIndex] = new Vector3(normal.X, normal.Y, normal.Z);
                        uvs[vertexIndex] = new Vector2(uv.X, uv.Y);
                    }
                }

                if (completedCallback != null)
                {
                    args.Progress = 66f + ((((float)groupIndex / result.Groups.Count) * 33f));
                    completedCallback(args);
                    yield return new WaitForEndOfFrame();
                }
            }

            mesh.vertices = vertexes;
            if (result.Textures != null && result.Textures.Count > 0)
            {
                mesh.uv = uvs;
            }

            if (result.Normals != null && result.Normals.Count > 0)
            {
                mesh.normals = normals;
            }

            for (var i = 0; i < result.Groups.Count; i++)
            {
                var triangleGroup = result.Groups[i];

                mesh.SetTriangles(
                    triangleGroup.Faces.SelectMany(
                        face =>
                        {
                            var indexes = new int[face.Count];
                            for (var j = 0; j < face.Count; j++)
                            {
                                indexes[j] = face[j].VertexIndex - 1;
                            }

                            return indexes;
                        }).ToArray(),
                    i);

                if (completedCallback != null)
                {
                    args.Progress = 99f + (((float)i / result.Groups.Count) * 0.1f);
                    completedCallback(args);
                    yield return new WaitForEndOfFrame();
                }
            }

            mesh.Optimize();
            setMeshCallback(mesh);
        }

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        public bool CanRead(string key, ContentManager<string> content)
        {
            var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
            if (!File.Exists(path))
            {
                return false;
            }

            var extension = Path.GetExtension(key);
            return extension.EndsWith(".obj");
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(string key, ContentManager<string> content, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            if (completedCallback == null)
            {
                throw new ArgumentNullException("completedCallback");
            }

            var scheduler = CoroutineManager.Instance;
            scheduler.StartCoroutine(this.GetData(key, content, completedCallback));
        }

        /// <summary>
        /// Gets the data from the 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        /// <param name="url">The url to be requested.</param>
        /// <returns>Returns a <see cref="IEnumerable"/> coroutine.</returns>
        private IEnumerator GetData(string key, ContentManager<string> content, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());

            var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
            LoadResult result = null;
            var isLoading = true;
            var fileStream = new FileStream(path, FileMode.Open);

            var progress = 0f;
            var callback = new Action<float, LoadResult>(
                (p, value) =>
                {
                    isLoading = value == null;
                    result = value;
                    progress = p * 0.66f;
                });

            objLoader.LoadAsync(fileStream, callback);

#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<string, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<string, object>();
#endif

            args.State = ReadState.Working;
            args.Key = key;
            while (isLoading)
            {
                args.Progress = progress;
                completedCallback(args);
                yield return new WaitForEndOfFrame();
            }

            Mesh mesh = null;
            var scheduler = CoroutineManager.Instance;
            scheduler.StartCoroutine(this.BuildMesh(m => { mesh = m; }, result, args, completedCallback));
            while (mesh == null)
            {
                yield return new WaitForEndOfFrame();
            }

            args.Progress = 100;
            args.State = ReadState.Completed;
            args.Result = mesh;
            completedCallback(args);
        }
    }
}