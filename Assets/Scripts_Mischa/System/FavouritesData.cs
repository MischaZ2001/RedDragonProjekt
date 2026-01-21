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
    public List<FavouriteImageRef> images = new();
}

[Serializable]
public class FavouriteImageRef
{
    public string relativePath;   // z.B. "FavouritesImages/icon_123.png"
    public string displayName;    // optional für UI (Dateiname ohne Endung etc.)
}
