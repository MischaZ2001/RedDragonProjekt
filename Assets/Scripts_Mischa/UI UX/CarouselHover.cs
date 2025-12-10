using UnityEngine;
using UnityEngine.EventSystems;

public class CarouselHover : MonoBehaviour, IPointerEnterHandler
{
    public int myIndex;
    public CarouselManager manager; // <- assign in Inspector

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.SetFocus(myIndex);
        }
        else
        {
            Debug.LogError("[CarouselHover] Manager is not assigned on " + gameObject.name);
        }
    }
}
