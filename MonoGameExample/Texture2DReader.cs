namespace MonoGameExample
{
    using Codefarts.ContentManager;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.IO;
    using System.Net;

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
            using (var client = new WebClient())
            {
                var bytes = client.DownloadData(key);
                using (var mem = new MemoryStream())
                {
                    mem.Write(bytes, 0, bytes.Length);
                    mem.Seek(0, SeekOrigin.Begin);
                    return Texture2D.FromStream(this.Game.GraphicsDevice, mem);
                }
            }
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
        public void ReadAsync(Uri key, ContentManager<Uri> content, Action<ReadAsyncArgs<Uri, object>> completedCallback)
        {
            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
                {
                    if (completedCallback != null)
                    {
                        using (var mem = new MemoryStream())
                        {
                            var bytes = new byte[1024];
                            var count = e.Result.Read(bytes, 0, bytes.Length);
                            while (count > 0)
                            {
                                mem.Write(bytes, 0, count);
                                count = e.Result.Read(bytes, 0, bytes.Length);
                            }

                            mem.Seek(0, SeekOrigin.Begin);
                            completedCallback(new ReadAsyncArgs<Uri, object>() { State =  ReadState.Completed, Result = Texture2D.FromStream(this.Game.GraphicsDevice, mem) });
                        }
                    }
                };

            client.OpenReadAsync(key);
        }
    }
}