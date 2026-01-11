using System;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    [Header("Items (beliebig viele)")]
    public RectTransform[] items;          // N UI Images
    public RectTransform centerPoint;
    public Text descriptionText;

    [Header("Slots")]
    public int visibleCount = 5;           // muss ungerade sein (5,7,9...)
    public float spacing = 200f;
    public float yOffset = 0f;

    [Header("Scale")]
    public float baseScale = 0.9f;
    public float hoverScale = 1.05f;
    public float focusedScale = 1.4f;

    [Header("Scroll")]
    public bool enableMouseWheel = true;
    public float wheelThreshold = 0.05f;

    private int focusedIndex = -1;
    private int hoveredIndex = -1;

    void Start()
    {
        if (items == null || items.Length < visibleCount)
        {
            Debug.LogError($"CarouselManager: items[] muss >= visibleCount sein. items={items?.Length}, visibleCount={visibleCount}");
            return;
        }
        if (visibleCount % 2 == 0)
        {
            Debug.LogError("CarouselManager: visibleCount muss ungerade sein (z.B. 5).");
            return;
        }

        // Links->Rechts sortieren (stabiler Start)
        Array.Sort(items, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

        // CarouselItem init + Button (optional)
        for (int i = 0; i < items.Length; i++)
        {
            var ci = items[i].GetComponent<CarouselItem>();
            if (!ci) ci = items[i].gameObject.AddComponent<CarouselItem>();
            ci.index = i;
            ci.manager = this;

            // Optional: Button-Highlight vermeiden (wenn du willst)
            var btn = items[i].GetComponent<Button>();
            if (!btn) btn = items[i].gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            int idx = i;
            btn.onClick.AddListener(() => SetFocus(idx));
        }

        // Start: kein Fokus -> alle klein, aber wir zeigen initial die ersten 5
        focusedIndex = 0;            // wichtig fürs “Fenster”
        SnapWindowToFocus();
        focusedIndex = -1;           // und dann Fokus deaktivieren
        ApplyScales();
        ApplyDescription();
    }

    void Update()
    {
        // Hover stabil
        int newHover = GetHoveredIndex();
        if (newHover != hoveredIndex)
        {
            hoveredIndex = newHover;
            ApplyScales();
        }

        // Scrollen
        if (enableMouseWheel)
        {
            float w = Input.mouseScrollDelta.y;
            if (Mathf.Abs(w) > wheelThreshold)
            {
                // Wheel up -> nach links / vorheriges
                // Wheel down -> nach rechts / nächstes
                int dir = w > 0 ? -1 : 1;

                if (focusedIndex == -1)
                {
                    // wenn noch kein Fokus: nimm das aktuelle Center des Fensters
                    focusedIndex = Mathf.Clamp(0 + (visibleCount / 2), 0, items.Length - 1);
                }

                SetFocus(ClampFocus(focusedIndex + dir));
            }
        }
    }

    public void SetFocus(int index)
    {
        focusedIndex = ClampFocus(index);
        SnapWindowToFocus();
        ApplyScales();
        ApplyDescription();
    }

    private int ClampFocus(int index)
        => Mathf.Clamp(index, 0, items.Length - 1);

    private void SnapWindowToFocus()
    {
        Vector2 c = centerPoint.anchoredPosition;
        c.y += yOffset;

        int half = visibleCount / 2;

        // erst alle ausblenden
        for (int i = 0; i < items.Length; i++)
            items[i].gameObject.SetActive(false);

        // dann die 5 sichtbaren platzieren
        for (int slot = -half; slot <= half; slot++)
        {
            int idx = focusedIndex + slot;
            if (idx < 0 || idx >= items.Length) continue; // an den Enden weniger als 5 sichtbar

            items[idx].gameObject.SetActive(true);
            items[idx].anchoredPosition = new Vector2(c.x + slot * spacing, c.y);
        }

        if (focusedIndex >= 0 && focusedIndex < items.Length)
            items[focusedIndex].SetAsLastSibling();
    }

    private void ApplyScales()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].gameObject.activeSelf) continue;

            float s = baseScale;
            if (i == focusedIndex) s = focusedScale;
            else if (i == hoveredIndex && i != focusedIndex) s = Mathf.Max(s, hoverScale);

            items[i].localScale = Vector3.one * s;
        }
    }

    private void ApplyDescription()
    {
        if (!descriptionText) return;

        if (focusedIndex < 0 || focusedIndex >= items.Length)
        {
            descriptionText.text = "";
            return;
        }

        var ci = items[focusedIndex].GetComponent<CarouselItem>();
        descriptionText.text = ci ? ci.description : "";
    }

    private int GetHoveredIndex()
    {
        Vector2 mouse = Input.mousePosition;

        // von oben nach unten prüfen (kein Flackern)
        for (int i = items.Length - 1; i >= 0; i--)
        {
            if (!items[i].gameObject.activeSelf) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(items[i], mouse))
                return i;
        }
        return -1;
    }
}
