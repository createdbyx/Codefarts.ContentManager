/*
<copyright>
  Copyright (c) 2012 Codefarts
  All rights reserved.
  contact@codefarts.com
  http://www.codefarts.com
</copyright>
*/
namespace Codefarts.ContentManager.Scripts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;

    using Codefarts.ContentManager;

    using UnityEditor;

    using UnityEngine;
#if USEOBJECTPOOLING
    using Codefarts.ObjectPooling;
#endif

    /// <summary>
    /// Provides a <see cref="Texture2D"/> reader.
    /// </summary>
    public class Texture2DUriReader : IReader<Uri>
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
        public object Read(Uri key, ContentManager<Uri> content)
        {
            if (!key.IsFile)
            {
                throw new NotSupportedException("Can only read a local file url when reading synchronously.");
            }

            var texture = new Texture2D(8, 8, TextureFormat.ARGB32, true);
            texture.LoadImage(File.ReadAllBytes(key.LocalPath));
            return texture;
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
        private IEnumerator GetData(Uri url, Action<ReadAsyncArgs<Uri, object>> completedCallback)
        {
            var client = new WebClient();
#if USEOBJECTPOOLING
                    var args = ObjectPoolManager<ReadAsyncArgs<Uri, object>>.Instance.Pop();
#else
            var args = new ReadAsyncArgs<Uri, object>();
#endif

            args.Result = null;
            args.Key = url;
            args.State = ReadState.Working;
            byte[] data = null;
            client.DownloadProgressChanged += (s, e) =>
                {
                    Debug.Log("Progresschanged: " + e.ProgressPercentage);
                    args.Progress = e.ProgressPercentage;
                };

            client.DownloadDataCompleted += (s, e) =>
            {
                Debug.Log("Completed");
                args.Progress = 100;
                args.State = ReadState.Completed;

                if (e.Error == null)
                {
                    data = e.Result;
                }
                else
                {
                    args.Error = e.Error;
                }
            };
                   
            client.DownloadDataAsync(url);
            while (args.Error == null && args.State != ReadState.Completed)
            {
                Debug.Log("Progress: " + args.Progress);
                completedCallback(args);
                yield return new WaitForFixedUpdate();
            }

            // if no error try to load image
            if (args.Error == null)
            {
                Debug.Log("no error");
                try
                {
                    var texture = new Texture2D(8, 8, TextureFormat.ARGB32, true);
                    texture.LoadImage(data);
                    args.Result = texture;
                }
                catch (Exception ex)
                {
                    args.Error = ex;
                    args.Result = null;
                }
            }
            else
            {
                Debug.Log(args.Error.ToString());
            }

            completedCallback(args);
        }
    }
}