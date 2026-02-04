using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedDragon
{
    public class CarouselSearchFilter : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private TMP_InputField searchField;

        [Header("Carousel Root (contains PanelBluprintDark1..n)")]
        [SerializeField] private Transform carouselContentRoot;

        [Header("Optional: if your panels are under a parent like ImageCarousel -> Content")]
        [SerializeField] private bool includeInactiveOnScan = true;

        private readonly List<SearchablePanel> panels = new();

        private void Awake()
        {
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

        private void CachePanels()
        {
            panels.Clear();

            if (carouselContentRoot == null)
            {
                Debug.LogError("[CarouselSearchFilter] carouselContentRoot is not assigned.");
                return;
            }

            // Scan all children panels (PanelBluprintDark prefabs)
            var panelRoots = carouselContentRoot.GetComponentsInChildren<Transform>(includeInactiveOnScan);
            foreach (var t in panelRoots)
            {
                // We only want top-level panel objects under carouselContentRoot
                if (t.parent != carouselContentRoot) continue;

                var searchable = t.GetComponent<SearchablePanel>();
                if (searchable == null)
                    searchable = t.gameObject.AddComponent<SearchablePanel>(); // auto-add helper

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

            // Empty -> show all
            if (string.IsNullOrWhiteSpace(query))
            {
                foreach (var p in panels)
                    p.SetVisible(true);
                return;
            }

            query = query.ToLowerInvariant();

            foreach (var p in panels)
            {
                bool match = p.Contains(query);
                p.SetVisible(match);
            }
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

            // Combine all text once for fast searching
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

        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf == visible) return;
            gameObject.SetActive(visible);
        }
    }
}
