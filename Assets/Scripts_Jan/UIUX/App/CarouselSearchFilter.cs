using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedDragon
{
    public class CarouselSearchFilter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_InputField searchFieldWhite;

        [Header("Carousel Manager (required)")]
        [SerializeField] private CarouselManager carouselManager;

        [Header("Carousel Root (the Content that holds the instantiated panels)")]
        [SerializeField] private Transform carouselContentRoot;
        [SerializeField] private Transform carouselContentRootWhite;

        private readonly List<SearchablePanel> panels = new();

        private void Start()
        {
            if (searchField != null)
                searchField.onValueChanged.AddListener(HandleSearchChanged);

            if (searchFieldWhite != null)
                searchFieldWhite.onValueChanged.AddListener(HandleSearchChanged);

            StartCoroutine(DelayedInit());
        }

        private void OnDestroy()
        {
            if (searchField != null)
                searchField.onValueChanged.RemoveListener(HandleSearchChanged);

            if (searchFieldWhite != null)
                searchFieldWhite.onValueChanged.RemoveListener(HandleSearchChanged);
        }

        private System.Collections.IEnumerator DelayedInit()
        {
            // 1ñ2 Frames warten, damit der CarouselManager instantiieren kann
            yield return null;
            yield return null;

            CachePanels();

            // ein gemeinsames Query verwenden (beide Felder schreiben denselben Filter)
            string q = "";
            if (searchField != null && !string.IsNullOrWhiteSpace(searchField.text)) q = searchField.text;
            else if (searchFieldWhite != null) q = searchFieldWhite.text;

            ApplyFilter(q);
        }

        /// <summary>
        /// Falls du sp‰ter Panels neu baust, kannst du das von auﬂen callen.
        /// </summary>
        public void RebuildCacheNow()
        {
            CachePanels();

            string q = "";
            if (searchField != null && !string.IsNullOrWhiteSpace(searchField.text)) q = searchField.text;
            else if (searchFieldWhite != null) q = searchFieldWhite.text;

            ApplyFilter(q);
        }


        private void CachePanels()
        {
            panels.Clear();

            if (carouselContentRoot == null && carouselContentRootWhite == null)
            {
                Debug.LogError("[CarouselSearchFilter] No carousel roots assigned.");
                return;
            }

            CollectDirectChildren(carouselContentRoot);
            CollectDirectChildren(carouselContentRootWhite);

            // Optional: Sortieren (stabil)
            panels.Sort((a, b) => a.ItemIndex.CompareTo(b.ItemIndex));
        }

        private void CollectDirectChildren(Transform root)
        {
            if (root == null) return;

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child == null) continue;

                var p = child.GetComponent<SearchablePanel>();
                if (p == null) continue;

                if (p.ItemIndex < 0)
                {
                    Debug.LogWarning($"[CarouselSearchFilter] ItemIndex not set on {child.name}");
                    continue;
                }

                p.BuildCache();
                panels.Add(p);
            }
        }

        private void HandleSearchChanged(string query)
        {
            ApplyFilter(query);
        }

        private void ApplyFilter(string query)
        {
            if (carouselManager == null)
            {
                Debug.LogError("[CarouselSearchFilter] carouselManager is not assigned.");
                return;
            }

            query = (query ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(query))
            {
                carouselManager.SetFilterAllowed(null);
                return;
            }

            query = query.ToLowerInvariant();

            var allowed = new HashSet<int>();
            for (int i = 0; i < panels.Count; i++)
            {
                var p = panels[i];
                if (p != null && p.Contains(query))
                    allowed.Add(p.ItemIndex); // <- stabiler Index vom Manager
            }

            carouselManager.SetFilterAllowed(allowed);
        }
    }

    /// <summary>
    /// Helper on each panel that caches its TMP texts and can search quickly.
    /// </summary>
    public class SearchablePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] texts;

        [HideInInspector] public int ItemIndex = -1;

        private string cachedCombinedLower;

        public void BuildCache()
        {
            if (texts == null || texts.Length == 0)
                texts = GetComponentsInChildren<TMP_Text>(true); 

            var combined = "";
            for (int i = 0; i < texts.Length; i++)
            {
                var t = texts[i];
                if (t == null) continue;
                if (!t.enabled) continue;
                if (!t.gameObject.activeInHierarchy) continue;

                combined += " " + t.text;
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

