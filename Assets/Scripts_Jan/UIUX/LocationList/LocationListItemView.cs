using LocationFinder.UIUX.Favourites;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.LocationList
{
    public class LocationListItemView : MonoBehaviour
    {
        [Header("Text (vom Carousel-Panel)")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        [Header("Favourite (Toggle)")]
        [SerializeField] private Toggle favouriteToggle;

        [Header("Favourites Manager (1x in Scene)")]
        [SerializeField] private FavouritesScrollManager favouritesManager;

        [Header("Optional: Manual Id (leer lassen => gameObject.name)")]
        [SerializeField] private string manualId;

        private bool _suppress;

        private void Reset()
        {
            if (!favouriteToggle) favouriteToggle = GetComponentInChildren<Toggle>(true);
        }

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(manualId))
                manualId = gameObject.name;

            if (!favouriteToggle)
            {
                Debug.LogError($"[LocationListItemView] favouriteToggle fehlt: {gameObject.name}");
                return;
            }

            favouriteToggle.onValueChanged.AddListener(OnFavouriteChanged);
        }

        private void Start()
        {
            // Beim Start: gespeicherten Zustand anwenden
            if (!favouritesManager)
            {
                // Falls du den Manager nicht per Inspector zuweisen willst:
                favouritesManager = Object.FindAnyObjectByType<FavouritesScrollManager>();
            }

            if (!favouritesManager) return;

            bool shouldBeOn = favouritesManager.IsSavedFavourite(manualId);

            _suppress = true;
            favouriteToggle.SetIsOnWithoutNotify(shouldBeOn);
            _suppress = false;

            // Wenn fav gespeichert ist, sicherstellen, dass der Eintrag in der ScrollView existiert
            if (shouldBeOn)
            {
                var data = new FavouriteData(
                    manualId,
                    nameText ? nameText.text : "",
                    cityText ? cityText.text : "",
                    categoryText ? categoryText.text : ""
                );

                favouritesManager.AddFavourite(data);
            }
        }

        private void OnFavouriteChanged(bool isOn)
        {
            if (_suppress) return;

            if (!favouritesManager)
            {
                favouritesManager = Object.FindAnyObjectByType<FavouritesScrollManager>();
                if (!favouritesManager)
                {
                    Debug.LogError($"[LocationListItemView] favouritesManager NICHT gefunden: {gameObject.name}");
                    return;
                }
            }

            var data = new FavouriteData(
                manualId,
                nameText ? nameText.text : "",
                cityText ? cityText.text : "",
                categoryText ? categoryText.text : ""
            );

            Debug.Log($"[LocationListItemView] Toggle {manualId} => {isOn}");

            if (isOn) favouritesManager.AddFavourite(data);
            else favouritesManager.RemoveFavourite(manualId);
        }
    }
}
