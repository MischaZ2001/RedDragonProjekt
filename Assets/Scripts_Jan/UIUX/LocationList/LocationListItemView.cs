using LocationFinder.Core.Domain;
using LocationFinder.System;
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

        private Location _location;
        private bool _suppressCallback;

        private void Reset()
        {
            if (!favouriteToggle) favouriteToggle = GetComponentInChildren<Toggle>(true);
        }

        public void Setup(Location location)
        {
            _location = location;
            if (_location == null) return;

            if (nameText) nameText.text = _location.Name;
            if (cityText) cityText.text = _location.City;
            if (categoryText) categoryText.text = _location.Category;

            if (!favouriteToggle)
            {
                Debug.LogError("[LocationListItemView] favouriteToggle missing on prefab.");
                return;
            }

            favouriteToggle.onValueChanged.RemoveAllListeners();

            // Initialen Zustand setzen OHNE Event auszulösen
            _suppressCallback = true;
            favouriteToggle.isOn = FavouritesRuntime.Favs.IsFavourite(_location);
            _suppressCallback = false;

            favouriteToggle.onValueChanged.AddListener(OnFavouriteChanged);
        }

        private void OnFavouriteChanged(bool isOn)
        {
            if (_suppressCallback || _location == null) return;

            FavouritesRuntime.Favs.SetFavourite(_location, isOn);
            Debug.Log($"Favourite set: {_location.Name} ({_location.Id}) => {isOn}");
        }
    }
}
