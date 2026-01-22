using System;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    [Header("Carousel Items")]
    public RectTransform[] items;
    public Text descriptionText;

    [Header("Layout")]
    public float spacing = 200f;
    public float yPosition = 0f;
    public float centerX = 0f;

    [Header("Input")]
    public bool enableMouseWheel = true;
    public float wheelCooldown = 0.12f;

    [Header("Scale")]
    public float baseScale = 0.9f;
    public float hoverScale = 1.05f;
    public float focusedScale = 1.4f;

    [Header("Visibility")]
    public int visibleSlots = 5;

    [Header("Start")]
    [SerializeField] private int startCenterIndex = 0;

    private int focusedIndex = -1;       // -1 = nichts highlighted
    private int hoveredIndex = -1;
    private int currentCenterIndex = 0;
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
            Debug.LogError("CarouselManager: visibleSlots muss ungerade und >= 1 sein.");
            return;
        }

        Array.Sort(items, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

        // Klick-Handler ohne Button
        for (int i = 0; i < items.Length; i++)
        {
            int index = i;
            var proxy = items[i].GetComponent<CarouselClickProxy>();
            if (!proxy) proxy = items[i].gameObject.AddComponent<CarouselClickProxy>();
            proxy.Init(this, index);
        }

        // 🔥 NUR Buttons innerhalb der Carousel-Items verbinden
        WireClearButtonsInItems();

        focusedIndex = -1;
        currentCenterIndex = Mathf.Clamp(startCenterIndex, 0, items.Length - 1);

        SnapToCenter(currentCenterIndex);
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
        SnapToCenter(focusedIndex);
        ApplyScales();
        ApplyDescription();
    }

    public void ClearFocus()
    {
        focusedIndex = -1;
        SnapToCenter(currentCenterIndex);
        ApplyScales();
        ApplyDescription();
    }

    private void WireClearButtonsInItems()
    {
        foreach (var item in items)
        {
            // true = auch inaktiven Children
            var buttons = item.GetComponentsInChildren<Button>(true);

            foreach (var b in buttons)
            {
                // Sicherheitscheck: NICHT das Root-Item selbst "buttonisieren"
                if (b.transform == item) continue;

                b.onClick.RemoveListener(ClearFocus);
                b.onClick.AddListener(ClearFocus);
            }
        }
    }

    private void SnapToCenter(int centerIndex)
    {
        currentCenterIndex = Mod(centerIndex, items.Length);

        int n = items.Length;
        int half = visibleSlots / 2;

        for (int i = 0; i < n; i++)
            items[i].gameObject.SetActive(false);

        for (int slot = -half; slot <= half; slot++)
        {
            int itemIndex = Mod(currentCenterIndex + slot, n);
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

        descriptionText.text = focusedIndex < 0
            ? ""
            : items[focusedIndex].GetComponent<CarouselItem>()?.description ?? "";
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

        currentCenterIndex = Mod(currentCenterIndex + dir, items.Length);
        SnapToCenter(currentCenterIndex);
        ApplyScales();

        wheelTimer = wheelCooldown;
    }

    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
}
