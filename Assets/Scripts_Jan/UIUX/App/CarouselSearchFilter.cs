using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedDragon
{
    public class CarouselSearchFilter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField searchField;

        [Header("Carousel Manager (required)")]
        [SerializeField] private CarouselManager carouselManager;

        [Header("Carousel Root (the Content that holds the instantiated panels)")]
        [SerializeField] private Transform carouselContentRoot;

        private readonly List<SearchablePanel> panels = new();

        private void Start()
        {
            // Manager baut in Start() -> wir cachen danach
            CachePanels();

            if (searchField != null)
                searchField.onValueChanged.AddListener(HandleSearchChanged);

            ApplyFilter(searchField != null ? searchField.text : string.Empty);
        }

        private void OnDestroy()
        {
            if (searchField != null)
                searchField.onValueChanged.RemoveListener(HandleSearchChanged);
        }

        /// <summary>
        /// Falls du sp‰ter Panels neu baust, kannst du das von auﬂen callen.
        /// </summary>
        public void RebuildCacheNow()
        {
            CachePanels();
            ApplyFilter(searchField != null ? searchField.text : string.Empty);
        }

        private void CachePanels()
        {
            panels.Clear();

            if (carouselManager == null)
            {
                Debug.LogError("[CarouselSearchFilter] carouselManager is not assigned.");
                return;
            }

            if (carouselContentRoot == null)
            {
                Debug.LogError("[CarouselSearchFilter] carouselContentRoot is not assigned.");
                return;
            }

            // WICHTIG: Reihenfolge = SiblingIndex (0..n-1), damit Indizes zum Manager passen
            for (int i = 0; i < carouselContentRoot.childCount; i++)
            {
                var child = carouselContentRoot.GetChild(i);
                if (child == null) continue;

                var searchable = child.GetComponent<SearchablePanel>();
                if (searchable == null)
                    searchable = child.gameObject.AddComponent<SearchablePanel>();

                searchable.BuildCache();
                panels.Add(searchable);
            }
        }

        private void HandleSearchChanged(string query)
        {
            ApplyFilter(query);
        }

        private void ApplyFilter(string query)
        {
            query = (query ?? string.Empty).Trim();

            // Empty -> remove filter
            if (string.IsNullOrWhiteSpace(query))
            {
                carouselManager.SetFilterAllowed(null);
                return;
            }

            query = query.ToLowerInvariant();

            var allowed = new HashSet<int>();

            // index i == child sibling index == manager item index
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i] != null && panels[i].Contains(query))
                    allowed.Add(i);
            }

            carouselManager.SetFilterAllowed(allowed);
        }
    }

    /// <summary>
    /// Helper on each panel that caches its TMP texts and can search quickly.
    /// </summary>
    public class SearchablePanel : MonoBehaviour
    {
        [Tooltip("If empty, we auto-collect TMP texts from children.")]
        [SerializeField] private TMP_Text[] texts;

        private string cachedCombinedLower;

        public void BuildCache()
        {
            if (texts == null || texts.Length == 0)
                texts = GetComponentsInChildren<TMP_Text>(true);

            var combined = "";
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null) continue;
                combined += " " + texts[i].text;
            }

            cachedCombinedLower = combined.ToLowerInvariant();
        }

        public bool Contains(string queryLower)
        {
            if (string.IsNullOrEmpty(cachedCombinedLower))
                BuildCache();

            return cachedCombinedLower.Contains(queryLower);
        }
    }
}
