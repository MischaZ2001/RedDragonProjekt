using System.Collections.Generic;

namespace LocationFinder.Core.Domain
{
    public interface ILocationRepository
    {
        IReadOnlyList<Location> GetAll();
    }
}

