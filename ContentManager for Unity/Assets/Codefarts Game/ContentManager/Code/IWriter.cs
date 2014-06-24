// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager
{
    using System;

    /// <summary>
    /// Provides a interface for content writers.
    /// </summary>
    /// <typeparam name="T">
    /// The type use as the indexer key.
    /// </typeparam>
    public interface IWriter<T>
    {
        /// <summary>
        /// Gets the <see cref="Type"/> that this writer implementation handles.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Writes data to a file.
        /// </summary>
        /// <param name="key">The id to be written to.</param>
        /// <param name="content">A reference to the content manager that invoked the write.</param>
        /// <returns>Returns a type representing the data.</returns>
        object Write(T key, ContentManager<T> content);

        /// <summary>
        /// Determines if the writer can write the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be written to by this writer; otherwise false.</returns>
        bool CanWrite(T key, ContentManager<T> content);

        /// <summary>
        /// Writes to a file asynchronously.
        /// </summary>
        /// <param name="key">The id to be written to.</param>
        /// <param name="content">A reference to the content manager that invoked the write.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the write is complete.</param>
        void WriteAsync(T key, ContentManager<T> content, Action<object> completedCallback);
    }
}