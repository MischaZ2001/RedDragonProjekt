using System;
using System.IO;
using UnityEngine;

public static class FavouritesService
{
    private static FavouritesData data;

    // JSON-Speicherort
    private static string JsonPath =>
        Path.Combine(Application.persistentDataPath, "favourites.json");

    // Ordner für importierte Bilder
    private static string ImagesDir =>
        Path.Combine(Application.persistentDataPath, "FavouritesImages");

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
                string json = File.ReadAllText(JsonPath);
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
            Debug.LogWarning($"FavouritesService.Load failed: {e.Message}");
            data = new FavouritesData();
        }

        Directory.CreateDirectory(ImagesDir);
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
            Debug.LogWarning($"FavouritesService.Save failed: {e.Message}");
        }
    }

    public static bool CreateFolder(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return false;

        if (Data.folders.Exists(f => EqualsIgnoreCase(f.folderName, folderName)))
            return false;

        Data.folders.Add(new FavouritesFolderData { folderName = folderName });
        Save();
        return true;
    }

    public static bool RenameFolder(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName)) return false;
        if (EqualsIgnoreCase(oldName, newName)) return false;

        if (Data.folders.Exists(f => EqualsIgnoreCase(f.folderName, newName)))
            return false;

        var folder = FindFolder(oldName);
        if (folder == null) return false;

        folder.folderName = newName;
        Save();
        return true;
    }

    public static bool DeleteFolder(string folderName)
    {
        int removed = Data.folders.RemoveAll(f => EqualsIgnoreCase(f.folderName, folderName));
        if (removed > 0) Save();
        return removed > 0;
    }

    public static bool TryGetFolder(string folderName, out FavouritesFolderData folder)
    {
        folder = FindFolder(folderName);
        return folder != null;
    }

    public static bool ImportAndAddImage(string folderName, string absoluteSourcePath, string displayName = null)
    {
        if (string.IsNullOrWhiteSpace(absoluteSourcePath) || !File.Exists(absoluteSourcePath))
            return false;

        var folder = FindFolder(folderName);
        if (folder == null) return false;

        Directory.CreateDirectory(ImagesDir);

        string ext = Path.GetExtension(absoluteSourcePath).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            return false;

        // Zielname eindeutig machen (damit keine Überschreibung)
        string safeBase = MakeSafeFileName(Path.GetFileNameWithoutExtension(absoluteSourcePath));
        string uniqueName = $"{safeBase}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}{ext}";
        string destFullPath = Path.Combine(ImagesDir, uniqueName);

        try
        {
            File.Copy(absoluteSourcePath, destFullPath, overwrite: false);

            string rel = Path.Combine("FavouritesImages", uniqueName).Replace("\\", "/");

            // Duplikate vermeiden (gleicher relativer Pfad)
            if (folder.images.Exists(i => string.Equals(i.relativePath, rel, StringComparison.OrdinalIgnoreCase)))
                return false;

            folder.images.Add(new FavouriteImageRef
            {
                relativePath = rel,
                displayName = string.IsNullOrWhiteSpace(displayName) ? safeBase : displayName
            });

            Save();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ImportAndAddImage failed: {e.Message}");
            return false;
        }
    }

    public static bool RemoveImage(string folderName, string relativePath, bool deleteFileToo = false)
    {
        var folder = FindFolder(folderName);
        if (folder == null) return false;

        int removed = folder.images.RemoveAll(i =>
            string.Equals(i.relativePath, relativePath, StringComparison.OrdinalIgnoreCase));

        if (removed <= 0) return false;

        if (deleteFileToo)
        {
            try
            {
                string full = ResolveToFullPath(relativePath);
                if (File.Exists(full)) File.Delete(full);
            }
            catch { /* bewusst still */ }
        }

        Save();
        return true;
    }

    public static Sprite LoadSprite(FavouriteImageRef img, float pixelsPerUnit = 100f)
    {
        if (img == null || string.IsNullOrWhiteSpace(img.relativePath))
            return null;

        string fullPath = ResolveToFullPath(img.relativePath);
        if (!File.Exists(fullPath)) return null;

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes)) return null;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LoadSprite failed: {e.Message}");
            return null;
        }
    }


    private static FavouritesFolderData FindFolder(string folderName)
    {
        return Data.folders.Find(f => EqualsIgnoreCase(f.folderName, folderName));
    }

    private static bool EqualsIgnoreCase(string a, string b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static string ResolveToFullPath(string relativePath)
    {
        // relativePath ist bewusst relativ zu persistentDataPath
        string rel = relativePath.Replace("\\", "/");
        return Path.Combine(Application.persistentDataPath, rel);
    }

    private static string MakeSafeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "image";
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim();
    }
}
