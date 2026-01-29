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
                s.SetManager(manager);
        }
    }
}
