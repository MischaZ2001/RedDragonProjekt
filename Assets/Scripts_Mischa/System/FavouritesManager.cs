using System;
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

        [Header("Fallback Prefabs (used if Resources empty OR loadFromResources=false)")]
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

        private const string PlayerPrefsKey = "LF_FAV_JSON_V1";

        [Serializable]
        private class FavouriteDataList
        {
            public List<FavouriteData> Items = new List<FavouriteData>();
        }

        private FavouriteDataList _saved = new FavouriteDataList();

        private readonly Dictionary<string, EntryPair> _entriesById = new Dictionary<string, EntryPair>();

        private GameObject[] _darkPool;
        private GameObject[] _whitePool;

        private struct EntryPair
        {
            public GameObject Dark;
            public GameObject White;
        }

        private void Awake()
        {
            LoadPools();
            LoadSaved();          // lädt JSON -> _saved.Items
            RebuildUIFromSaved(); // spawnt UI Einträge
            UpdateEmptyState();
        }

        // -------------------------
        // Public API
        // -------------------------

        public bool IsSavedFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return _saved.Items.Exists(x => x.Id == id);
        }

        public int FavouriteCount => _saved.Items.Count;

        public void AddFavourite(FavouriteData data)
        {
            if (string.IsNullOrWhiteSpace(data.Id)) return;

            // 1) Speichern (wenn noch nicht vorhanden)
            if (!_saved.Items.Exists(x => x.Id == data.Id))
            {
                _saved.Items.Add(data);
                Save();
            }

            // 2) UI Eintrag existiert schon?
            if (_entriesById.ContainsKey(data.Id))
            {
                UpdateEmptyState();
                return;
            }

            // 3) Spawnen (Dark + White)
            var pair = new EntryPair
            {
                Dark = SpawnOne(data, favouritesContentDark, GetPrefabFromPool(_darkPool, favouriteItemPrefabDarkFallback, "DARK")),
                White = SpawnOne(data, favouritesContentWhite, GetPrefabFromPool(_whitePool, favouriteItemPrefabWhiteFallback, "WHITE"))
            };

            // 4) Safety: Wenn gar nichts spawnbar war -> rollback
            if (pair.Dark == null && pair.White == null)
            {
                Debug.LogError("[FavouritesScrollManager] Could not spawn any favourite entry (missing parents/prefabs). Rolling back save.");
                _saved.Items.RemoveAll(x => x.Id == data.Id);
                Save();
                UpdateEmptyState();
                return;
            }

            _entriesById[data.Id] = pair;
            UpdateEmptyState();
        }

        public void RemoveFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            // UI entfernen
            if (_entriesById.TryGetValue(id, out var pair))
            {
                if (pair.Dark != null) Destroy(pair.Dark);
                if (pair.White != null) Destroy(pair.White);
            }
            _entriesById.Remove(id);

            // Persistenz entfernen
            _saved.Items.RemoveAll(x => x.Id == id);
            Save();

            UpdateEmptyState();
        }

        public void ClearAllFavourites()
        {
            ClearSpawnedEntriesOnly();
            _saved.Items.Clear();
            Save();
            UpdateEmptyState();
        }

        // -------------------------
        // Spawning / Layout
        // -------------------------

        private GameObject SpawnOne(FavouriteData data, RectTransform parent, GameObject prefab)
        {
            if (parent == null || prefab == null) return null;

            var go = Instantiate(prefab, parent, false);
            go.name = $"FAV_{data.Id}";
            var rt = go.GetComponent<RectTransform>();
            if (rt)
            {
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                rt.anchoredPosition3D = Vector3.zero;
                rt.localPosition = Vector3.zero;
            }

            ApplyLayout(go);

            var view = go.GetComponent<FavouriteEntryView>();
            if (view != null)
                view.Setup(data, this);

            return go;
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
            finalHeight = Mathf.Clamp(finalHeight, 80f, 180f);

            if (autoHeightFromPrefab && visualRT != null && visualRT.rect.height > 0.01f)
            {
                finalHeight = visualRT.rect.height * visualScale + 8f;
            }

            le.minHeight = finalHeight;
            le.preferredHeight = finalHeight;
            le.flexibleHeight = 0f;
        }

        // -------------------------
        // Resources / Prefab Pools
        // -------------------------

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

        // -------------------------
        // Persistence (JSON)
        // -------------------------

        private void LoadSaved()
        {
            _saved = new FavouriteDataList();

            string json = PlayerPrefs.GetString(PlayerPrefsKey, "");
            if (string.IsNullOrWhiteSpace(json)) return;

            try
            {
                var loaded = JsonUtility.FromJson<FavouriteDataList>(json);
                _saved = loaded ?? new FavouriteDataList();
                if (_saved.Items == null) _saved.Items = new List<FavouriteData>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FavouritesScrollManager] Failed to load favourites JSON. Resetting. {e.Message}");
                _saved = new FavouriteDataList();
            }

            CleanupSavedList();
        }

        private void Save()
        {
            CleanupSavedList();
            string json = JsonUtility.ToJson(_saved);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        private void CleanupSavedList()
        {
            if (_saved?.Items == null)
            {
                _saved = new FavouriteDataList();
                return;
            }

            _saved.Items.RemoveAll(x => string.IsNullOrWhiteSpace(x.Id));

            var seen = new HashSet<string>();
            for (int i = _saved.Items.Count - 1; i >= 0; i--)
            {
                if (!seen.Add(_saved.Items[i].Id))
                    _saved.Items.RemoveAt(i);
            }
        }

        // -------------------------
        // Rebuild / Empty State
        // -------------------------

        private void RebuildUIFromSaved()
        {
            ClearSpawnedEntriesOnly();

            foreach (var data in _saved.Items)
            {
                if (string.IsNullOrWhiteSpace(data.Id)) continue;

                var pair = new EntryPair
                {
                    Dark = SpawnOne(data, favouritesContentDark, GetPrefabFromPool(_darkPool, favouriteItemPrefabDarkFallback, "DARK")),
                    White = SpawnOne(data, favouritesContentWhite, GetPrefabFromPool(_whitePool, favouriteItemPrefabWhiteFallback, "WHITE"))
                };

                if (pair.Dark != null || pair.White != null)
                    _entriesById[data.Id] = pair;
            }
        }

        private void ClearSpawnedEntriesOnly()
        {
            foreach (var kv in _entriesById)
            {
                if (kv.Value.Dark) Destroy(kv.Value.Dark);
                if (kv.Value.White) Destroy(kv.Value.White);
            }
            _entriesById.Clear();
        }

        private void UpdateEmptyState()
        {
            if (!emptyState) return;
            emptyState.SetActive(_saved.Items.Count == 0);
        }
    }
}