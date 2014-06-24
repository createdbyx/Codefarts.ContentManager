// <copyright>
//   Copyright (c) 2012 Codefarts
//   All rights reserved.
//   contact@codefarts.com
//   http://www.codefarts.com
// </copyright>

namespace Codefarts.CoreProjectCode.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using Codefarts.Localization;

    using UnityEngine;

    public class LocalizationHelpers
    {
        /// <summary>
        /// Sets up and loads localized strings for the localization system.
        /// </summary>
        public static void LoadLocalizationData(CultureInfo currentCulture, string path)
        {
            // attempt to read localization data for the current culture
            var data = Resources.LoadAll<TextAsset>(path);
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning(string.Format("No localization file(s) found for '{0}'", currentCulture.Name));
                return;
            }

            var entries = new Dictionary<string, string>();
            foreach (var item in data)
            {
                try
                {
                    var lines = item.text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var index = line.IndexOf(" ", StringComparison.Ordinal);
                        if (index == -1)
                        {
                            continue;
                        }

                        var keyValue = line.Substring(0, index).Trim();
                        var value = line.Substring(index).Trim();
                        value = value.Replace(@"\r\n", "\r\n");
                        var prefix = string.Empty;
                        if (!entries.ContainsKey(prefix + keyValue))
                        {
                            entries.Add(prefix + keyValue, value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }

            // register localization entries
            var local = LocalizationManager.Instance;
            local.Register(currentCulture, entries);
        }
    }
}
