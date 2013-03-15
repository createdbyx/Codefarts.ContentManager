namespace CBX.ContentManager
{
    using System;

    public static class ExtensionMethods
    {
        public static T Load<T>(this ContentManager manager, string key)
        {
            return manager.Load<T>(key, true);
        }

        public static void Load<T>(this ContentManager manager, string key, Action<T> completedCallback)
        {
            manager.Load(key, completedCallback, true);
        }
    }
}