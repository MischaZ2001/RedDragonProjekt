using UnityEngine;
using UnityEngine.EventSystems;

public class CarouselClickProxy : MonoBehaviour, IPointerClickHandler
{
    private CarouselManager manager;
    private int index;

    public void Init(CarouselManager manager, int index)
    {
        this.manager = manager;
        this.index = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
            manager.SetFocus(index);
    }
}
