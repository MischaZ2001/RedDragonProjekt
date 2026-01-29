using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouriteEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        [Header("Optional Remove Button")]
        [SerializeField] private Button removeButton;

        private string _id;
        private FavouritesScrollManager _manager;

        public void Setup(FavouriteData data, FavouritesScrollManager manager)
        {
            _id = data.Id;
            _manager = manager;

            if (nameText) nameText.text = data.Name;
            if (cityText) cityText.text = data.City;
            if (categoryText) categoryText.text = data.Category;

            if (removeButton)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => _manager.RemoveFavourite(_id));
            }
        }
    }
}

