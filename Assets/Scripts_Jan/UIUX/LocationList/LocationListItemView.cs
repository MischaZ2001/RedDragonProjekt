using LocationFinder.Core.Domain;
using TMPro;
using UnityEngine;

namespace LocationFinder.UIUX.LocationList
{
    public class LocationListItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text cityText;
        [SerializeField] private TMP_Text categoryText;

        public void Setup(Location location)
        {
            nameText.text = location.Name;
            cityText.text = location.City;
            categoryText.text = location.Category;
        }
    }
}

