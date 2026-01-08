using LocationFinder.UIUX.LocationList;
using LocationFinder.Core.Domain;
using LocationFinder.System;
using UnityEngine;

namespace LocationFinder.UIUX
{
    public class LocationListBootstrap : MonoBehaviour
    {
        [SerializeField] private UnityLocationListView view;

        private void Start()
        {
            ILocationRepository repo = new JsonLocationRepository(); 
            ILocationFilterService filter = new LocationFilterService();

            var presenter = new LocationListPresenter(view, repo, filter);
            presenter.Initialize();
        }
    }
}
