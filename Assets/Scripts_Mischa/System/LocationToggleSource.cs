using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouriteToggleSource : MonoBehaviour
    {
        [Header("UI References (vom Carousel-Panel)")]
        [SerializeField] private Toggle favouriteToggle;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        private string _id;
        private FavouritesScrollManager _manager;
        private bool _suppress;
        private bool _wired;

        /// <summary>
        /// Wird vom CarouselManager gesetzt (Option B)
        /// </summary>
        public void Init(FavouritesScrollManager manager, string stableId)
        {
            _manager = manager;
            _id = stableId;

            WireIfNeeded();
            SyncFromManager();
        }

        private void Reset()
        {
            if (!favouriteToggle) favouriteToggle = GetComponentInChildren<Toggle>(true);
        }

        private void Awake()
        {
            WireIfNeeded();
        }

        private void WireIfNeeded()
        {
            if (_wired) return;

            if (!favouriteToggle)
                favouriteToggle = GetComponentInChildren<Toggle>(true);

            if (!favouriteToggle)
            {
                Debug.LogError($"[FavouriteToggleSource] Toggle fehlt auf {gameObject.name}");
                return;
            }

            favouriteToggle.onValueChanged.RemoveListener(OnToggleChanged);
            favouriteToggle.onValueChanged.AddListener(OnToggleChanged);
            _wired = true;
        }

        private void SyncFromManager()
        {
            if (!_wired || favouriteToggle == null) return;
            if (_manager == null) return;
            if (string.IsNullOrWhiteSpace(_id)) return;

            _suppress = true;
            favouriteToggle.isOn = _manager.IsSavedFavourite(_id);
            _suppress = false;
        }

        private void OnToggleChanged(bool isOn)
        {
            if (_suppress) return;

            if (_manager == null)
            {
                Debug.LogWarning($"[FavouriteToggleSource] Manager not injected yet on {gameObject.name}");
                return;
            }

            if (string.IsNullOrWhiteSpace(_id))
            {
                Debug.LogError($"[FavouriteToggleSource] Stable ID missing on {gameObject.name}");
                return;
            }

            var data = new FavouriteData(
                _id,
                nameText ? nameText.text : "",
                cityText ? cityText.text : "",
                categoryText ? categoryText.text : ""
            );

            if (isOn) _manager.AddFavourite(data);
            else _manager.RemoveFavourite(_id);
        }
    }
}
