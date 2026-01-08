using System.Collections.Generic;
using UnityEngine;
using LocationFinder.Core.Domain;

namespace LocationFinder.System
{
    /// <summary>
    /// Simple JSON repository that loads locations from Resources/locations.json
    /// </summary>
    public class JsonLocationRepository : ILocationRepository
    {
        private const string ResourcesFileName = "locations"; 
        private List<Location> _cache;

        public IReadOnlyList<Location> GetAll()
        {
            EnsureLoaded();
            return _cache;
        }

        private void EnsureLoaded()
        {
            if (_cache != null) return;

            TextAsset jsonAsset = Resources.Load<TextAsset>(ResourcesFileName);
            if (jsonAsset == null)
            {
                Debug.LogError($"[JsonLocationRepository] Could not find Resources/{ResourcesFileName}.json");
                _cache = new List<Location>();
                return;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<LocationsWrapper>(jsonAsset.text);
                _cache = wrapper?.items ?? new List<Location>();
            }
            catch(global::System.Exception e) 

            {
                Debug.LogError($"[JsonLocationRepository] Failed to parse JSON: {e.Message}");
                _cache = new List<Location>();
            }
        }

        [global::System.Serializable]
        private class LocationsWrapper
        {
            public List<Location> items;
        }
    }
}


