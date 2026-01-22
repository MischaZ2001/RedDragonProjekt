using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [Header("Sound")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Volumes")]
    [Range(0f, 1f)] public float hoverVolume = 0.6f;
    [Range(0f, 1f)] public float clickVolume = 1f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        UISoundManager.Instance?.PlayOneShot(hoverSound, hoverVolume);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;
        UISoundManager.Instance?.PlayOneShot(clickSound, clickVolume);
    }
}
