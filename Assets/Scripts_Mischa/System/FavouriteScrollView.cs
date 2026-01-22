using UnityEngine;
using LocationFinder.System;
using LocationFinder.Core.Domain;

public class FavouriteScrollView : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject favouriteItemPrefab;

    private void OnEnable()
    {
        if (FavouritesRuntime.Favs == null)
        {
            StartCoroutine(SubscribeNextFrame());
            return;
        }

        FavouritesRuntime.Favs.FavouritesChanged += Refresh;
        Refresh();
    }

    private System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null;

        if (FavouritesRuntime.Favs == null)
        {
            Debug.LogError("[FavouriteScrollView] FavouritesRuntime.Favs is null. " +
                           "Make sure there is an enabled GameObject with the FavouritesRuntime script in the scene.",
                           this);
            yield break;
        }

        FavouritesRuntime.Favs.FavouritesChanged += Refresh;
        Refresh();
    }


    private void OnDisable()
    {
        if (FavouritesRuntime.Favs != null)
            FavouritesRuntime.Favs.FavouritesChanged -= Refresh;
    }

    private void Refresh()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        var favourites = FavouritesRuntime.Favs.GetFavouriteLocations();

        foreach (var loc in favourites)
        {
            var go = Instantiate(favouriteItemPrefab, contentRoot);

            var view = go.GetComponent<FavouriteItemView>();
            if (view != null)
                view.Setup(loc);
            else
                Debug.LogError("[FavouriteScrollView] FavouriteItemPrefab is missing FavouriteItemView.", go);
        }
    }
}

