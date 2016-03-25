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

    /// <summary>
    /// Provides extension methods for the <see cref="ContentManager{TKey}"/> type.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Loads a asset from disk with the option to cache the data.
        /// </summary>
        /// <typeparam name="T">The asset type that will be returned.</typeparam>
        /// <typeparam name="K">The type used for the key.</typeparam>
        /// <param name="manager">Reference to the command manager that will perform the loading.</param>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="cache">If true the asset will be cached.</param>
        /// <param name="cachingCallback">A <see cref="Func{TResult}"/> callback that will be invoked allowing the caller to perform caching logic. If returns true the asset will be refreshed by removing and reloading it.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="T"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="T"/> could be found.</exception>
        /// <returns>Returns a reference to a cached asset.</returns>
        /// <remarks>If the asset is already cached it will returned the cached asset.</remarks>
        public static T Load<T, K>(this ContentManager<K> manager, K key, bool cache, Func<T, bool> cachingCallback)
        {
            var model = manager.Load<T>(key, cache);

            // re read load the content every x seconds (IE: Keep cached for x seconds)
            if (cachingCallback(model))
            {
                manager.Assets.Remove(key);
                model = manager.Load<T>(key, cache);
            }

            return model;
        }
    }
}