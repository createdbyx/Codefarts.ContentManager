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
    using System.IO;
    using System.Linq;

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
            var fileStream = new FileStream(path, FileMode.Open);
            var result = objLoader.Load(fileStream);

            var mesh = this.BuildMesh(result);
            return mesh;
        }

        private Mesh BuildMesh(LoadResult result)
        {
            var mesh = new Mesh();
              var normals = new Vector3[result.Vertices.Count];
            var vertexes = new Vector3[result.Vertices.Count];
            var uvs = new Vector2[result.Vertices.Count];

            foreach (var meshGroup in result.Groups)
            {
                foreach (var face in meshGroup.Faces)
                {
                    for (var faceIndex = 0; faceIndex < face.Count; faceIndex++)
                    {
                        var faceInfo = face[faceIndex];
                        var normal = result.Normals[faceInfo.NormalIndex - 1];
                        var vertexIndex = faceInfo.VertexIndex - 1;
                        var vertex = result.Vertices[vertexIndex];
                        var uv = result.Textures[vertexIndex];
                        normals[vertexIndex] = new Vector3(normal.X, normal.Y, normal.Z);
                        vertexes[vertexIndex] = new Vector3(vertex.X, vertex.Y, vertex.Z);
                        uvs[vertexIndex] = new Vector2(uv.X, uv.Y);
                    }
                }
            }

            mesh.vertices = vertexes;
            mesh.uv = uvs;
            mesh.normals = normals;

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
            }

            mesh.Optimize();
            return mesh;
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
            var fileStream = new FileStream(path, FileMode.Open);
            var result = objLoader.LoadAsync(fileStream);

#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<string, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<string, object>();
#endif

            if (result.Current is LoadResult)
            {
                args.Progress = 100;
                args.State = ReadState.Completed;
                args.Key = key;
                var mesh = this.BuildMesh((LoadResult)result.Current);
                args.Result = mesh;
                completedCallback(args);
                yield return null;
            }

            if (result.Current is ValueType && result.Current is float)
            {
                args.Progress = (float)result.Current * 100f;
                args.State = ReadState.Working;
                args.Key = key;
                args.Result = null;
                completedCallback(args);
            }

            yield return result.Current;
        }
    }
}