using TMPro;
using UnityEngine;
using LocationFinder.Core.Domain;

public class FavouriteItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text cityText;

    public void Setup(Location loc)
    {
        nameText.text = loc.Name;
        cityText.text = loc.City;
    }
}
