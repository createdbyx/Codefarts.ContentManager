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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using Codefarts.ContentManager.Scripts.Code;

    using ObjLoader.Loader.Loaders;

    using UnityEngine;

#if USEOBJECTPOOLING
    using Codefarts.ObjectPooling;
#endif

    using Object = UnityEngine.Object;

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class ObjMeshPreviewTextureStringReader : IReader<MeshPreviewTextureArgs>
    {
        private struct RootObject
        {
            public Transform transform;
            public bool activeState;
        }

        /// <summary>
        /// Gets the <see cref="IReader{T}.Type"/> that this reader implementation returns.
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(MeshPreviewTexture);
            }
        }

        /// <summary>
        /// Reads a file and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns a type representing the data.</returns>
        public object Read(MeshPreviewTextureArgs key, ContentManager<MeshPreviewTextureArgs> content)
        {
            var meshPreviewTexture = this.DoLoadCache(key);
            if (meshPreviewTexture != null)
            {
                return meshPreviewTexture;
            }

            // hide and disable all top level objects
            var roots = this.HideTopLevelObjects();

            GameObject cameraObject;
            Camera camera;
            var previewObject = this.DoSetupObjects(key, content, out cameraObject, out camera);

            // TODO: Hide mesh renderes before rendering to prevent any other scene data from being rendered along with preview

            // we need to manually render one frame with the camera here
            camera.Render();

            // generate a preview texture
            var texture = new Texture2D(key.Width, key.Height, TextureFormat.ARGB32, true);
            texture.ReadPixels(new Rect(0, 0, key.Width, key.Height), 0, 0);
            texture.Apply();
                                                                                        
            // destroy preview objects
            Object.DestroyImmediate(previewObject);
            Object.DestroyImmediate(cameraObject);

            this.DoRestoreScene(roots);

            this.DoSaveToCache(key, texture);

            return new MeshPreviewTexture() { Texture = texture };
        }

        private MeshPreviewTexture DoLoadCache(MeshPreviewTextureArgs key)
        {
            if (!key.LoadFromCache)
            {
                return null;
            }

            if (!Directory.Exists(key.CacheFolder))
            {
                return null;
            }

            var path = Path.Combine(key.CacheFolder, "MeshPreviewCache");
            Directory.CreateDirectory(path);

            var sha256 = MD5.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key.Key));
            var result = Convert.ToBase64String(bytes);

            path = Path.Combine(path, result + ".png");
            if (!File.Exists(path))
            {
                return null;
            }

            var texture = new Texture2D(4, 4, TextureFormat.ARGB32, true);
            if (texture.LoadImage(File.ReadAllBytes(path)))
            {
                return new MeshPreviewTexture() { Texture = texture };
            }

            Object.DestroyImmediate(texture);
            return null;
        }

        private void DoRestoreScene(List<RootObject> roots)
        {
            // restore the scene        
            foreach (var rootObject in roots)
            {
                rootObject.transform.gameObject.SetActive(rootObject.activeState);
            }
        }

        private GameObject DoSetupObjects(MeshPreviewTextureArgs key, ContentManager<MeshPreviewTextureArgs> content, out GameObject cameraObject, out Camera camera)
        {
            // load object
            var previewObject = new GameObject("PreviewMesh");
            this.LoadPreviewGameObjects(key.Key, content, previewObject);

            previewObject.AddComponent<MeshRenderer>();
            previewObject.transform.position = Vector3.zero;
            previewObject.renderer.material = key.Material == null ? new Material(Shader.Find("Diffuse")) : key.Material;

            // setup preview camera
            cameraObject = new GameObject("PreviewCamera");
            camera = cameraObject.AddComponent<Camera>();
            camera.backgroundColor = key.BackgroundColor;
            camera.clearFlags = CameraClearFlags.SolidColor;

            // calc size of view port
            camera.pixelRect = new Rect(0, 0, key.Width, key.Height);

            // calc max radius of the object render bounds
            var halfSize = previewObject.renderer.bounds.extents; // bounds.extents;
            var radius = halfSize.x > halfSize.y ? halfSize.x : halfSize.y;
            radius = radius > halfSize.z ? radius : halfSize.z;

            // calculate how far to position the camera away from the object
            var dist = radius / (float)Math.Sin(camera.fieldOfView * (Math.PI / 180) / 2);
            camera.transform.position = new Vector3(dist, dist, 0);
            camera.transform.LookAt(previewObject.transform.position);
            return previewObject;
        }

        private void DoSaveToCache(MeshPreviewTextureArgs key, Texture2D texture)
        {
            if (key.SaveToCache)
            {
                if (!Directory.Exists(key.CacheFolder))
                {
                    throw new DirectoryNotFoundException("Cache folder appears to me missing!");
                }

                var path = Path.Combine(key.CacheFolder, "MeshPreviewCache");
                Directory.CreateDirectory(path);

                var sha256 = MD5.Create(); //utf8 here as well
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key.Key));
                var result = Convert.ToBase64String(bytes);

                foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
                {
                    result = result.Replace(invalidFileNameChar.ToString(), string.Empty);
                }
                path = Path.Combine(path, result + ".png");
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
        }

        private List<RootObject> HideTopLevelObjects()
        {
            var roots = new List<RootObject>();
            var objects = GameObject.FindObjectsOfType<Transform>();
            foreach (var transform in objects)
            {
                var found = false;
                foreach (var root in roots)
                {
                    if (root.transform.GetInstanceID() != transform.root.GetInstanceID())
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    roots.Add(new RootObject() { transform = transform, activeState = transform.gameObject.activeSelf });
                    transform.gameObject.SetActive(false); // hide the game object temporarily
                    // Debug.Log(transform.name);
                }
            }

            return roots;
        }

        private void LoadPreviewGameObjects(string key, ContentManager<MeshPreviewTextureArgs> content, GameObject gameObject)
        {
            var objLoaderFactory = new ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create(new MaterialNullStreamProvider());

            LoadResult result;
            var path = Path.IsPathRooted(key) ? key : Path.Combine(content.RootDirectory, key);
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                result = objLoader.Load(fileStream);
            }

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
                mesh.SetTriangles(triangleGroup.Faces.SelectMany(
                    face =>
                    {
                        var indexes = new int[face.Count];
                        for (var j = 0; j < face.Count; j++)
                        {
                            indexes[j] = face[j].VertexIndex - 1;
                        }

                        return indexes;
                    }).ToArray(), i);
            }

            mesh.Optimize();
            var filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
        }

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        public bool CanRead(MeshPreviewTextureArgs key, ContentManager<MeshPreviewTextureArgs> content)
        {
            var path = Path.IsPathRooted(key.Key) ? key.Key : Path.Combine(content.RootDirectory, key.Key);
            var extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension) && extension.ToLower() == ".obj" && File.Exists(path);
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(MeshPreviewTextureArgs key, ContentManager<MeshPreviewTextureArgs> content, Action<ReadAsyncArgs<MeshPreviewTextureArgs, object>> completedCallback)
        {
            if (completedCallback == null)
            {
                throw new ArgumentNullException("completedCallback");
            }

            var scheduler = CoroutineManager.Instance;
            scheduler.StartCoroutine(this.GetData(key, completedCallback, content));
        }

        /// <summary>
        /// Gets the data from the 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        /// <param name="content"></param>
        /// <param name="url">The url to be requested.</param>
        /// <returns>Returns a <see cref="IEnumerable"/> coroutine.</returns>
        private IEnumerator GetData(MeshPreviewTextureArgs key, Action<ReadAsyncArgs<MeshPreviewTextureArgs, object>> completedCallback, ContentManager<MeshPreviewTextureArgs> content)
        {
#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<MeshPreviewTextureArgs, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<MeshPreviewTextureArgs, object>();
#endif

            Texture2D texture = null;
            var meshPreviewTexture = this.DoLoadCache(key);
            if (meshPreviewTexture != null)
            {
                args.Progress = 100;
                args.State = ReadState.Completed;
                args.Key = key;
                args.Result = meshPreviewTexture;
                completedCallback(args);
                yield return null;
            }
            else
            {
                // hide and disable all top level objects
                var roots = this.HideTopLevelObjects();

                args.Progress = 30;
                args.State = ReadState.Working;
                args.Key = key;
                completedCallback(args);
                yield return new WaitForEndOfFrame();

                GameObject cameraObject;
                Camera camera;
                var previewObject = this.DoSetupObjects(key, content, out cameraObject, out camera);

                // TODO: Hide mesh renderes before rendering to prevent any other scene data from being rendered along with preview

                // we need to manually render one frame with the camera here
                camera.Render();

                // generate a preview texture
                texture = new Texture2D(key.Width, key.Height, TextureFormat.ARGB32, true);
                texture.ReadPixels(new Rect(0, 0, key.Width, key.Height), 0, 0);
                texture.Apply();

                // destroy preview objects
                Object.DestroyImmediate(previewObject);
                Object.DestroyImmediate(cameraObject);

                args.Progress = 60;
                args.State = ReadState.Working;
                args.Key = key;
                completedCallback(args);
                yield return new WaitForEndOfFrame();

                this.DoRestoreScene(roots);

                args.Progress = 90;
                args.State = ReadState.Working;
                args.Key = key;
                completedCallback(args);
                yield return new WaitForEndOfFrame();

                this.DoSaveToCache(key, texture);
            }

            args.Progress = 100;
            args.State = ReadState.Completed;
            args.Key = key;
            args.Result = texture;
            completedCallback(args);
        }
    }
}