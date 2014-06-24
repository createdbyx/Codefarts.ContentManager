﻿namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.IO;

    using UnityEngine;

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
            var importer = new ObjImporter();
            return importer.ImportFile(key);
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
            UnityThreadHelper.Dispatcher.Dispatch(() =>
                {
                    var importer = new ObjImporter();
                    return importer.ImportFile(key);
                });
        }
    }
}