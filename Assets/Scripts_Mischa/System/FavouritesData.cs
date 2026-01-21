using System;
using System.Collections.Generic;

[Serializable]
public class FavouritesData
{
    public List<FavouritesFolderData> folders = new();
}

[Serializable]
public class FavouritesFolderData
{
    public string folderName;
    public List<string> itemKeys = new(); // z.B. "SwordItem", "ShieldItem" (Prefab-Name)
}
