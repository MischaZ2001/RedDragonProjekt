using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouritesScrollManager : MonoBehaviour
    {
        [Header("ScrollView Content (Viewport/Content)")]
        [SerializeField] private RectTransform favouritesContent;

        [Header("Prefab for favourite entry (PROJECT prefab)")]
        [SerializeField] private GameObject favouriteItemPrefab;

        [Header("Optional Empty State")]
        [SerializeField] private GameObject emptyState;

        [Header("Entry Layout")]
        [SerializeField] private float entryHeight = 220f;

        [Header("Visual Scaling (Inspector)")]
        [Range(0.5f, 1.2f)]
        [SerializeField] private float visualScale = 0.85f;

        [SerializeField] private bool autoHeightFromPrefab = true;


        private readonly Dictionary<string, GameObject> _entriesById = new();
        private readonly HashSet<string> _savedIds = new();

        private const string PlayerPrefsKey = "LF_FAV_IDS";

        private void Awake()
        {
            LoadSavedIds();
            UpdateEmptyState();
        }

        public bool IsSavedFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return _savedIds.Contains(id);
        }

        public void AddFavourite(FavouriteData data)
        {
            if (string.IsNullOrWhiteSpace(data.Id)) return;

            if (_savedIds.Add(data.Id))
                SaveIds();

            if (_entriesById.ContainsKey(data.Id))
            {
                UpdateEmptyState();
                return;
            }

            if (!favouritesContent)
            {
                Debug.LogError("[FavouritesScrollManager] favouritesContent ist NULL. Assign Viewport/Content.");
                return;
            }

            if (!favouriteItemPrefab)
            {
                Debug.LogError("[FavouritesScrollManager] favouriteItemPrefab ist NULL. Assign a PROJECT prefab.");
                return;
            }

            var go = Instantiate(favouriteItemPrefab, favouritesContent);
            go.name = $"FAV_{data.Id}";

            ApplyLayout(go);

            var view = go.GetComponent<FavouriteEntryView>();
            if (view != null)
                view.Setup(data, this);

            _entriesById[data.Id] = go;
            UpdateEmptyState();
        }

        public void RemoveFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            if (_entriesById.TryGetValue(id, out var go) && go != null)
                Destroy(go);

            _entriesById.Remove(id);

            if (_savedIds.Remove(id))
                SaveIds();

            UpdateEmptyState();
        }

        private void ApplyLayout(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (!rt) return;

            // 🔒 ROOT niemals skalieren (Layout-Sicherheit)
            rt.localScale = Vector3.one;

            // 🔧 VISUAL skalieren (Optik)
            Transform visual = go.transform.Find("Visual");
            if (visual != null)
            {
                visual.localScale = Vector3.one * visualScale;
            }
            else
            {
                Debug.LogWarning("[FavouritesScrollManager] Child 'Visual' fehlt im Prefab.");
            }

            // 📐 Layout-Höhe korrekt setzen
            var le = go.GetComponent<LayoutElement>();
            if (!le) le = go.AddComponent<LayoutElement>();

            if (autoHeightFromPrefab)
            {
                // Originalhöhe * Scale
                float baseHeight = rt.rect.height;
                float finalHeight = baseHeight * visualScale;

                le.minHeight = finalHeight;
                le.preferredHeight = finalHeight;
            }
            else
            {
                // fallback: feste Höhe
                le.minHeight = entryHeight;
                le.preferredHeight = entryHeight;
            }

            le.flexibleHeight = 0f;
        }


        private void UpdateEmptyState()
        {
            if (!emptyState) return;
            emptyState.SetActive(_savedIds.Count == 0);
        }

        private void LoadSavedIds()
        {
            _savedIds.Clear();

            string raw = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (string.IsNullOrWhiteSpace(raw)) return;

            var parts = raw.Split('|');
            for (int i = 0; i < parts.Length; i++)
            {
                var id = parts[i];
                if (!string.IsNullOrWhiteSpace(id))
                    _savedIds.Add(id);
            }
        }

        private void SaveIds()
        {
            string raw = string.Join("|", _savedIds);
            PlayerPrefs.SetString(PlayerPrefsKey, raw);
            PlayerPrefs.Save();
        }
    }
}
