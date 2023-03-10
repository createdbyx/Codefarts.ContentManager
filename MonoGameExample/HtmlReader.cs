namespace MonoGameExample
{
    using Codefarts.ContentManager;
    using System;
    using System.Net;

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
            using (var client = new WebClient())
            {
                var result = client.DownloadString(key);
                return new HtmlData() { Markup = result };
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
            client.DownloadStringCompleted += (s, e) =>
                {
                    if (completedCallback != null)
                    {
                        completedCallback(new ReadAsyncArgs<Uri, object>() { State = ReadState.Completed, Result = new HtmlData() { Markup = e.Result } });
                    }
                };

            client.DownloadStringAsync(key);
        }
    }
}