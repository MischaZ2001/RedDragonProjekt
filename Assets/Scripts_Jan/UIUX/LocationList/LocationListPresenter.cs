using LocationFinder.Core.Domain;
using System.Collections.Generic;

namespace LocationFinder.UIUX.LocationList
{
    public class LocationListPresenter
    {
        private readonly ILocationListView _view;
        private readonly ILocationRepository _repo;
        private readonly ILocationFilterService _filter;

        private IReadOnlyList<Location> _all;

        public LocationListPresenter(
            ILocationListView view,
            ILocationRepository repo,
            ILocationFilterService filter)
        {
            _view = view;
            _repo = repo;
            _filter = filter;

            _view.SetPresenter(this);
        }

        public void Initialize()
        {
            _all = _repo.GetAll();

            if (_all.Count == 0)
            {
                _view.ShowEmptyState();
                return;
            }

            ApplyFilter();
        }

        public void OnSearchChanged() => ApplyFilter();
        public void OnCategoryChanged() => ApplyFilter();

        private void ApplyFilter()
        {
            var filtered = _filter.Filter(
                _all,
                _view.SearchText,
                _view.SelectedCategory);

            if (filtered.Count == 0)
                _view.ShowEmptyState();
            else
                _view.ShowLocations(filtered);
        }
    }
}

