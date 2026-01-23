using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LocationFinder.Core.Domain;

namespace LocationFinder.UIUX.LocationList
{
    public class UnityLocationListView : MonoBehaviour, ILocationListView
    {
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Dropdown categoryDropdown;
        [SerializeField] private Transform listRoot;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private GameObject emptyState;

        private LocationListPresenter presenter;

        public string SearchText => searchField ? searchField.text : "";
        public string SelectedCategory =>
            categoryDropdown && categoryDropdown.options.Count > 0
                ? categoryDropdown.options[categoryDropdown.value].text
                : "All";

        public void SetPresenter(LocationListPresenter presenter) => this.presenter = presenter;

        private void Awake()
        {
            if (searchField) searchField.onValueChanged.AddListener(_ => presenter?.OnSearchChanged());
            if (categoryDropdown) categoryDropdown.onValueChanged.AddListener(_ => presenter?.OnCategoryChanged());
        }

        public void ShowLocations(IReadOnlyList<Location> list)
        {
            ClearList();
            if (emptyState) emptyState.SetActive(false);

            foreach (var loc in list)
            {
                var go = Instantiate(listItemPrefab, listRoot);

                var item = go.GetComponentInChildren<LocationListItemView>(true);
                if (!item)
                {
                    Debug.LogError("[UnityLocationListView] Prefab has no LocationListItemView (root/children): " + go.name);
                    continue;
                }

                item.Setup(loc);
            }
        }

        public void ShowEmptyState()
        {
            ClearList();
            if (emptyState) emptyState.SetActive(true);
        }

        public void showError(string msg)
        {
            Debug.LogError("LocationList Error: " + msg);
        }

        private void ClearList()
        {
            if (!listRoot) return;

            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
        }
    }
}
