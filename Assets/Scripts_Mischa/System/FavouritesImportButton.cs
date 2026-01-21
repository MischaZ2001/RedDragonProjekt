using UnityEngine;

public class FavouritesImportButton : MonoBehaviour
{
    [SerializeField] private string targetFolder = "Icons";

    void Awake()
    {
        FavouritesService.Load();
        FavouritesService.CreateFolder(targetFolder);
    }

    public void OnPickImages_OpenImportFolder()
    {
        FavouritesService.OpenImportFolder();
    }

    public void OnImportFromImportFolder()
    {
        int count = FavouritesService.ScanImportFolderAndAddTo(targetFolder, deleteFromImportAfter: false);
        Debug.Log($"Imported {count} images into '{targetFolder}'");
    }

}

