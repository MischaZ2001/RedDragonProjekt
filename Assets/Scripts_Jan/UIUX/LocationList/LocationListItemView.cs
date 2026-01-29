using LocationFinder.UIUX.Favourites;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.LocationList
{
    public class LocationListItemView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        [Header("Favourite (Toggle)")]
        [SerializeField] private Toggle favouriteToggle;

        [Header("Favourites Target (1x in Scene)")]
        [SerializeField] private FavouritesScrollManager favouritesManager;

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


        private void OnFavouriteChanged(bool isOn)
        {
            if (_suppress) return;

            if (!favouritesManager)
            {
                Debug.LogError($"[LocationListItemView] favouritesManager NICHT gesetzt: {gameObject.name}");
                return;
            }

            if (string.IsNullOrWhiteSpace(manualId))
            {
                Debug.LogError($"[LocationListItemView] manualId ist leer: {gameObject.name}");
                return;
            }

            var data = new FavouriteData(
                manualId,
                nameText ? nameText.text : "",
                cityText ? cityText.text : "",
                categoryText ? categoryText.text : ""
            );

            Debug.Log($"[LocationListItemView] Fav toggle => {manualId} isOn={isOn}");

            if (isOn) favouritesManager.AddFavourite(data);
            else favouritesManager.RemoveFavourite(manualId);
        }
    }
}
