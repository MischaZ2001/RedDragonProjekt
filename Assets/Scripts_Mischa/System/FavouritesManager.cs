using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouritesScrollManager : MonoBehaviour
    {
        [Header("ScrollView Content Roots")]
        [SerializeField] private RectTransform favouritesContentDark;
        [SerializeField] private RectTransform favouritesContentWhite;

        [Header("Runtime Prefab Loading (Resources)")]
        [SerializeField] private bool loadFromResources = true;
        [SerializeField] private string darkResourcesPath = "Dark";
        [SerializeField] private string whiteResourcesPath = "White";

        [Header("Fallback Prefabs (only used if loadFromResources = false OR resources empty)")]
        [SerializeField] private GameObject favouriteItemPrefabDarkFallback;
        [SerializeField] private GameObject favouriteItemPrefabWhiteFallback;

        [Header("Optional Empty State")]
        [SerializeField] private GameObject emptyState;

        [Header("Entry Layout")]
        [SerializeField] private float entryHeight = 220f;

        [Header("Visual Scaling")]
        [Range(0.5f, 1.2f)]
        [SerializeField] private float visualScale = 0.85f;
        [SerializeField] private bool autoHeightFromPrefab = true;

        private readonly Dictionary<string, EntryPair> _entriesById = new();
        private readonly HashSet<string> _savedIds = new();

        private const string PlayerPrefsKey = "LF_FAV_IDS";

        private GameObject[] _darkPool;
        private GameObject[] _whitePool;

        private struct EntryPair
        {
            public GameObject Dark;
            public GameObject White;
        }

        private void Awake()
        {
            LoadSavedIds();
            LoadPools();
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

            var pair = new EntryPair
            {
                Dark = SpawnOne(data, favouritesContentDark, GetPrefabFromPool(_darkPool, favouriteItemPrefabDarkFallback, "DARK")),
                White = SpawnOne(data, favouritesContentWhite, GetPrefabFromPool(_whitePool, favouriteItemPrefabWhiteFallback, "WHITE"))
            };

            if (pair.Dark == null && pair.White == null)
            {
                Debug.LogError("[FavouritesScrollManager] Could not spawn any favourite entry (both contents/prefabs missing).");
                _savedIds.Remove(data.Id);
                SaveIds();
                UpdateEmptyState();
                return;
            }

            _entriesById[data.Id] = pair;
            UpdateEmptyState();
        }

        public void RemoveFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            if (_entriesById.TryGetValue(id, out var pair))
            {
                if (pair.Dark != null) Destroy(pair.Dark);
                if (pair.White != null) Destroy(pair.White);
            }

            _entriesById.Remove(id);

            if (_savedIds.Remove(id))
                SaveIds();

            UpdateEmptyState();
        }

        private GameObject SpawnOne(FavouriteData data, RectTransform parent, GameObject prefab)
        {
            if (parent == null) return null;
            if (prefab == null) return null;

            var go = Instantiate(prefab, parent, false);
            go.name = $"FAV_{data.Id}";

            ApplyLayout(go);

            var view = go.GetComponent<FavouriteEntryView>();
            if (view != null)
                view.Setup(data, this);

            return go;
        }

        private void LoadPools()
        {
            if (!loadFromResources)
            {
                _darkPool = null;
                _whitePool = null;
                return;
            }

            _darkPool = Resources.LoadAll<GameObject>(darkResourcesPath);
            _whitePool = Resources.LoadAll<GameObject>(whiteResourcesPath);

            if (_darkPool == null || _darkPool.Length == 0)
                Debug.LogWarning($"[FavouritesScrollManager] No dark prefabs found in Resources/{darkResourcesPath} (fallback used if set).");

            if (_whitePool == null || _whitePool.Length == 0)
                Debug.LogWarning($"[FavouritesScrollManager] No white prefabs found in Resources/{whiteResourcesPath} (fallback used if set).");
        }

        private GameObject GetPrefabFromPool(GameObject[] pool, GameObject fallback, string label)
        {
            if (pool != null && pool.Length > 0) return pool[0];
            if (fallback != null) return fallback;

            Debug.LogWarning($"[FavouritesScrollManager] No prefab available for {label}.");
            return null;
        }

        private void ApplyLayout(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (!rt) return;

            rt.localScale = Vector3.one;

            Transform visual = go.transform.Find("Visual");
            RectTransform visualRT = visual ? visual.GetComponent<RectTransform>() : null;

            if (visualRT != null)
                visualRT.localScale = Vector3.one * visualScale;

            var le = go.GetComponent<LayoutElement>();
            if (!le) le = go.AddComponent<LayoutElement>();

            float finalHeight = entryHeight;

            if (autoHeightFromPrefab && visualRT != null && visualRT.rect.height > 0.01f)
            {
                finalHeight = visualRT.rect.height * visualScale;
                finalHeight += 8f;
            }

            le.minHeight = finalHeight;
            le.preferredHeight = finalHeight;
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
