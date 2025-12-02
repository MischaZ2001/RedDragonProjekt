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

        public string SearchText => searchField.text;
        public string SelectedCategory => categoryDropdown.options[categoryDropdown.value].text;

        public void SetPresenter(LocationListPresenter presenter)
        {
            this.presenter = presenter;
        }

        private void Awake()
        {
            searchField?.onValueChanged.AddListener(_ => presenter?.OnSearchChanged());
            categoryDropdown?.onValueChanged.AddListener(_ => presenter?.OnCategoryChanged());
        }

        public void ShowLocations(IReadOnlyList<Location> list)
        {
            ClearList();
            emptyState.SetActive(false);

            foreach (var loc in list)
            {
                var go = Instantiate(listItemPrefab, listRoot);
                var item = go.GetComponent<LocationListItemView>();
                item.Setup(loc);
            }
        }

        public void ShowEmptyState()
        {
            ClearList();
            emptyState.SetActive(true);
        }

        public void showError(string msg)
        {
            Debug.LogError("LocationList Error: " + msg);
        }

        private void ClearList()
        {
            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
        }
    }
}

