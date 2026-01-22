using LocationFinder.Core.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using LocationFinder.System;

namespace LocationFinder.UIUX.LocationList
{
    public class LocationListItemView : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        [Header("Favourite")]
        [SerializeField] private Button starButton;
        [SerializeField] private Image starIcon;
        [SerializeField] private Sprite starOn;
        [SerializeField] private Sprite starOff;

        private Location _location;

        public void Setup(Location location)
        {
            _location = location;

            nameText.text = location.Name;
            cityText.text = location.City;
            categoryText.text = location.Category;

            RefreshStar();

            if (starButton != null)
            {
                starButton.onClick.RemoveAllListeners();
                starButton.onClick.AddListener(ToggleFavourite);
            }
        }

        private void ToggleFavourite()
        {
            if (_location == null) return;

            bool fav = FavouritesRuntime.Favs.ToggleFavourite(_location);
            ApplyStar(fav);
            Debug.Log("Toggled favourite for: " + _location.Id);

        }

        private void RefreshStar()
        {
            if (_location == null) return;

            bool fav = FavouritesRuntime.Favs.IsFavourite(_location);
            ApplyStar(fav);
        }

        private void ApplyStar(bool fav)
        {
            if (starIcon != null && starOn != null && starOff != null)
                starIcon.sprite = fav ? starOn : starOff;
        }
    }
}

