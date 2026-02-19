using UnityEngine;
using LocationFinder.UIUX.Favourites;

namespace LocationFinder.UIUX
{
    public class FavouritesBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            var manager = Object.FindAnyObjectByType<FavouritesScrollManager>();
            var sources = Object.FindObjectsByType<FavouriteToggleSource>(FindObjectsSortMode.None);

            if (!manager)
            {
                Debug.LogError("[FavouritesBootstrap] Kein FavouritesScrollManager in der Scene gefunden.");
                return;
            }

            foreach (var s in sources)
            {
                if (!s) continue;

                // stabile ID automatisch ableiten:
                // erwartet Namen wie: Panel_03_DARK_PanelBlprintDarkRussland
                string stableId = ExtractStableIdFromName(s.gameObject.name);

                // Fallback: name als ID (besser als gar nichts)
                if (string.IsNullOrWhiteSpace(stableId))
                    stableId = s.gameObject.name;

                s.Init(manager, stableId);
            }
        }

        private static string ExtractStableIdFromName(string goName)
        {
            if (string.IsNullOrWhiteSpace(goName)) return "";

            int lastUnderscore = goName.LastIndexOf('_');
            if (lastUnderscore >= 0 && lastUnderscore + 1 < goName.Length)
                return goName.Substring(lastUnderscore + 1).Replace("(Clone)", "").Trim();

            return goName.Replace("(Clone)", "").Trim();
        }
    }
}
