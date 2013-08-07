﻿namespace SilverlightExample
{
    using System;
    using System.Net;

    using Codefarts.ContentManager;

    /// <summary>
    /// Provides a html reader.
    /// </summary>
    public class HtmlReader : IReader<string>
    {
        /// <summary>
        /// Gets the <see cref="IReader{T}.Type"/> that this reader implementation returns.
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(string);
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(string key, ContentManager<string> content, Action<object> completedCallback)
        {
            var client = new WebClient();
            client.DownloadStringCompleted += (s, e) =>
                {
                    if (completedCallback != null)
                    {
                        completedCallback(e.Result);
                    }
                };

            client.DownloadStringAsync(new Uri(key));
        }
    }
}