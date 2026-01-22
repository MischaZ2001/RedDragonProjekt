using System;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    public RectTransform[] items;
    public Text descriptionText;

    public float spacing = 200f;
    public float yPosition = 0f;
    public float centerX = 0f;
    public float wheelCooldown = 0.12f;

    public bool enableMouseWheel = true;

    public float baseScale = 0.9f;
    public float hoverScale = 1.05f;
    public float focusedScale = 1.4f;

    public int visibleSlots = 5;

    private int focusedIndex = -1;
    private int hoveredIndex = -1;

    [SerializeField] private int startCenterIndex = 0;

    private float wheelTimer = 0f;

    void Start()
    {
        if (items == null || items.Length < 1)
        {
            Debug.LogError("CarouselManager: items[] ist leer.");
            return;
        }

        if (visibleSlots < 1 || visibleSlots % 2 == 0)
        {
            Debug.LogError("CarouselManager: visibleSlots muss ungerade und >= 1 sein (z.B. 5).");
            return;
        }

        if (items.Length < visibleSlots)
        {
            Debug.LogError($"CarouselManager: items.Length ({items.Length}) muss >= visibleSlots ({visibleSlots}) sein.");
            return;
        }

        Array.Sort(items, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

        for (int i = 0; i < items.Length; i++)
        {
            int index = i;

            var proxy = items[i].GetComponent<CarouselClickProxy>();
            if (!proxy) proxy = items[i].gameObject.AddComponent<CarouselClickProxy>();
            proxy.Init(this, index);
        }

        startCenterIndex = Mathf.Clamp(startCenterIndex, 0, items.Length - 1);

        SnapToCenter(startCenterIndex);
        ApplyScales();
        ApplyDescription(); 
    }

    void Update()
    {
        int newHover = GetHoveredIndex();
        if (newHover != hoveredIndex)
        {
            hoveredIndex = newHover;
            ApplyScales();
        }

        HandleMouseWheel();
    }

    public void SetFocus(int index)
    {
        focusedIndex = Mathf.Clamp(index, 0, items.Length - 1);
        SnapToFocus();
        ApplyScales();
        ApplyDescription();
    }

    private void SnapToFocus()
    {
        SnapToCenter(focusedIndex);
    }

    private void SnapToCenter(int centerIndex)
    {
        int n = items.Length;
        int half = visibleSlots / 2;

        for (int i = 0; i < n; i++)
            items[i].gameObject.SetActive(false);

        for (int slot = -half; slot <= half; slot++)
        {
            int itemIndex = Mod(centerIndex + slot, n);

            items[itemIndex].gameObject.SetActive(true);
            items[itemIndex].anchoredPosition = new Vector2(centerX + slot * spacing, yPosition);

            if (focusedIndex >= 0 && itemIndex == focusedIndex)
                items[itemIndex].SetAsLastSibling();
        }
    }

    private void ApplyScales()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].gameObject.activeSelf) continue;

            float scale = baseScale;

            if (focusedIndex >= 0 && i == focusedIndex)
                scale = focusedScale;
            else if (i == hoveredIndex)
                scale = hoverScale;

            items[i].localScale = Vector3.one * scale;
        }
    }

    private void ApplyDescription()
    {
        if (!descriptionText) return;

        if (focusedIndex < 0)
        {
            descriptionText.text = "";
            return;
        }

        var desc = items[focusedIndex].GetComponent<CarouselItem>();
        descriptionText.text = desc ? desc.description : "";
    }

    private int GetHoveredIndex()
    {
        Vector2 mouse = Input.mousePosition;

        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (!items[i].gameObject.activeSelf) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(items[i], mouse, null))
                return i;
        }
        return -1;
    }

    private void HandleMouseWheel()
    {
        if (!enableMouseWheel) return;

        wheelTimer -= Time.unscaledDeltaTime;
        if (wheelTimer > 0f) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        int dir = scroll > 0 ? -1 : 1;

        if (focusedIndex < 0)
            focusedIndex = startCenterIndex;

        int next = Mod(focusedIndex + dir, items.Length);
        SetFocus(next);

        wheelTimer = wheelCooldown;
    }

    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
}
