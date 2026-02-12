using System.Collections.Generic;
using UnityEngine;

namespace LocationFinder.UIUX.Favourites
{
    public static class FavouritesPersistence
    {
        private const string Key = "LF_FAV_IDS";

        public static void Save(HashSet<string> ids)
        {
            string raw = string.Join("|", ids);
            PlayerPrefs.SetString(Key, raw);
            PlayerPrefs.Save();
        }

        public static HashSet<string> Load()
        {
            string raw = PlayerPrefs.GetString(Key, "");
            var set = new HashSet<string>();

            if (string.IsNullOrWhiteSpace(raw))
                return set;

            var parts = raw.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                var id = parts[i];
                if (!string.IsNullOrWhiteSpace(id))
                    set.Add(id);
            }
            return set;
        }
    }
}
