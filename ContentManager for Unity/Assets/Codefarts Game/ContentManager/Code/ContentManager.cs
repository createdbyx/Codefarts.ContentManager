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

    /// <summary>
    /// Provides a content manager for loading and caching assets.
    /// </summary>
    /// <typeparam name="TKey">The type used as the indexer key.</typeparam>
    public class ContentManager<TKey>
    {
        /// <summary>
        /// Holds a reference to the readers dictionary.
        /// </summary>
        protected readonly Dictionary<Type, IReader<TKey>> Readers;

        /// <summary>
        /// Holds a singleton instance of a <see cref="ContentManager"/> type.
        /// </summary>
        private static ContentManager<TKey> singleton;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentManager{TKey}"/> class. 
        /// </summary>
        /// <param name="rootDirectory">
        /// A path to where the assets are stored.
        /// </param>
        public ContentManager(string rootDirectory)
        {
            this.Assets = new Dictionary<TKey, object>();
            this.RootDirectory = rootDirectory;
            this.Readers = new Dictionary<Type, IReader<TKey>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentManager{TKey}"/> class. 
        /// </summary>
        public ContentManager()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Gets a singleton instance of a <see cref="ContentManager"/> type.
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
        public virtual Dictionary<TKey, object> Assets { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not <see cref="Unload"/> method will call <see cref="IDisposable.Dispose"/> method on 
        /// cached assets that implement <see cref="IDisposable"/> interface.
        /// </summary>
        public bool AutoDisposeOnUnload { get; set; }

        /// <summary>
        /// Gets or sets the root directory associated with this <see cref="ContentManager{T}"/>.
        /// </summary>
        public virtual string RootDirectory { get; set; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of type <see cref="IReader{T}"/> types that have been registered with the <see cref="ContentManager{T}"/>.
        /// </summary>
        /// <returns>Returns a <see cref="IEnumerable{T}"/> of <see cref="IReader{T}"/>'s.</returns>
        public virtual IEnumerable<IReader<TKey>> GetReaders()
        {
            foreach (var reader in this.Readers)
            {
                yield return reader.Value;
            }
        }

        /// <summary>
        /// Loads a asset from disk if not already done so and caches it.
        /// </summary>
        /// <typeparam name="TReturnValue">The asset type that will be returned.</typeparam>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="cache">If true the asset will be cached.</param>
        /// <returns>Returns a reference to a cached asset.</returns>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>If the asset is already cached it will returned the cached asset.</remarks>
        public TReturnValue Load<TReturnValue>(TKey key, bool cache)
        {
            // if "K" is a string type check if the string is empty and if so throw an exception
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
                throw new ArgumentException("No reader is available for the specified type.");
            }

            // get reader and read asset
            var reader = this.Readers[type];
            var readerObject = (TReturnValue)reader.Read(key, this);

            // if caching then ass returned value to the cache
            if (cache)
            {
                lock (this.Assets)
                {
                    if (!this.Assets.ContainsKey(key))
                    {
                        this.Assets.Add(key, readerObject);
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
        /// <param name="cache">If true the asset will be cached.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>If the asset is already cached it will returned the cached asset.</remarks>
        public void Load<TReturnValue>(TKey key, Action<TReturnValue> completedCallback, bool cache)
        {
            // if "K" is a string type check if the string is empty and if so throw an exception
            if (key is string && string.IsNullOrEmpty((key as string).Trim()))
            {
                throw new ArgumentNullException("key");
            }

            // if already cached returned the cached asset
            if (this.Assets.ContainsKey(key) && completedCallback != null)
            {
                completedCallback((TReturnValue)this.Assets[key]);
                return;
            }

            // try to find a matching reader that return the same type specified with "K"
            var type = typeof(List<TReturnValue>).GetGenericArguments()[0];
            if (!this.Readers.ContainsKey(type))
            {
                throw new ArgumentException("No reader is available for the specified type.");
            }

            // get reader and read asset
            var reader = this.Readers[type];
            reader.ReadAsync(
            key, 
            this, 
            readerObject =>
                {
                    // if caching then add returned value to the cache
                    if (cache)
                    {
                        lock (this.Assets)
                        {
                            if (!this.Assets.ContainsKey(key))
                            {
                                this.Assets.Add(key, readerObject);
                            }
                        }
                    }

                    // if a callback was specified call it now and pass in the reference to the loaded asset
                    if (completedCallback != null)
                    {
                        completedCallback((TReturnValue)readerObject);
                    }
                });
        }

        /// <summary>
        /// Registers a<see cref="IReader{T}"/> with the <see cref="ContentManager{T}"/>.
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
            if (this.Readers.ContainsKey(reader.Type))
            {
                throw new ArgumentException("A reader that reads the '{0}' type has already been added!");
            }

            this.Readers.Add(reader.Type, reader);
        }

        /// <summary>
        /// Unloads all cached assets.
        /// </summary>
        /// <remarks>If an asset implements <see cref="IDisposable"/> the assets <see cref="IDisposable.Dispose"/> method will be called.</remarks>
        public virtual void Unload()
        {
            try
            {
                if (this.AutoDisposeOnUnload)
                {
                    foreach (IDisposable disposable in this.Assets.Values)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                this.Assets.Clear();
            }
        }
    }
}