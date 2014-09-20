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
    using UnityEngine;

    /// <summary>
    /// Provides a singleton coroutine manager for executing coroutines.
    /// </summary>
    public class CoroutineManager : MonoBehaviour
    {
        /// <summary>
        /// Holds a singleton instance of the <see cref="CoroutineManager"/> type.
        /// </summary>
        private static CoroutineManager instance;

        /// <summary>
        /// Gets a singleton instance of the <see cref="CoroutineManager"/> type.
        /// </summary>
        public static CoroutineManager Instance
        {
            get
            {
                // if instance is null create the singleton
                if (instance == null)
                {
                    var obj = new GameObject("CoroutineManager_ContentManager");
                    instance = obj.AddComponent<CoroutineManager>();
                }

                return instance;
            }
        }
    }
}
