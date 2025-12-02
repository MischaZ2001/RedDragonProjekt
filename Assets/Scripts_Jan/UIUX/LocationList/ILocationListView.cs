using System.Collections.Generic;
using LocationFinder.Core.Domain;

namespace LocationFinder.UIUX.LocationList
{
    public interface ILocationListView
    {
        string SearchText { get; }
        string SelectedCategory { get; }

        void ShowLocations(IReadOnlyList<Location> list);
        void ShowEmptyState();
        void showError(string msg);

        void SetPresenter(LocationListPresenter presenter);
    }
}


