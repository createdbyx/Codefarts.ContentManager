namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;

    using UnityEngine;

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class Texture2DStringReader : IReader<string>
    {
        /// <summary>
        /// Gets the <see cref="IReader{T}.Type"/> that this reader implementation returns.
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(Texture2D);
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
            var texture = new Texture2D(4, 4, TextureFormat.DXT5, true);
            texture.LoadImage(File.ReadAllBytes(key));
            return texture;
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
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png";
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(string key, ContentManager<string> content, Action<object> completedCallback)
        {
           UnityThreadHelper.Dispatcher.Dispatch(() =>
            {
                var texture = new Texture2D(4, 4, TextureFormat.DXT5, true);
                texture.LoadImage(File.ReadAllBytes(key));
                completedCallback(texture);
            });
        }
    }
}