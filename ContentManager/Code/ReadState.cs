// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager
{
    /// <summary>
    /// Provides a state enum for asynchronous read operation.
    /// </summary>
    public enum ReadState
    {
        /// <summary>
        /// Read operation is still working.
        /// </summary>
        Working,

        /// <summary>
        /// Read operation has completed.
        /// </summary>
        Completed,
    }
}