using System.Collections.Generic;
using LocationFinder.Core.Domain;

namespace LocationFinder.System
{
    public interface IFavouritesService
    {
        bool IsFavourite(Location loc);
        bool ToggleFavourite(Location loc);
        void SetFavourite(Location loc, bool isFav);
    }

    public class InMemoryFavouritesService : IFavouritesService
    {
        private readonly HashSet<string> _favIds = new HashSet<string>();

        public bool IsFavourite(Location loc)
        {
            if (loc == null || string.IsNullOrEmpty(loc.Id)) return false;
            return _favIds.Contains(loc.Id);
        }

        public bool ToggleFavourite(Location loc)
        {
            if (loc == null || string.IsNullOrEmpty(loc.Id)) return false;

            if (_favIds.Contains(loc.Id))
            {
                _favIds.Remove(loc.Id);
                return false;
            }

            _favIds.Add(loc.Id);
            return true;
        }

        public void SetFavourite(Location loc, bool isFav)
        {
            if (loc == null || string.IsNullOrEmpty(loc.Id)) return;

            if (isFav) _favIds.Add(loc.Id);
            else _favIds.Remove(loc.Id);
        }
    }

    
    /// <summary>
    /// Globale Runtime-Instanz (damit Favs nie null sind).
    /// </summary>
    public static class FavouritesRuntime
    {
        private static IFavouritesService _favs;

        public static IFavouritesService Favs
        {
            get
            {
                if (_favs == null) _favs = new InMemoryFavouritesService();
                return _favs;
            }
        }
    }
}
