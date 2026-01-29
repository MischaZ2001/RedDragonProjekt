using System.Collections.Generic;
using UnityEngine;

namespace LocationFinder.UIUX.Favourites
{
    public class FavouritesScrollManager : MonoBehaviour
    {
        [Header("Favourites ScrollView Content (Viewport/Content)")]
        [SerializeField] private Transform favouritesContent;

        [Header("Prefab for favourite entry")]
        [SerializeField] private GameObject favouriteItemPrefab;

        [Header("Optional Empty State")]
        [SerializeField] private GameObject emptyState;

        [Header("Sizing (Option A)")]
        [Tooltip("Height of each favourite entry in the scroll view.")]
        [SerializeField] private float entryHeight = 140f;

        [Tooltip("Set true to stretch the entry to the full width of the Content.")]
        [SerializeField] private bool stretchToContentWidth = true;

        private readonly Dictionary<string, GameObject> _entriesById = new Dictionary<string, GameObject>();

        public void AddFavourite(FavouriteData data)
        {
            if (string.IsNullOrWhiteSpace(data.Id)) return;

            if (!favouritesContent)
            {
                Debug.LogError("[FavouritesScrollManager] favouritesContent is NULL. Assign Viewport/Content in inspector.");
                return;
            }

            if (!favouriteItemPrefab)
            {
                Debug.LogError("[FavouritesScrollManager] favouriteItemPrefab is NULL. Assign a prefab in inspector.");
                return;
            }

            if (_entriesById.ContainsKey(data.Id))
            {
                UpdateEmptyState();
                return;
            }

            var go = Instantiate(favouriteItemPrefab, favouritesContent);
            go.name = $"FAV_{data.Id}";

            ApplySizing(go);

            var view = go.GetComponent<FavouriteEntryView>();
            if (view != null)
            {
                view.Setup(data, this);
            }
            else
            {
                Debug.LogWarning("[FavouritesScrollManager] favouriteItemPrefab has no FavouriteEntryView. Entry still spawned.");
            }

            _entriesById[data.Id] = go;
            UpdateEmptyState();
        }

        public void RemoveFavourite(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;

            if (_entriesById.TryGetValue(id, out var go) && go != null)
            {
                Destroy(go);
            }

            _entriesById.Remove(id);
            UpdateEmptyState();
        }

        private void ApplySizing(GameObject go)
        {
            var rt = go.transform as RectTransform;
            var parentRt = favouritesContent as RectTransform;

            if (rt == null || parentRt == null)
            {
                return;
            }

            rt.localScale = Vector3.one;

            if (stretchToContentWidth)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);

                rt.offsetMin = new Vector2(0f, rt.offsetMin.y);
                rt.offsetMax = new Vector2(0f, rt.offsetMax.y);
            }

            // Fixed height
            if (entryHeight > 0f)
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, entryHeight);
        }

        private void UpdateEmptyState()
        {
            if (!emptyState) return;
            emptyState.SetActive(_entriesById.Count == 0);
        }
    }
}
