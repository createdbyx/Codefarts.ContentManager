/*
<copyright>
  Copyright (c) 2012 Codefarts
  All rights reserved.
  contact@codefarts.com
  http://www.codefarts.com
</copyright>
*/
namespace Codefarts.ContentManager
{
    using System;

    /// <summary>
    /// Provides a interface for content readers.
    /// </summary>
    /// <typeparam name="T">
    /// The type use as the indexer key.
    /// </typeparam>
    public interface IReader<T>
    {
        /// <summary>
        /// Gets the <see cref="Type"/> that this reader implementation returns.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Reads a file and returns a type representing the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns a type representing the data.</returns>
        object Read(T key, ContentManager<T> content);

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        bool CanRead(T key, ContentManager<T> content);

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        void ReadAsync(T key, ContentManager<T> content, Action<ReadAsyncArgs<T, object>> completedCallback);
    }
}