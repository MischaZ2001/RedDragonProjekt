using System.Collections.Generic;
using UnityEngine;
using LocationFinder.Core.Domain;

namespace LocationFinder.System
{
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

            string raw = jsonAsset.text ?? "";
            Debug.Log($"[JsonLocationRepository] RAW (first 300 chars): {raw.Substring(0, Mathf.Min(300, raw.Length))}");

            try
            {
                LocationsWrapper wrapper = null;

                wrapper = JsonUtility.FromJson<LocationsWrapper>(raw);

                if (wrapper == null || wrapper.items == null)
                {
                    string trimmed = raw.TrimStart();
                    if (trimmed.StartsWith("["))
                    {
                        string wrapped = "{ \"items\": " + raw + " }";
                        wrapper = JsonUtility.FromJson<LocationsWrapper>(wrapped);
                    }
                }

                var dtos = wrapper?.items ?? new List<LocationDto>();

                _cache = new List<Location>(dtos.Count);
                for (int i = 0; i < dtos.Count; i++)
                {
                    var d = dtos[i];
                    if (d == null) continue;

                    _cache.Add(new Location(
                        d.id,
                        d.name,
                        d.category,
                        d.city,
                        d.tags
                    ));
                }

                Debug.Log($"[JsonLocationRepository] Loaded locations: {_cache.Count}");

                if (_cache.Count > 0)
                    Debug.Log($"[JsonLocationRepository] First: {_cache[0].Name} ({_cache[0].Id})");
            }
            catch (global::System.Exception e)
            {
                Debug.LogError($"[JsonLocationRepository] Failed to parse JSON: {e.Message}");
                _cache = new List<Location>();
            }
        }

        [global::System.Serializable]
        private class LocationsWrapper
        {
            public List<LocationDto> items;
        }

        [global::System.Serializable]
        private class LocationDto
        {
            public string id;
            public string name;
            public string category;
            public string city;
            public string[] tags;
        }
    }
}
