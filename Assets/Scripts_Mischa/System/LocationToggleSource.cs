using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouriteToggleSource : MonoBehaviour
    {
        [Header("ID (muss eindeutig sein)")]
        [SerializeField] private string locationId;

        [Header("UI References (vom Carousel-Panel)")]
        [SerializeField] private Toggle favouriteToggle;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        [Header("Manager (in der Scene einmal vorhanden)")]
        [SerializeField] private FavouritesScrollManager favouritesManager;

        private bool _suppress;
        public void SetManager(FavouritesScrollManager m) => favouritesManager = m;


        private void Reset()
        {
            if (!favouriteToggle) favouriteToggle = GetComponentInChildren<Toggle>(true);
            if (!nameText || !cityText || !categoryText)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                // optional: falls du nicht zuweisen willst, aber besser im Inspector setzen
            }
        }

        private void Awake()
        {
            if (!favouriteToggle)
            {
                Debug.LogError($"[FavouriteToggleSource] Toggle fehlt auf {gameObject.name}");
                return;
            }

            favouriteToggle.onValueChanged.RemoveAllListeners();
            favouriteToggle.onValueChanged.AddListener(OnToggleChanged);

            // Startzustand: aus (oder lass es so wie im Inspector)
            // _suppress = true; favouriteToggle.isOn = false; _suppress = false;
        }

        private void OnToggleChanged(bool isOn)
        {
            if (_suppress) return;

            if (string.IsNullOrEmpty(locationId))
            {
                Debug.LogError($"[FavouriteToggleSource] locationId fehlt auf {gameObject.name}");
                return;
            }

            if (!favouritesManager)
            {
                Debug.LogError($"[FavouriteToggleSource] favouritesManager nicht gesetzt auf {gameObject.name}");
                return;
            }

            var data = new FavouriteData(
                locationId,
                nameText ? nameText.text : "",
                cityText ? cityText.text : "",
                categoryText ? categoryText.text : ""
            );

            if (isOn) favouritesManager.AddFavourite(data);
            else favouritesManager.RemoveFavourite(locationId);
        }

        // optional: damit der Fav-Eintrag in der ScrollView auch "ent-favouriten" kann
        public void ForceSetToggle(bool on)
        {
            if (!favouriteToggle) return;
            _suppress = true;
            favouriteToggle.isOn = on;
            _suppress = false;
        }
    }

    public readonly struct FavouriteData
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string City;
        public readonly string Category;

        public FavouriteData(string id, string name, string city, string category)
        {
            Id = id;
            Name = name;
            City = city;
            Category = category;
        }
    }
}

