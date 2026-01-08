using UnityEngine;
using UnityEngine.EventSystems;

public class CarouselItem : MonoBehaviour, IPointerClickHandler
{
    [TextArea] public string description;

    [HideInInspector] public int index;
    [HideInInspector] public CarouselManager manager;

    public void OnPointerClick(PointerEventData eventData)
        => manager?.SetFocus(index);
}
