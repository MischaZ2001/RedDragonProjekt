using System;
using System.IO;
using UnityEngine;

public static class FavouritesService
{
    private static FavouritesData data;

    private static string JsonPath =>
        Path.Combine(Application.persistentDataPath, "favourites.json");

    public static FavouritesData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(JsonPath))
            {
                var json = File.ReadAllText(JsonPath);
                data = JsonUtility.FromJson<FavouritesData>(json) ?? new FavouritesData();
            }
            else
            {
                data = new FavouritesData();
                Save();
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"FavouritesService.Load failed: {e.Message}");
            data = new FavouritesData();
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
            File.WriteAllText(JsonPath, JsonUtility.ToJson(Data, true));
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"FavouritesService.Save failed: {e.Message}");
        }
    }

    public static void EnsureFolder(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return;

        if (!Data.folders.Exists(f => EqualsIgnoreCase(f.folderName, folderName)))
        {
            Data.folders.Add(new FavouritesFolderData { folderName = folderName });
            Save();
        }
    }

    public static bool IsFavourite(string folderName, string itemKey)
    {
        if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(itemKey))
            return false;

        var folder = FindFolder(folderName);
        if (folder == null) return false;

        return folder.itemKeys.Exists(k => EqualsIgnoreCase(k, itemKey));
    }

    /// <summary>
    /// Toggle: wenn drin -> raus, wenn nicht drin -> rein.
    /// returns: true = ist danach Favourite, false = ist danach nicht Favourite
    /// </summary>
    public static bool ToggleFavourite(string folderName, string itemKey)
    {
        if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(itemKey))
            return false;

        EnsureFolder(folderName);
        var folder = FindFolder(folderName);
        if (folder == null) return false;

        int idx = folder.itemKeys.FindIndex(k => EqualsIgnoreCase(k, itemKey));
        bool nowFav;

        if (idx >= 0)
        {
            folder.itemKeys.RemoveAt(idx);
            nowFav = false;
        }
        else
        {
            folder.itemKeys.Add(itemKey);
            nowFav = true;
        }

        Save();
        return nowFav;
    }

    private static FavouritesFolderData FindFolder(string folderName)
        => Data.folders.Find(f => EqualsIgnoreCase(f.folderName, folderName));

    private static bool EqualsIgnoreCase(string a, string b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
