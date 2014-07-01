﻿// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.Collections;
    using System.IO;

    using UnityEngine;

#if USEOBJECTPOOLING
    using Codefarts.ObjectPooling;
#endif

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class Texture2DResourceReader : IReader<string>
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
            var result = Resources.Load<Texture2D>(key);
            if (result == null)
            {
                throw new IOException(string.Format("Failed to load '{0}'.", key));
            }

            return result;
        }

        /// <summary>
        /// Determines if the reader can read the data.
        /// </summary>
        /// <param name="key">The id to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <returns>Returns true if the data can be read by this reader; otherwise false.</returns>
        public bool CanRead(string key, ContentManager<string> content)
        {
            return true;
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(string key, ContentManager<string> content, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            if (completedCallback == null)
            {
                throw new ArgumentNullException("completedCallback");
            }

            var scheduler = CoroutineManager.Instance;
            scheduler.StartCoroutine(this.GetData(key, completedCallback));
        }

        /// <summary>
        /// Gets the data from the 
        /// </summary>
        /// <param name="url">The url to be requested.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        /// <returns>Returns a <see cref="IEnumerable"/> coroutine.</returns>
        private IEnumerator GetData(string key, Action<ReadAsyncArgs<string, object>> completedCallback)
        {
            var result = Resources.Load<Texture2D>(key);

#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<string, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<string, object>();
#endif
            args.Progress = 100;
            args.State = ReadState.Completed;
            args.Key = key;
            args.Result = result;
            if (result == null)
            {
                args.Error = new IOException(string.Format("Failed to load '{0}'.", key));
            }

            completedCallback(args);
            return null;
        }
    }
}