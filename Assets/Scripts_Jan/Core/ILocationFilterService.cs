using System.Collections.Generic;

namespace LocationFinder.Core.Domain
{
    public interface ILocationFilterService
    {
        IReadOnlyList<Location> Filter(
            IReadOnlyList<Location> all,
            string searchText,
            string category);
    }
}
