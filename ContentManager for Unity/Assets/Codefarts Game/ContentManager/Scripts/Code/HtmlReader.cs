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
    using System.Net;

    using Codefarts.ContentManager;

    using UnityEngine;

    /// <summary>
    /// Provides a html reader.
    /// </summary>
    public class HtmlReader : IReader<Uri>
    {
        /// <summary>
        /// Gets the <see cref="IReader{T}.Type"/> that this reader implementation returns.
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(HtmlData);
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
            var client = new WebClient();
            var data = new HtmlData() { Markup = client.DownloadString(key) };
            return data;
        }

        /// <summary>
        /// Reads a file asynchronously and returns a type representing the data.
        /// </summary>
        /// <param name="key">The file to be read.</param>
        /// <param name="content">A reference to the content manager that invoked the read.</param>
        /// <param name="completedCallback">Specifies a callback that will be invoked when the read is complete.</param>
        public void ReadAsync(Uri key, ContentManager<Uri> content, Action<object> completedCallback)
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
        private IEnumerator GetData(Uri url, Action<object> completedCallback)
        {
            var www = new WWW(url.ToString());
            yield return www;

            completedCallback(new HtmlData { Markup = !string.IsNullOrEmpty(www.error) ? www.error : www.text });
        }
    }
}