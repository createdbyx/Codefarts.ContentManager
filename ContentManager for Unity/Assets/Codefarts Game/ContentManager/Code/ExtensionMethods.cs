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
        /// Asynchronously loads a asset from disk if not already done so and caches it.
        /// </summary>
        /// <typeparam name="TReturnValue">
        /// The asset type that will be returned.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// The type of key that is used to specify a location of a asset.
        /// </typeparam>
        /// <param name="manager">
        /// A reference to a <see cref="ContentManager{TKey}"/> that will be used to load the asset.
        /// </param>
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
        public static TReturnValue Load<TReturnValue, TKey>(this ContentManager<TKey> manager, TKey key)
        {
            return manager.Load<TReturnValue>(key, true);
        }

        /// <summary>
        /// Asynchronously loads a asset from disk if not already done so and caches it.
        /// </summary>
        /// <typeparam name="TReturnValue">The asset type that will be returned.</typeparam>
        /// <typeparam name="TKey">The type of key that is used to specify a location of a asset.</typeparam>
        /// <param name="manager">A reference to a <see cref="ContentManager{TKey}"/> that will be used to load the asset.</param>
        /// <param name="key">The key that points to the asset to load.</param>
        /// <param name="completedCallback">A reference to a callback method that will be invoked when loading completes.</param>
        /// <exception cref="ArgumentNullException">If the <see cref="key"/> is a string type and has no value.</exception>
        /// <exception cref="ArgumentException">If unable to find a registered <see cref="IReader{T}.Type"/> that matches the <see cref="TReturnValue"/> type.</exception>
        /// <exception cref="NullReferenceException">Can occur if no reader for type <see cref="TReturnValue"/> could be found.</exception>
        /// <remarks>This method will cause the asset to be cached. If the asset is already cached it will returned the cached asset.</remarks>
        public static void Load<TReturnValue, TKey>(this ContentManager<TKey> manager, TKey key, Action<TReturnValue> completedCallback)
        {
            manager.Load(key, completedCallback, true);
        }
    }
}