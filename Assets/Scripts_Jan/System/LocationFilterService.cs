using System.Collections.Generic;
using System.Linq;
using LocationFinder.Core.Domain;

namespace LocationFinder.System
{
    public class LocationFilterService : ILocationFilterService
    {
        public IReadOnlyList<Location> Filter(
            IReadOnlyList<Location> all,
            string searchText,
            string category)
        {
            IEnumerable<Location> result = all;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string s = searchText.ToLowerInvariant();
                result = result.Where(loc =>
                    loc.Name.ToLowerInvariant().Contains(s) ||
                    loc.City.ToLowerInvariant().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                string cat = category.ToLowerInvariant();
                result = result.Where(loc => loc.Category.ToLowerInvariant() == cat);
            }

            return result.ToList();
        }
    }
}

