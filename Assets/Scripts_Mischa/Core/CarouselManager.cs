using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    public Image[] images;
    public float bigScale = 1.4f;
    public float smallScale = 0.9f;

    /// <summary>Sets the initial focused image when the scene starts.</summary>
    void Start()
    {
        SetFocus(2);
    }

    /// <summary>Makes the selected image larger and all others smaller.</summary>
    public void SetFocus(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].transform.localScale = (i == index)
                ? Vector3.one * bigScale
                : Vector3.one * smallScale;
        }
    }
}
