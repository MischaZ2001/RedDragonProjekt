namespace LocationFinder.UIUX.Favourites
{
    public readonly struct FavouriteData
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string City;
        public readonly string Category;

        public FavouriteData(string id, string name, string city, string category)
        {
            Id = id;
            Name = name;
            City = city;
            Category = category;
        }
    }
}
