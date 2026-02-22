using System;

namespace LocationFinder.UIUX.Favourites
{
    [Serializable]
    public struct FavouriteData
    {
        public string Id;
        public string Name;
        public string City;
        public string Category;

        public FavouriteData(string id, string name, string city, string category)
        {
            Id = id;
            Name = name;
            City = city;
            Category = category;
        }
    }
}
