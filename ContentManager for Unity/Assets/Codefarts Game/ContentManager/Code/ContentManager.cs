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
    using System.Collections.Generic;

#if !UNITY3D
    using Codefarts.ContentManager.Properties;
#else
    using Codefarts.Localization;
#endif

    /// <summary>
    /// Provides a content manager for loading and caching assets.
    /// </summary>
    /// <typeparam name="TKey">The type used as the indexer key.</typeparam>
    public class ContentManager<TKey>
    {
        /// <summary>
        /// Holds a reference to the readers dictionary.
        /// </summary>
        protected readonly Dictionary<Type, IList<IReader<TKey>>> Readers;

        /// <summary>
        /// Holds a reference to the writers dictionary.
        /// </summary>
        protected readonly Dictionary<Type, IList<IWriter<TKey>>> Writers;

        /// <summary>
        /// Holds a singleton instance of a <see cref="ContentManager{TKey}"/> type.
        /// </summary>
        private static ContentManager<TKey> singleton;

        /// <summary>
        /// Holds the dictionary for the <see cref="Assets"/> property.
        /// </summary>
        private Dictionary<TKey, object> assets;

        /// <summary>
        /// Holds the loading queue value for the <see cref="LoadingQueue"/> property.
        /// </summary>
        private int loadingQueue;

        /// <summary>
        /// Holds the saving queue value for the <see cref="SavingQueue"/> property.
        /// </summary>
        private int savingQueue;

        /// <summary>
        /// Holds the value for the <see cref="RootDirectory"/> property.    
        /// </summary>
        private string rootDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentManager{TKey}"/> class. 
        /// </summary>
        /// <param name="rootDirectory">
        /// A path to where the assets are stored.
        /// </param>
        public ContentManager(string rootDirectory)
        {
            this.assets = new Dictionary<TKey, object>();
            this.rootDirectory = rootDirectory;
            this.Readers = new Dictionary<Type, IList<IReader<TKey>>>();
            this.Writers = new Dictionary<Type, IList<IWriter<TKey>>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentManager{TKey}"/> class. 
        /// </summary>
        public ContentManager()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Gets a singleton instance of a <see cref="ContentManager{TKey}"/> type.
        /// </summary>
        public static ContentManager<TKey> Instance
        {
            get
            {
                return singleton ?? (singleton = new ContentManager<TKey>(null));
            }
        }

        /// <summary>
        /// Gets or sets a reference to a dictionary for storing cached assets.
        /// </summary>
        public virtual Dictionary<TKey, object> Assets
        {
            get
            {
                return this.assets;
            }

            protected set
            {
                this.assets = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not <see cref="Unload"/> method will call <see cref="IDisposable.Dispose"/> method on 
        /// cached assets that implement <see cref="IDisposable"/> interface.
        /// </summary>
        public virtual bool AutoDisposeOnUnload { get; set; }

        /// <summary>
        /// Gets the asynchronous loading queue.
        /// </summary>
        /// <remarks>This value indicates the number of unfinished asynchronous load operations.</remarks>
        public int LoadingQueue
        {
            get
            {
                return this.loadingQueue;
            }
        }

        /// <summary>
        /// Gets the asynchronous saving queue.
        /// </summary>
        /// <remarks>This value indicates the number of unfinished asynchronous save operations.</remarks>
        public int SavingQueue
        {
            get
            {
                return this.savingQueue;
            }
        }

        /// <summary>
        /// Gets or sets the root directory associated with this <see cref="ContentManager{TKey}"/>.
        /// </summary>
        public virtual string RootDirectory
        {
            get
            {
                return this.rootDirectory;
            }

            set
            {
                this.rootDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to a asset in the <see cref="Assets"/> cache.
        /// </summary>
        /// <param name="key">The key used to access the asset.</param>
        /// <returns>Returns a reference to a cached asset.</returns>
        public virtual object this[TKey key]
        {
            get
            {
                return this.assets[key];
            }

            set
            {
                this.assets[key] = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of type <see cref="IReader{T}"/> types that have been registered with the <see cref="ContentManager{TKey}"/>.
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of <see cref="IReader{T}"/>'s.</returns>
        public virtual IEnumerable<IReader<TKey>> GetReaders()
        {
            foreach (var list in this.Readers)
            {
                foreach (var reader in list.Value)
                {
                    yield return reader;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of type <see cref="IWriter{T}"/> types that have been registered with the <see cref="ContentManager{TKey}"/>.
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of <see cref="IWriter{T}"/>'s.</returns>
        public virtual IEnumerable<IWriter<TKey>> GetWriters()
        {
            foreach (var list in this.Writers)
            {
                foreach (var writer in list.Value)
                {
                    yield return writer;
                }
            }
        }

        #region Load Methods
        /// <summary>
        /// Synchronously loads a asset from disk if not already done so and caches it.
        /// </summary>
        /// <typeparam name="TReturnValue">
        /// The asset type that will be returned.
        /// </typeparam>
        /// <param name="key">
        /// The key that points to the asset to load.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <see cref="key"/> is a string type and has no value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Can occur if no reader for type <see cref="TReturnValue"/> could be found.
        /// </exception>
        /// <remarks>
        /// This method will cause the asset to be cached. If the asset is already cached it will returned the cached asset.
        /// </remarks>
        /// <returns>
        /// Returns a <see cref="TReturnValue"/> type containing the requested data.
        /// </returns>
        public virtual TReturnValue Load<TReturnValue>(TKey key)
        {
            return this.Load<TReturnValue>(key, true);
        }

        /// <summary>
        /// Loads a asset from disk with the option to cache the data.
        /// </summary>
        /// <typeparam name="TReturnValue">The asset type that will be returned.</typeparam>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="cache">If true the asset will be cached.</param>
        /// <returns>Returns a reference to a cached asset.</returns>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>If the asset is already cached it will returned the cached asset.</remarks>
        public virtual TReturnValue Load<TReturnValue>(TKey key, bool cache)
        {
            // if "key" is a string type check if the string is empty and if so throw an exception
            if (key is string && string.IsNullOrEmpty((key as string).Trim()))
            {
                throw new ArgumentNullException("key");
            }

            // if already cached returned the cached asset
            if (this.Assets.ContainsKey(key))
            {
                return (TReturnValue)this.Assets[key];
            }

            // try to find a matching reader that return the same type specified with "K"
            var type = typeof(List<TReturnValue>).GetGenericArguments()[0];
            if (!this.Readers.ContainsKey(type))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_NoReaderIsAvailable"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_NoReaderIsAvailable"));
#endif
            }

            // try to find a reader that can read the asset
            var readers = this.Readers[type];
            IReader<TKey> reader = null;
            foreach (var item in readers)
            {
                if (item.CanRead(key, this))
                {
                    reader = item;
                    break;
                }
            }

            // if reader is not assigned throw exception
            if (reader == null)
            {
#if UNITY3D
                throw new NotSupportedException(LocalizationManager.Instance.Get("ContentManager_ERR_NoReaderIsAvailable"));
#else
                throw new NotSupportedException(Resources.ResourceManager.GetString("ContentManager_ERR_NoReaderIsAvailable"));
#endif
            }

            // attempt to read the asset
            var readerObject = (TReturnValue)reader.Read(key, this);

            // if caching then ass returned value to the cache
            if (cache)
            {
                lock (this.assets)
                {
                    if (!this.assets.ContainsKey(key))
                    {
                        this.assets.Add(key, readerObject);
                    }
                }
            }

            return readerObject;
        }

        /// <summary>
        /// Asynchronously loads a asset from disk if not already done so and caches it.
        /// </summary>
        /// <typeparam name="TReturnValue">The asset type that will be returned.</typeparam>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="completedCallback">A reference to a callback method that will be invoked when loading completes.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>This method will cause the asset to be cached. If the asset is already cached it will returned the cached asset.</remarks>
        public virtual void Load<TReturnValue>(TKey key, Action<ReadAsyncArgs<TKey, TReturnValue>> completedCallback)
        {
            this.Load(key, completedCallback, true);
        }

        /// <summary>
        /// Asynchronously loads a asset from disk with the option to cache the data.
        /// </summary>
        /// <typeparam name="TReturnValue">The asset type that will be returned.</typeparam>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="completedCallback">A reference to a callback method that will be invoked when loading completes.</param>
        /// <param name="cache">If true the asset will be cached.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value, or the <see cref="completedCallback"/> is null.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>If the asset is already cached it will returned the cached asset.</remarks>
        public virtual void Load<TReturnValue>(TKey key, Action<ReadAsyncArgs<TKey, TReturnValue>> completedCallback, bool cache)
        {
            if (completedCallback == null)
            {
                throw new ArgumentNullException("completedCallback");
            }

            // if "key" is a string type check if the string is empty and if so throw an exception
            if (key is string && string.IsNullOrEmpty((key as string).Trim()))
            {
                throw new ArgumentNullException("key");
            }

            // if already cached returned the cached asset
            if (this.Assets.ContainsKey(key))
            {
#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<TKey, TReturnValue>>.Instance.Pop();
#else
                var args = new ReadAsyncArgs<TKey, TReturnValue>();
#endif
                args.Key = key;
                args.Progress = 100.0f;
                args.State = ReadState.Completed;
                args.Result = (TReturnValue)this.Assets[key];

                // if a callback was specified call it now and pass in the reference to the loaded asset
                completedCallback(args);


                return;
            }

            // try to find a matching reader that return the same type specified with "TReturnValue"
            var type = typeof(List<TReturnValue>).GetGenericArguments()[0];
            if (!this.Readers.ContainsKey(type))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_NoReaderIsAvailable"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_NoReaderIsAvailable"));
#endif
            }

            // try to find a reader that can read the asset
            var readers = this.Readers[type];
            IReader<TKey> reader = null;
            foreach (var item in readers)
            {
                if (item.CanRead(key, this))
                {
                    reader = item;
                    break;
                }
            }

            // if reader is not assigned throw exception
            if (reader == null)
            {
#if UNITY3D
                throw new NotSupportedException(LocalizationManager.Instance.Get("ContentManager_ERR_NoReaderIsAvailable"));
#else
                throw new NotSupportedException(Resources.ResourceManager.GetString("ContentManager_ERR_NoReaderIsAvailable"));
#endif
            }

            // update the loading queue
            this.loadingQueue++;

            // perform asynchronous read
            reader.ReadAsync(
            key,
            this,
            readerObject =>
            {
                // if caching then add returned value to the cache
                if (readerObject.State == ReadState.Completed)
                {
                    // update loading queue
                    this.loadingQueue = Math.Max(0, this.loadingQueue - 1);

                    if (cache)
                    {
                        lock (this.assets)
                        {
                            if (!this.assets.ContainsKey(key))
                            {
                                this.assets.Add(key, readerObject.Result);
                            }
                        }
                    }
                }

#if USEOBJECTPOOLING
            var args = ObjectPoolManager<ReadAsyncArgs<TKey, TReturnValue>>.Instance.Pop();
#else
                var args = new ReadAsyncArgs<TKey, TReturnValue>();
#endif
                args.Key = readerObject.Key;
                args.Progress = readerObject.Progress;
                args.State = readerObject.State;
                args.Result = (TReturnValue)readerObject.Result;

                // if a callback was specified call it now and pass in the reference to the loaded asset
                completedCallback(args);
            });
        }
        #endregion

        #region Save Methods

        /// <summary>
        /// Saves a asset to disk.
        /// </summary>
        /// <typeparam name="TData">The asset type that will be saved.</typeparam>
        /// <param name="key">
        /// The key that points to the asset to save.
        /// </param>
        /// <param name="data">
        /// The data to be saved.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <see cref="key"/> is a string type and has no value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If unable to find a registered <see cref="IWriter{T}.Type"/> that matches the <see cref="TData"/> type.
        /// </exception>
        /// <exception cref="NullReferenceException">
        /// Can occur if no writer for type <see cref="TData"/> could be found.
        /// </exception>
        public virtual void Save<TData>(TKey key, TData data)
        {
            // if "key" is a string type check if the string is empty and if so throw an exception
            if (key is string && string.IsNullOrEmpty((key as string).Trim()))
            {
                throw new ArgumentNullException("key");
            }

            // try to find a matching writer that writes the same type specified with "TData"
            var type = typeof(List<TData>).GetGenericArguments()[0];
            if (!this.Writers.ContainsKey(type))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_NoWriterIsAvailable"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_NoWriterIsAvailable"));
#endif
            }

            // try to find a writer that can write the data
            var writers = this.Writers[type];
            IWriter<TKey> writer = null;
            foreach (var item in writers)
            {
                if (item.CanWrite(key, this))
                {
                    writer = item;
                    break;
                }
            }

            // if reader is not assigned throw exception
            if (writer == null)
            {
#if UNITY3D
                throw new NotSupportedException(LocalizationManager.Instance.Get("ContentManager_ERR_NoWriterIsAvailable"));
#else
                throw new NotSupportedException(Resources.ResourceManager.GetString("ContentManager_ERR_NoWriterIsAvailable"));
#endif
            }

            // attempt to write the asset
            writer.Write(key, data, this);
        }

        /// <summary>
        /// Asynchronously saves a asset to disk.
        /// </summary>
        /// <typeparam name="TData">The asset type that will be saved.</typeparam>
        /// <param name="key">The key that points to the asset to save.</param>
        /// <param name="data">
        /// The data to be saved.
        /// </param>
        /// <param name="completedCallback">A reference to a callback method that will be invoked when saving completes.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value, or the <see cref="completedCallback"/> is null.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IWriter{T}.Type"/> that matches the <see cref="TData"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no writer for type <see cref="TData"/> could be found.</exception>
        public virtual void Save<TData>(TKey key, TData data, Action completedCallback)
        {
            if (completedCallback == null)
            {
                throw new ArgumentNullException("completedCallback");
            }

            var callback = new Action(
                () =>
                {
                    // update saving queue
                    this.savingQueue = Math.Max(0, this.savingQueue - 1);

                    // if a callback was specified call it now and pass in the reference to the loaded asset
                    completedCallback();
                });

            // if "key" is a string type check if the string is empty and if so throw an exception
            if (key is string && string.IsNullOrEmpty((key as string).Trim()))
            {
                throw new ArgumentNullException("key");
            }

            // try to find a matching reader that return the same type specified with "TReturnValue"
            var type = typeof(List<TData>).GetGenericArguments()[0];
            if (!this.Writers.ContainsKey(type))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_NoWriterIsAvailable"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_NoWriterIsAvailable"));
#endif
            }

            // try to find a writer that can save the data
            var writers = this.Writers[type];
            IWriter<TKey> writer = null;
            foreach (var item in writers)
            {
                if (item.CanWrite(key, this))
                {
                    writer = item;
                    break;
                }
            }

            // if writer is not assigned throw exception
            if (writer == null)
            {
#if UNITY3D
                throw new NotSupportedException(LocalizationManager.Instance.Get("ContentManager_ERR_NoWriterIsAvailable"));
#else
                throw new NotSupportedException(Resources.ResourceManager.GetString("ContentManager_ERR_NoWriterIsAvailable"));
#endif
            }

            // update the saving queue
            this.savingQueue++;

            // perform asynchronous write
            writer.WriteAsync(key, data, this, callback);
        }
        #endregion

        /// <summary>
        /// Registers a<see cref="IReader{T}"/> with the <see cref="ContentManager{TKey}"/>.
        /// </summary>
        /// <param name="reader">The reference to the reader to to be registered.</param>
        /// <exception cref="ArgumentNullException">I the <see cref="reader"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If a reader has already been registered that returns the same type.</exception>
        public virtual void Register(IReader<TKey> reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // Check if 
            if (!this.Readers.ContainsKey(reader.Type))
            {
                this.Readers.Add(reader.Type, new List<IReader<TKey>>());
            }

            var list = this.Readers[reader.Type];
            if (list.Contains(reader))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_ReaderAlreadyAdded"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_ReaderAlreadyAdded"));
#endif
            }

            list.Add(reader);
        }

        /// <summary>
        /// Registers a<see cref="IReader{T}"/> with the <see cref="ContentManager{TKey}"/>.
        /// </summary>
        /// <param name="writer">The reference to the writer to to be registered.</param>
        /// <exception cref="ArgumentNullException">I the <see cref="writer"/> parameter is null.</exception>
        /// <exception cref="ArgumentException">If a writer has already been registered that writes the same type.</exception>
        public virtual void Register(IWriter<TKey> writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            // Check if 
            if (!this.Writers.ContainsKey(writer.Type))
            {
                this.Writers.Add(writer.Type, new List<IWriter<TKey>>());
            }

            var list = this.Writers[writer.Type];
            if (list.Contains(writer))
            {
#if UNITY3D
                throw new ArgumentException(LocalizationManager.Instance.Get("ContentManager_ERR_WriterAlreadyAdded"));
#else
                throw new ArgumentException(Resources.ResourceManager.GetString("ContentManager_ERR_WriterAlreadyAdded"));
#endif
            }

            list.Add(writer);
        }

        /// <summary>
        /// Unloads all cached assets.
        /// </summary>
        /// <remarks>If an asset implements <see cref="IDisposable"/> and <see cref="AutoDisposeOnUnload"/> the assets <see cref="IDisposable.Dispose"/> method will be called.</remarks>
        public virtual void Unload()
        {
            if (this.AutoDisposeOnUnload)
            {
                foreach (var disposable in this.Assets.Values)
                {
                    var disposableObject = disposable as IDisposable;
                    if (disposableObject != null)
                    {
                        disposableObject.Dispose();
                    }
                }
            }

            this.Assets.Clear();
        }
    }
}