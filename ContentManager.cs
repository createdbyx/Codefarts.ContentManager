namespace CBX.ContentManager
{
    using System;
    using System.Collections.Generic;

    public class ContentManager
    {
        public virtual Dictionary<string, object> Assets { get; protected set; }
        public virtual string RootDirectory { get; set; }
        protected readonly Dictionary<Type, IReader> Readers;

        /// <summary>
        /// Holds a singleton instance of a <see cref="ContentManager"/> type.
        /// </summary>
        private static ContentManager singleton;

        /// <summary>
        /// Gets a singleton instance of a <see cref="ContentManager"/> type.
        /// </summary>
        public static ContentManager Instance
        {
            get
            {
                return singleton ?? (singleton = new ContentManager(null));
            }
        }

        public virtual IEnumerable<IReader> GetReaders()
        {
            foreach (var reader in this.Readers)
            {
                yield return reader.Value;
            }
        }

        public virtual void Register(IReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (this.Readers.ContainsKey(reader.Type))
            {
                throw new ArgumentException("A reader that reads the '{0}' type has already been added!");
            }

            this.Readers.Add(reader.Type, reader);
        }

        public virtual void Unload()
        {
            try
            {
                foreach (IDisposable disposable in this.Assets.Values)
                {
                    disposable.Dispose();
                }
            }
            finally
            {
                this.Assets.Clear();
            }
        }

        public T Load<T>(string key, bool cache)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (this.Assets.ContainsKey(key))
            {
                return (T)this.Assets[key];
            }

            var type = typeof(List<T>).GetGenericArguments()[0];
            if (!this.Readers.ContainsKey(type))
            {
                throw new ArgumentException("No reader is available for the specified type.");
            }

            var reader = this.Readers[type];
            var readerObject = (T)reader.Read(key, this);
            if (cache)
            {
                lock (this.Assets)
                {
                    if (this.Assets.ContainsKey(key))
                    {
                        this.Assets.Add(key, readerObject);
                    }
                }
            }

            return readerObject;
        }

      public void Load<T>(string key, Action<T> completedCallback, bool cache)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (this.Assets.ContainsKey(key))
            {
                if (completedCallback != null)
                {
                    completedCallback((T)this.Assets[key]);
                }

                return;
            }

            var type = typeof(List<T>).GetGenericArguments()[0];
            if (!this.Readers.ContainsKey(type))
            {
                throw new ArgumentException("No reader is available for the specified type.");
            }

            var reader = this.Readers[type];
            reader.ReadAsync(key, this, readerObject =>
                {
                    if (cache)
                    {
                        this.Assets.Add(key, readerObject);
                    }

                    if (completedCallback != null)
                    {
                        completedCallback((T)readerObject);
                    }
                });
        }

        public ContentManager(string rootDirectory)
        {
            this.Assets = new Dictionary<string, object>();
            this.RootDirectory = rootDirectory;
            this.Readers = new Dictionary<Type, IReader>();
        }
    }
}