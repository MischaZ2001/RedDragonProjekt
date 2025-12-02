namespace LocationFinder.Core.Domain
{
    /// <summary>
    /// reine Datenklasse für Drehorte
    /// </summary>
    public class Location
    {
        public string Id { get; }
        public string Name { get; }
        public string Category { get; }
        public string City { get; }
        public string[] Tags { get; }

        public Location(string id, string name, string category, string city, string[] tags)
        {
            Id = id;
            Name = name;
            Category = category;
            City = city;
            Tags = tags ?? new string[0];
        }
    }
}
