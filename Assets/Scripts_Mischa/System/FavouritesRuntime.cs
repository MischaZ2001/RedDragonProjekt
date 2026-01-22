using UnityEngine;
using LocationFinder.System;

[DefaultExecutionOrder(-100)]
public class FavouritesRuntime : MonoBehaviour
{
    public static FavouritesService Favs { get; private set; }

    private void Awake()
    {
        if (Favs != null) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
        Favs = new FavouritesService(new JsonLocationRepository());
    }
}

