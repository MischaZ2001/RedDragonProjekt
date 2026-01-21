using UnityEngine;

public class FavouritesGridBuilder : MonoBehaviour
{
    [SerializeField] private Transform gridParent; // z.B. Content in ScrollView
    [SerializeField] private string resourcesFolder = "FavouriteItems";

    private void Start()
    {
        FavouritesService.Load(); // wichtig

        Build();
    }

    public void Build()
    {
        // Clear
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        // Load all item prefabs from Resources/FavouriteItems
        var prefabs = Resources.LoadAll<GameObject>(resourcesFolder);

        foreach (var prefab in prefabs)
        {
            Instantiate(prefab, gridParent);
        }
    }
}

