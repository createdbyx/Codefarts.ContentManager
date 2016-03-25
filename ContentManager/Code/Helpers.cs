namespace Codefarts.ContentManager
{
#if UNITY_5
    using Codefarts.Localization;
#endif

    internal class Helpers
    {
        public static string GetResourceString(string name)
        {
#if UNITY_5
            return LocalizationManager.Instance.Get(name);
#else
            return Resources.ResourceManager.GetString(name);
#endif      
        }
    }
}
