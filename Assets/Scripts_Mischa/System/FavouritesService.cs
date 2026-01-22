using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using LocationFinder.Core.Domain;

namespace LocationFinder.System
{
    [Serializable]
    public class FavouritesData
    {
        public List<string> favouriteLocationIds = new();
        public List<FavouritesFolder> folders = new();
    }

    [Serializable]
    public class FavouritesFolder
    {
        public string name;
        public List<string> locationIds = new();
    }

    /// <summary>
    /// Manages favourites + folders and persists them to:
    /// Application.persistentDataPath/favourites.json
    ///
    /// Uses Location.Id as identifier (no manual keys needed).
    /// Fires FavouritesChanged event after any state change so UI can refresh.
    /// </summary>
    public class FavouritesService
    {
        public event Action FavouritesChanged;

        private readonly ILocationRepository _repo;
        private FavouritesData _data;

        private readonly string _filePath =
            Path.Combine(Application.persistentDataPath, "favourites.json");

        public FavouritesService(ILocationRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            Load();
        }

        // -------------------------
        // Load / Save
        // -------------------------
        public void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    _data = JsonUtility.FromJson<FavouritesData>(json) ?? new FavouritesData();
                }
                else
                {
                    _data = new FavouritesData();
                    SaveInternal(); // create file
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FavouritesService] Load failed: {e.Message}");
                _data = new FavouritesData();
            }

            // UI initial refresh (optional but handy)
            FavouritesChanged?.Invoke();
        }

        public void Save()
        {
            SaveInternal();
            FavouritesChanged?.Invoke();
        }

        private void SaveInternal()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.WriteAllText(_filePath, JsonUtility.ToJson(_data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FavouritesService] Save failed: {e.Message}");
            }
        }

        // -------------------------
        // Favourites (⭐)
        // -------------------------
        public bool IsFavourite(Location location)
        {
            if (location == null) return false;
            return _data.favouriteLocationIds.Contains(location.Id);
        }

        public bool IsFavourite(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId)) return false;
            return _data.favouriteLocationIds.Contains(locationId);
        }

        /// <summary>
        /// Toggle favourite status. If unfavourited, optionally removes from all folders.
        /// Returns: true if now favourite, false if removed.
        /// </summary>
        public bool ToggleFavourite(Location location, bool removeFromFoldersWhenUnfavourited = true)
        {
            if (location == null) return false;
            return ToggleFavourite(location.Id, removeFromFoldersWhenUnfavourited);
        }

        public bool ToggleFavourite(string locationId, bool removeFromFoldersWhenUnfavourited = true)
        {
            if (string.IsNullOrWhiteSpace(locationId)) return false;

            bool nowFav;

            if (_data.favouriteLocationIds.Contains(locationId))
            {
                _data.favouriteLocationIds.Remove(locationId);

                if (removeFromFoldersWhenUnfavourited)
                {
                    for (int i = 0; i < _data.folders.Count; i++)
                        _data.folders[i].locationIds.RemoveAll(x => x == locationId);
                }

                nowFav = false;
            }
            else
            {
                _data.favouriteLocationIds.Add(locationId);
                nowFav = true;
            }

            SaveInternal();
            FavouritesChanged?.Invoke();
            return nowFav;
        }

        public IReadOnlyList<string> GetFavouriteIds()
            => _data.favouriteLocationIds;

        /// <summary>
        /// Resolves current favourites to Location objects using the repository.
        /// </summary>
        public IReadOnlyList<Location> GetFavouriteLocations()
        {
            var all = _repo.GetAll();
            var set = new HashSet<string>(_data.favouriteLocationIds);
            return all.Where(l => set.Contains(l.Id)).ToList();
        }

        // -------------------------
        // Folders (in-app virtual folders)
        // -------------------------
        public IReadOnlyList<string> GetFolderNames()
            => _data.folders.Select(f => f.name).ToList();

        public bool CreateFolder(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return false;

            if (_data.folders.Any(f => string.Equals(f.name, folderName, StringComparison.OrdinalIgnoreCase)))
                return false;

            _data.folders.Add(new FavouritesFolder { name = folderName });
            SaveInternal();
            FavouritesChanged?.Invoke();
            return true;
        }

        public bool RenameFolder(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return false;

            if (_data.folders.Any(f => string.Equals(f.name, newName, StringComparison.OrdinalIgnoreCase)))
                return false;

            var folder = FindFolder(oldName);
            if (folder == null) return false;

            folder.name = newName;
            SaveInternal();
            FavouritesChanged?.Invoke();
            return true;
        }

        public bool DeleteFolder(string folderName)
        {
            int removed = _data.folders.RemoveAll(f => string.Equals(f.name, folderName, StringComparison.OrdinalIgnoreCase));
            if (removed <= 0) return false;

            SaveInternal();
            FavouritesChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Adds a location to a folder and ensures it's also in favourites.
        /// </summary>
        public bool AddLocationToFolder(string folderName, Location location)
        {
            if (location == null) return false;
            return AddLocationToFolder(folderName, location.Id);
        }

        public bool AddLocationToFolder(string folderName, string locationId)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(locationId)) return false;

            var folder = FindFolder(folderName);
            if (folder == null) return false;

            // ensure favourite
            if (!_data.favouriteLocationIds.Contains(locationId))
                _data.favouriteLocationIds.Add(locationId);

            if (folder.locationIds.Contains(locationId))
            {
                // already inside folder, still save favourite enforcement
                SaveInternal();
                FavouritesChanged?.Invoke();
                return false;
            }

            folder.locationIds.Add(locationId);
            SaveInternal();
            FavouritesChanged?.Invoke();
            return true;
        }

        public bool RemoveLocationFromFolder(string folderName, Location location)
        {
            if (location == null) return false;
            return RemoveLocationFromFolder(folderName, location.Id);
        }

        public bool RemoveLocationFromFolder(string folderName, string locationId)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(locationId)) return false;

            var folder = FindFolder(folderName);
            if (folder == null) return false;

            int removed = folder.locationIds.RemoveAll(x => x == locationId);
            if (removed <= 0) return false;

            SaveInternal();
            FavouritesChanged?.Invoke();
            return true;
        }

        public IReadOnlyList<string> GetFolderLocationIds(string folderName)
        {
            var folder = FindFolder(folderName);
            return folder?.locationIds ?? new List<string>();
        }

        public IReadOnlyList<Location> GetFolderLocations(string folderName)
        {
            var folder = FindFolder(folderName);
            if (folder == null) return new List<Location>();

            var all = _repo.GetAll();
            var set = new HashSet<string>(folder.locationIds);
            return all.Where(l => set.Contains(l.Id)).ToList();
        }

        private FavouritesFolder FindFolder(string folderName)
        {
            return _data.folders.FirstOrDefault(f =>
                string.Equals(f.name, folderName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
