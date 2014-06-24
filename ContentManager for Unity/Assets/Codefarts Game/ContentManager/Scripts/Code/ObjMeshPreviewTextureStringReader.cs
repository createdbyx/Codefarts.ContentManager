namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Codefarts.ContentManager.Scripts.Code;

    using UnityEngine;

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class ObjMeshPreviewTextureStringReader : IReader<string>
    {
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
        public object Read(string key, ContentManager<string> content)
        {
            // hide and disable all top level objects
            var roots = new List<Transform>();
            var objects = GameObject.FindObjectsOfType<Transform>();
            foreach (var transform in objects)
            {
                if (!roots.Contains(transform.root))
                {
                    roots.Add(transform);
                    Debug.Log(transform.name);
                }
            }


            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, true);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            return new MeshPreviewTexture() { Texture = texture };
        }

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        public bool CanRead(string key, ContentManager<string> content)
        {
            var extension = Path.GetExtension(key);
            return File.Exists(key) && extension == ".obj";
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(string key, ContentManager<string> content, Action<object> completedCallback)
        {
            UnityThreadHelper.Dispatcher.Dispatch(() => completedCallback(this.Read(key, content)));
        }
    }
}