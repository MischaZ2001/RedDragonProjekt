using System;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    [Header("Items")]
    public RectTransform[] items;          
    public RectTransform centerPoint;      
    public Text descriptionText;           

    [Header("Layout")]
    public float spacing = 200f;           
    public float yOffset = 0f;

    [Header("Scale")]
    public float baseScale = 0.9f;         
    public float hoverScale = 1.05f;       
    public float focusedScale = 1.4f;      

    private int focusedIndex = -1;         
    private int hoveredIndex = -1;

    void Start()
    {
        // Safety
        if (items == null || items.Length != 5)
        {
            Debug.LogError("CarouselManager: items[] MUSS genau 5 Elemente haben.");
            return;
        }

        Array.Sort(items, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

        for (int i = 0; i < items.Length; i++)
        {
            int index = i;

            var btn = items[i].GetComponent<Button>();
            if (btn == null)
                btn = items[i].gameObject.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SetFocus(index));

        }

        SnapInitialPositions();
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
    }
    public void SetFocus(int index)
    {
        focusedIndex = Mathf.Clamp(index, 0, 4);
        SnapToFocus();
        ApplyScales();
        ApplyDescription();
    }

    private void SnapInitialPositions()
    {
        Vector2 c = centerPoint.anchoredPosition;
        c.y += yOffset;

        //Slots: -2 -1 0 +1 +2
        for (int i = 0; i < 5; i++)
        {
            int slot = i - 2;
            items[i].anchoredPosition = new Vector2(c.x + slot * spacing, c.y);
        }
    }

    private void SnapToFocus()
    {
        Vector2 c = centerPoint.anchoredPosition;
        c.y += yOffset;

        for (int slot = -2; slot <= 2; slot++)
        {
            int itemIndex = Mod(focusedIndex + slot, 5);
            items[itemIndex].anchoredPosition =
                new Vector2(c.x + slot * spacing, c.y);

            if (itemIndex == focusedIndex)
                items[itemIndex].SetAsLastSibling();
        }
    }

    private void ApplyScales()
    {
        for (int i = 0; i < 5; i++)
        {
            float scale = baseScale;

            if (i == focusedIndex)
                scale = focusedScale;
            else if (i == hoveredIndex)
                scale = hoverScale;

            items[i].localScale = Vector3.one * scale;
        }
    }

    private void ApplyDescription()
    {
        if (!descriptionText) return;

        if (focusedIndex == -1)
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
            if (RectTransformUtility.RectangleContainsScreenPoint(items[i], mouse))
                return i;
        }
        return -1;
    }

    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
}
