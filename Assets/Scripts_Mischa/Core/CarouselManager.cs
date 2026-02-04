using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    [Header("Prefab Setup")]
    [SerializeField] private RectTransform carouselContent;
    [SerializeField] private RectTransform itemPrefab;
    [Min(1)][SerializeField] private int itemCount = 7;

    [Header("Optional: Namen vergeben")]
    [SerializeField] private string baseItemName = "Panel";
    [SerializeField] private bool useNameAsDescription = false;

    [Header("Carousel Items (runtime)")]
    public Text descriptionText;

    [Header("Layout")]
    public float spacing = 200f;
    public float yPosition = 0f;
    public float centerX = 0f;

    [Header("Input")]
    public bool enableMouseWheel = true;
    public float wheelCooldown = 0.12f;

    [Header("Focus behaviour")]
    public bool disableScrollWhileFocused = true;

    [Header("Scale")]
    public float baseScale = 0.9f;
    public float hoverScale = 1.05f;
    public float focusedScale = 1.4f;

    [Header("Depth Scaling (outer items smaller)")]
    [Range(0.1f, 2f)] public float midScale = 0.9f; // neighbors
    [Range(0.1f, 2f)] public float farScale = 0.7f; // outer

    [Header("Depth Order (rendering)")]
    public bool depthOrderEnabled = true;
    public bool hoverInFrontOfDepth = true;

    [Header("Visibility")]
    public int visibleSlots = 5;

    [Header("Start")]
    [SerializeField] private int startCenterIndex = 0;

    private RectTransform[] items;
    private int focusedIndex = -1;
    private int hoveredIndex = -1;
    private int currentCenterIndex = 0;
    private float wheelTimer = 0f;

    // Map: itemIndex -> current slot (-half..+half)
    private readonly Dictionary<int, int> itemSlotMap = new Dictionary<int, int>();
    private readonly List<int> visibleIndices = new List<int>();

    void Start()
    {
        if (!carouselContent)
        {
            Debug.LogError("CarouselManager: carouselContent fehlt.");
            return;
        }

        if (!itemPrefab)
        {
            Debug.LogError("CarouselManager: itemPrefab fehlt.");
            return;
        }

        if (itemCount < 1)
        {
            Debug.LogError("CarouselManager: itemCount muss >= 1 sein.");
            return;
        }

        if (visibleSlots < 1 || visibleSlots % 2 == 0)
        {
            Debug.LogError("CarouselManager: visibleSlots muss ungerade und >= 1 sein.");
            return;
        }

        BuildItemsFromPrefab();

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

            if (depthOrderEnabled)
                ApplyDepthOrder();

            ApplyScales();
        }

        HandleMouseWheel();
    }

    // ---------- PUBLIC ----------
    public void SetFocus(int index)
    {
        focusedIndex = Mathf.Clamp(index, 0, items.Length - 1);

        // Force focused item to be the center
        SnapToCenter(focusedIndex);

        if (depthOrderEnabled)
            ApplyDepthOrder();

        // Focus always on top
        if (focusedIndex >= 0 && focusedIndex < items.Length)
            items[focusedIndex].SetAsLastSibling();

        ApplyScales();
        ApplyDescription();
    }

    public void ClearFocus()
    {
        focusedIndex = -1;

        SnapToCenter(currentCenterIndex);

        if (depthOrderEnabled)
            ApplyDepthOrder();

        ApplyScales();
        ApplyDescription();
    }

    // ---------- BUILD ----------
    private void BuildItemsFromPrefab()
    {
        for (int i = carouselContent.childCount - 1; i >= 0; i--)
            Destroy(carouselContent.GetChild(i).gameObject);

        items = new RectTransform[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            RectTransform instance = Instantiate(itemPrefab);
            instance.SetParent(carouselContent, false);
            ForceCenteredRect(instance);

            instance.name = $"{baseItemName}_{i:00}";

            if (useNameAsDescription)
            {
                var ci = instance.GetComponent<CarouselItem>();
                if (ci) ci.description = DescriptionFromName(instance.name);
            }

            int idx = i;
            var proxy = instance.GetComponent<CarouselClickProxy>();
            if (!proxy) proxy = instance.gameObject.AddComponent<CarouselClickProxy>();
            proxy.Init(this, idx);

            items[i] = instance;
        }

        WireClearButtonsInItems();
    }

    private static void ForceCenteredRect(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        // DO NOT touch size
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    private string DescriptionFromName(string rawName)
    {
        string n = rawName.Replace("(Clone)", "").Trim();

        int underscore = n.LastIndexOf('_');
        if (underscore >= 0)
        {
            string tail = n.Substring(underscore + 1);
            if (int.TryParse(tail, out _))
                n = n.Substring(0, underscore);
        }

        return n.Replace("_", " ");
    }

    // ---------- ORIGINAL LOGIC ----------
    private void WireClearButtonsInItems()
    {
        foreach (var item in items)
        {
            var buttons = item.GetComponentsInChildren<Button>(true);

            foreach (var b in buttons)
            {
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

        itemSlotMap.Clear();
        visibleIndices.Clear();

        for (int slot = -half; slot <= half; slot++)
        {
            int itemIndex = Mod(currentCenterIndex + slot, n);

            items[itemIndex].gameObject.SetActive(true);
            items[itemIndex].anchoredPosition = new Vector2(centerX + slot * spacing, yPosition);

            itemSlotMap[itemIndex] = slot;
            visibleIndices.Add(itemIndex);
        }

        if (depthOrderEnabled)
            ApplyDepthOrder();

        // Focus always on top if active
        if (focusedIndex >= 0 && focusedIndex < items.Length && items[focusedIndex].gameObject.activeSelf)
            items[focusedIndex].SetAsLastSibling();
    }

    private void ApplyScales()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].gameObject.activeSelf) continue;

            float scale = baseScale;

            // Focus > Hover > Depth scaling
            if (focusedIndex >= 0 && i == focusedIndex)
            {
                scale = focusedScale;
            }
            else if (i == hoveredIndex)
            {
                scale = hoverScale;
            }
            else if (itemSlotMap.TryGetValue(i, out int slot))
            {
                int dist = Mathf.Abs(slot);
                if (dist == 0)
                    scale = baseScale; // center if not focused
                else if (dist == 1)
                    scale = midScale;
                else
                    scale = farScale;
            }

            items[i].localScale = Vector3.one * scale;
        }
    }

    private void ApplyDepthOrder()
    {
        if (visibleIndices.Count == 0) return;

        visibleIndices.Sort((a, b) =>
        {
            int da = itemSlotMap.TryGetValue(a, out int sa) ? Mathf.Abs(sa) : 999;
            int db = itemSlotMap.TryGetValue(b, out int sb) ? Mathf.Abs(sb) : 999;

            // larger dist first => back
            int cmp = db.CompareTo(da);
            if (cmp != 0) return cmp;

            // stable tie-breaker
            int sa2 = itemSlotMap[a];
            int sb2 = itemSlotMap[b];
            return sa2.CompareTo(sb2);
        });

        for (int i = 0; i < visibleIndices.Count; i++)
            items[visibleIndices[i]].SetSiblingIndex(i);

        if (hoverInFrontOfDepth && hoveredIndex >= 0 && hoveredIndex < items.Length && items[hoveredIndex].gameObject.activeSelf)
            items[hoveredIndex].SetAsLastSibling();

        if (focusedIndex >= 0 && focusedIndex < items.Length && items[focusedIndex].gameObject.activeSelf)
            items[focusedIndex].SetAsLastSibling();
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

        // ✅ When something is focused/clicked -> keep it in center and disable scrolling
        if (disableScrollWhileFocused && focusedIndex >= 0)
            return;

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
