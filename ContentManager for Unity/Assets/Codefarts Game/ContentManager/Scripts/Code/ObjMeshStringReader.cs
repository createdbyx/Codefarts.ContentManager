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
    using System.Threading;

    using Codefarts.UnityThreading;

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

            var mesh = new Mesh();
            Vector3[] vertexes;
            Vector3[] normals;
            Vector2[] uvs;
            FaceGroup[] faces;
            this.BuildMesh(result, out vertexes, out normals, out uvs, out faces, null, null);
            mesh.vertices = vertexes;
            mesh.uv = uvs;
            mesh.normals = normals;

            for (var i = 0; i < faces.Length; i++)
            {
                mesh.SetTriangles(faces[0].indexes, i);
            }

            mesh.Optimize();
            return mesh;
        }

        private struct FaceGroup
        {
            public int[] indexes;
        }

        private void BuildMesh(LoadResult result, out Vector3[] vertexes, out Vector3[] normals, out Vector2[] uvs, out FaceGroup[] meshFaces, ReadAsyncArgs<string, object> args, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            //  var mesh = new Mesh();
            normals = new Vector3[result.Vertices.Count];
            vertexes = new Vector3[result.Vertices.Count];
            uvs = new Vector2[result.Vertices.Count];

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
                }
            }

            //mesh.vertices = vertexes;
            //if (result.Textures != null && result.Textures.Count > 0)
            //{
            //    mesh.uv = uvs;
            //}

            //if (result.Normals != null && result.Normals.Count > 0)
            //{
            //    mesh.normals = normals;
            //}

            meshFaces = new FaceGroup[result.Groups.Count];

            for (var subMeshIndex = 0; subMeshIndex < result.Groups.Count; subMeshIndex++)
            {
                var triangleGroup = result.Groups[subMeshIndex];

                var indexes = new int[0];
                for (var faceIndex = 0; faceIndex < triangleGroup.Faces.Count; faceIndex++)
                {
                    var face = triangleGroup.Faces[faceIndex];
                    var startIndex = indexes.Length;
                    Array.Resize(ref indexes, indexes.Length + face.Count);
                    for (var i = 0; i < face.Count; i++)
                    {
                        indexes[startIndex + i] = face[i].VertexIndex - 1;
                    }
                }

                meshFaces[subMeshIndex].indexes = indexes;
                //mesh.SetTriangles(indexes, subMeshIndex);

                if (completedCallback != null)
                {
                    args.Progress = 99f + (((float)subMeshIndex / result.Groups.Count) * 0.1f);
                    completedCallback(args);
                    //   yield return new WaitForEndOfFrame();
                }
            }

            //mesh.Optimize();
            //return mesh;
        }

        /*  private IEnumerator BuildMesh(Action<Mesh> setMeshCallback, LoadResult result, ReadAsyncArgs<string, object> args, Action<ReadAsyncArgs<string, object>> completedCallback)
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
                      //  yield return new WaitForEndOfFrame();
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

              for (var subMeshIndex = 0; subMeshIndex < result.Groups.Count; subMeshIndex++)
              {
                  var triangleGroup = result.Groups[subMeshIndex];

                  var indexes = new int[0];
                  for (var faceIndex = 0; faceIndex < triangleGroup.Faces.Count; faceIndex++)
                  {
                      var face = triangleGroup.Faces[faceIndex];
                      var startIndex = indexes.Length;
                      Array.Resize(ref indexes, indexes.Length + face.Count);
                      for (var i = 0; i < face.Count; i++)
                      {
                          indexes[startIndex + i] = face[i].VertexIndex - 1;
                      }
                  }

                  mesh.SetTriangles(indexes, subMeshIndex);

                  if (completedCallback != null)
                  {
                      args.Progress = 99f + (((float)subMeshIndex / result.Groups.Count) * 0.1f);
                      completedCallback(args);
                      //   yield return new WaitForEndOfFrame();
                  }
              }

              mesh.Optimize();
              if (setMeshCallback != null)
              {
                  setMeshCallback(mesh);
              }
          }                 */

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

            var isLoading = true;
            var progress = 0f;
            Vector3[] vertexes = null;
            Vector3[] normals = null;
            Vector2[] uvs = null;
            FaceGroup[] faces = null;
#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<string, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<string, object>();
#endif

            // create a main task to execute on a background thread
            var task = new Task() { Type = QueueType.BackgroundThread };

            task.action =
                t =>
                {
                    var objLoaderFactory = new ObjLoaderFactory();
                    var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());

                    var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
                    LoadResult result = null;
                    var fileStream = new FileStream(path, FileMode.Open);

                    Exception error = null;
                    var callback = new Action<float, Exception, LoadResult>(
                        (p, ex, value) =>
                        {
                            result = value;
                            progress = p * 0.66f;
                            error = ex;
                            isLoading = value == null && error == null;
                        });

                    objLoader.LoadAsync(fileStream, callback);

                    args.State = ReadState.Working;
                    args.Key = key;
                    while (isLoading)
                    {
                        args.Progress = progress;
                        args.Error = error;
                        if (error != null)
                        {
                            throw error;
                        }

                        // queue a task on the main thread (update method)
                        Threading.QueueTask(
                            tx =>
                            {
                                completedCallback(args);
                            });

                        Thread.Sleep(1);                                                              
                    }

                    // check if error occurred
                    this.BuildMesh(result, out vertexes, out normals, out uvs, out faces, args,
                      e =>
                      {
                          // queue a task on the main thread (update method)
                          Threading.QueueTask(
                              tx =>
                              {
                                  completedCallback(args);        
                              });
                      });
                };

            var buildMeshTask = new Task() { Type = QueueType.Update };
            buildMeshTask.action =
                t =>
                {
                    var mesh = new Mesh();
                    mesh.vertices = vertexes;
                    mesh.uv = uvs;
                    mesh.normals = normals;

                    for (var i = 0; i < faces.Length; i++)
                    {
                        mesh.SetTriangles(faces[0].indexes, i);
                    }

                    mesh.Optimize();
                    mesh.RecalculateBounds();

                    args.Progress = 100;
                    args.State = ReadState.Completed;
                    args.Result = mesh;

                    completedCallback(args);
                };

            // set the continuation task and execute the task
            task.continueWith = buildMeshTask;
            Threading.QueueTask(task);

            /*
            var isLoading = true;
            var progress = 0f;
            Loom.RunAsync(
                () =>
                {
                    var objLoaderFactory = new ObjLoaderFactory();
                    var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());

                    var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
                    LoadResult result = null;
                    var fileStream = new FileStream(path, FileMode.Open);

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
                        Loom.QueueOnMainThread(
                            () =>
                            {
                                completedCallback(args);
                            });
                    }


                    Vector3[] vertexes;
                    Vector3[] normals;
                    Vector2[] uvs;
                    FaceGroup[] faces;
                    this.BuildMesh(result, out vertexes, out normals, out uvs, out faces, args,
                        e =>
                        {
                            Loom.QueueOnMainThread(
                                () =>
                                {
                                    completedCallback(args);
                                });
                        });

                    Loom.QueueOnMainThread(
                        () =>
                        {
                            var mesh = new Mesh();
                            mesh.vertices = vertexes;
                            mesh.uv = uvs;
                            mesh.normals = normals;

                            for (var i = 0; i < faces.Length; i++)
                            {
                                mesh.SetTriangles(faces[0].indexes, i);
                            }

                            mesh.Optimize();

                            args.Progress = 100;
                            args.State = ReadState.Completed;
                            args.Result = mesh;

                            completedCallback(args);
                        });
                });
                     */


            //var scheduler = CoroutineManager.Instance;
            //scheduler.StartCoroutine(this.GetData(key, content, completedCallback));
        }

        /*   /// <summary>
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
           }                      */
    }
}