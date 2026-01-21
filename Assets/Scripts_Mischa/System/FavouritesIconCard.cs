using UnityEngine;
using UnityEngine.UI;

public class FavouriteItemCard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string title;
    [SerializeField] private Sprite image;

    [Header("UI Refs")]
    [SerializeField] private Image imageUI;
    [SerializeField] private Text titleUI; // falls du TMP nutzt, sag Bescheid
    [SerializeField] private Button starButton;
    [SerializeField] private Image starIcon; // das Stern-Icon-Image

    [Header("Star Visuals")]
    [SerializeField] private Sprite starOff;
    [SerializeField] private Sprite starOn;

    [Header("Favourites Folder Name")]
    [SerializeField] private string folderName = "Favourites";

    private string ItemKey => gameObject.name.Replace("(Clone)", "").Trim();

    private void Awake()
    {
        if (imageUI) imageUI.sprite = image;
        if (titleUI) titleUI.text = title;

        if (starButton)
            starButton.onClick.AddListener(OnStarClicked);

        RefreshStar();
    }

    private void OnEnable()
    {
        // falls UI neu geöffnet wird
        RefreshStar();
    }

    private void OnStarClicked()
    {
        bool nowFav = FavouritesService.ToggleFavourite(folderName, ItemKey);
        ApplyStarVisual(nowFav);
    }

    private void RefreshStar()
    {
        bool fav = FavouritesService.IsFavourite(folderName, ItemKey);
        ApplyStarVisual(fav);
    }

    private void ApplyStarVisual(bool fav)
    {
        if (starIcon == null) return;

        if (starOn != null && starOff != null)
            starIcon.sprite = fav ? starOn : starOff;

        // Optional: wenn du keine Sprites hast, kannst du stattdessen Farbe ändern:
        // starIcon.color = fav ? Color.yellow : Color.white;
    }
}

