namespace CBX.ContentManager
{
    using System;
    
    /// <summary>
    /// Provides a interface for content readers.
    /// </summary>
    public interface IReader
    {
        /// <summary>
        /// Reads a file and returns a type representing the data.
        /// </summary>
        /// <param name="filename">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns a type representing the data.</returns>
        object Read(string filename, ContentManager content);

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="filename">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        /// <returns>Returns a type representing the data.</returns>
        void ReadAsync(string filename, ContentManager content, Action<object> completedCallback);
        
        /// <summary>
        /// Gets the <see cref="Type"/> that this reader implementation returns.
        /// </summary>
        Type Type { get; }
    }
}