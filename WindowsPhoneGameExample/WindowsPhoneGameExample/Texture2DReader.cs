﻿namespace SilverlightExample
{
    using System;
    using System.Net;

    using Codefarts.ContentManager;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class Texture2DReader : IReader<Uri>
    {
        /// <summary>
        /// Gets or sets the <see cref="Game"/> reference used to create the textures.
        /// </summary>
        public Game Game { get; set; }

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
        public object Read(Uri key, ContentManager<Uri> content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        public bool CanRead(Uri key, ContentManager<Uri> content)
        {
            return true;
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(Uri key, ContentManager<Uri> content, Action<object> completedCallback)
        {
            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
                {
                    if (completedCallback != null)
                    {
                        completedCallback(Texture2D.FromStream(this.Game.GraphicsDevice, e.Result));
                    }
                };

            client.OpenReadAsync(key);
        }
    }
}