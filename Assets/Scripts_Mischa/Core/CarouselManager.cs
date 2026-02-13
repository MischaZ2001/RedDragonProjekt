using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselManager : MonoBehaviour
{
    [Header("Content Roots")]
    [SerializeField] private RectTransform carouselContentDark;     // e.g. Dark Canvas -> Content
    [SerializeField] private RectTransform carouselContentWhite;    // optional

    [Header("Runtime Prefab Loading (Resources)")]
    [SerializeField] private bool loadFromResources = true;
    [SerializeField] private string darkResourcesPath = "Dark";
    [SerializeField] private string whiteResourcesPath = "White";

    [Header("Fallback Prefabs (only used if loadFromResources = false)")]
    [SerializeField] private GameObject darkPrefabFallback;
    [SerializeField] private GameObject whitePrefabFallback;

    [Header("Instantiation")]
    [Min(1)][SerializeField] private int itemCount = 7;

    [Header("Optional: Namen vergeben")]
    [SerializeField] private string baseItemName = "Panel";
    [SerializeField] private bool useNameAsDescription = false;

    [Header("UI")]
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
    [Range(0.1f, 2f)] public float midScale = 0.9f;
    [Range(0.1f, 2f)] public float farScale = 0.7f;

    [Header("Depth Order (rendering)")]
    public bool depthOrderEnabled = true;
    public bool hoverInFrontOfDepth = true;

    [Header("Visibility")]
    public int visibleSlots = 5;

    [Header("Start")]
    [SerializeField] private int startCenterIndex = 0;

    // Built items
    private RectTransform[] itemsDark;
    private RectTransform[] itemsWhite;

    // State
    private int focusedIndex = -1;
    private int hoveredIndex = -1;
    private int currentCenterIndex = 0;
    private float wheelTimer = 0f;

    // Slot/depth info shared for both sets (same indices)
    private readonly Dictionary<int, int> itemSlotMap = new Dictionary<int, int>();
    private readonly List<int> visibleIndices = new List<int>();

    // Loaded prefab pools
    private GameObject[] darkPool;
    private GameObject[] whitePool;

    // FILTER: null = no filter, otherwise allowed indices
    private HashSet<int> filterAllowed;

    void Start()
    {
        if (!carouselContentDark)
        {
            Debug.LogError("CarouselManager: carouselContentDark fehlt.");
            return;
        }

        if (visibleSlots < 1 || visibleSlots % 2 == 0)
        {
            Debug.LogError("CarouselManager: visibleSlots muss ungerade und >= 1 sein.");
            return;
        }

        // Load pools
        if (loadFromResources)
        {
            LoadPoolsFromResources();
        }
        else
        {
            darkPool = darkPrefabFallback ? new[] { darkPrefabFallback } : null;
            whitePool = whitePrefabFallback ? new[] { whitePrefabFallback } : null;
        }

        if (darkPool == null || darkPool.Length == 0)
        {
            Debug.LogError($"CarouselManager: Keine Dark Prefabs gefunden (Resources/{darkResourcesPath}) und kein Fallback gesetzt.");
            return;
        }

        // Build: RANDOM PANELS PER ITEM (no pairing)
        itemsDark = BuildItemsIntoRandom(carouselContentDark, darkPool, "DARK");

        if (carouselContentWhite != null)
        {
            if (whitePool == null || whitePool.Length == 0)
            {
                Debug.LogWarning($"CarouselManager: carouselContentWhite ist gesetzt, aber keine White Prefabs gefunden (Resources/{whiteResourcesPath}) und kein Fallback. White wird übersprungen.");
                itemsWhite = null;
            }
            else
            {
                itemsWhite = BuildItemsIntoRandom(carouselContentWhite, whitePool, "WHITE");
            }
        }
        else
        {
            itemsWhite = null;
        }

        focusedIndex = -1;
        currentCenterIndex = Mathf.Clamp(startCenterIndex, 0, itemsDark.Length - 1);

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
            if (depthOrderEnabled) ApplyDepthOrder();
            ApplyScales();
        }

        HandleMouseWheel();
    }

    // ---------- PUBLIC ----------
    public void SetFocus(int index)
    {
        focusedIndex = Mathf.Clamp(index, 0, itemsDark.Length - 1);

        // Focused item must be center
        SnapToCenter(focusedIndex);

        if (depthOrderEnabled) ApplyDepthOrder();
        SetFocusedOnTop();

        ApplyScales();
        ApplyDescription();
    }

    public void ClearFocus()
    {
        focusedIndex = -1;

        SnapToCenter(currentCenterIndex);

        if (depthOrderEnabled) ApplyDepthOrder();

        ApplyScales();
        ApplyDescription();
    }

    public void RefreshLayout()
    {
        if (focusedIndex >= 0) SnapToCenter(focusedIndex);
        else SnapToCenter(currentCenterIndex);

        if (depthOrderEnabled) ApplyDepthOrder();
        SetFocusedOnTop();

        ApplyScales();
        ApplyDescription();
    }

    /// <summary>
    /// Filter: null/empty => no filter. Otherwise only these indices are used for scrolling/slots.
    /// </summary>
    public void SetFilterAllowed(HashSet<int> allowed)
    {
        filterAllowed = (allowed == null || allowed.Count == 0) ? null : allowed;

        focusedIndex = -1;

        // center auf ein gültiges item setzen
        currentCenterIndex = FindNextAllowed(currentCenterIndex, +1);

        SnapToCenter(currentCenterIndex);
        ApplyScales();
        ApplyDescription();
    }

    // ---------- RESOURCES ----------
    private void LoadPoolsFromResources()
    {
        darkPool = Resources.LoadAll<GameObject>(darkResourcesPath);
        whitePool = Resources.LoadAll<GameObject>(whiteResourcesPath);
    }

    // ---------- BUILD (RANDOM PER ITEM) ----------
    private RectTransform[] BuildItemsIntoRandom(RectTransform parent, GameObject[] pool, string label)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);

        var arr = new RectTransform[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            if (pool == null || pool.Length == 0)
            {
                Debug.LogError($"CarouselManager: Pool leer für {label}");
                return arr;
            }

            var prefab = pool[UnityEngine.Random.Range(0, pool.Length)];
            if (!prefab)
            {
                Debug.LogError($"CarouselManager: NULL prefab im Pool ({label}).");
                continue;
            }

            var go = Instantiate(prefab, parent, false);

            var rt = go.GetComponent<RectTransform>();
            if (!rt)
            {
                Debug.LogError($"CarouselManager: Prefab '{prefab.name}' hat keinen RectTransform (UI Prefab erwartet).");
                Destroy(go);
                continue;
            }

            ForceCenteredRect(rt);

            // Wichtig: SiblingIndex == i (damit Filter-Indizes stabil sind)
            go.transform.SetSiblingIndex(i);

            go.name = $"{baseItemName}_{i:00}_{label}_{prefab.name}";

            if (useNameAsDescription)
            {
                var ci = go.GetComponent<CarouselItem>();
                if (ci) ci.description = DescriptionFromName(prefab.name);
            }

            int idx = i;
            var proxy = go.GetComponent<CarouselClickProxy>();
            if (!proxy) proxy = go.AddComponent<CarouselClickProxy>();
            proxy.Init(this, idx);

            arr[i] = rt;
        }

        WireClearButtonsInItems(arr);
        return arr;
    }

    private static void ForceCenteredRect(RectTransform rt)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    private static string CleanName(string n)
    {
        return (n ?? string.Empty).Replace("(Clone)", "").Trim();
    }

    private string DescriptionFromName(string rawName)
    {
        return CleanName(rawName).Replace("_", " ");
    }

    private void WireClearButtonsInItems(RectTransform[] arr)
    {
        foreach (var item in arr)
        {
            if (!item) continue;

            var buttons = item.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b.transform == item) continue;
                b.onClick.RemoveListener(ClearFocus);
                b.onClick.AddListener(ClearFocus);
            }
        }
    }

    // ---------- LAYOUT ----------
    private void SnapToCenter(int centerIndex)
    {
        currentCenterIndex = Mod(centerIndex, itemsDark.Length);

        int n = itemsDark.Length;
        int half = visibleSlots / 2;

        SetAllActive(itemsDark, false);
        if (itemsWhite != null) SetAllActive(itemsWhite, false);

        itemSlotMap.Clear();
        visibleIndices.Clear();

        // Wenn gefiltert: center muss erlaubt sein
        int centerAllowed = FindNextAllowed(currentCenterIndex, +1);
        currentCenterIndex = centerAllowed;

        var used = new HashSet<int>();

        for (int slot = -half; slot <= half; slot++)
        {
            int itemIndex = GetSlotIndex(centerAllowed, slot, used);

            ActivateAndPlace(itemsDark, itemIndex, slot);
            if (itemsWhite != null) ActivateAndPlace(itemsWhite, itemIndex, slot);

            itemSlotMap[itemIndex] = slot;
            visibleIndices.Add(itemIndex);
        }

        if (depthOrderEnabled) ApplyDepthOrder();
        SetFocusedOnTop();
    }

    // slot -> index (respects filter, tries to avoid duplicates)
    private int GetSlotIndex(int centerAllowed, int slot, HashSet<int> used)
    {
        int n = itemsDark.Length;

        if (filterAllowed == null)
        {
            int index = Mod(centerAllowed + slot, n);
            used.Add(index);
            return index;
        }

        if (slot == 0)
        {
            used.Add(centerAllowed);
            return centerAllowed;
        }

        int steps = Mathf.Abs(slot);
        int dir = slot > 0 ? +1 : -1;

        int indexFiltered = centerAllowed;

        for (int s = 0; s < steps; s++)
            indexFiltered = FindNextAllowed(indexFiltered + dir, dir);

        // Schutz vor Duplikaten
        int safety = 0;
        while (used.Contains(indexFiltered) && safety < n)
        {
            indexFiltered = FindNextAllowed(indexFiltered + dir, dir);
            safety++;
        }

        used.Add(indexFiltered);
        return indexFiltered;
    }


    private void ActivateAndPlace(RectTransform[] arr, int itemIndex, int slot)
    {
        if (arr[itemIndex] == null) return;
        arr[itemIndex].gameObject.SetActive(true);
        arr[itemIndex].anchoredPosition = new Vector2(centerX + slot * spacing, yPosition);
    }

    private void SetAllActive(RectTransform[] arr, bool active)
    {
        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null)
                arr[i].gameObject.SetActive(active);
    }

    // ---------- FILTER HELPERS ----------
    private int FindNextAllowed(int start, int dir)
    {
        int n = itemsDark.Length;
        int idx = Mod(start, n);

        if (filterAllowed == null) return idx;

        for (int i = 0; i < n; i++)
        {
            if (filterAllowed.Contains(idx))
                return idx;

            idx = Mod(idx + dir, n);
        }

        // falls allowed doch "leer" ist oder keinen gültigen index enthält
        return Mod(start, n);
    }

    // ---------- SCALE ----------
    private void ApplyScales()
    {
        ApplyScalesFor(itemsDark);
        if (itemsWhite != null) ApplyScalesFor(itemsWhite);
    }

    private void ApplyScalesFor(RectTransform[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == null || !arr[i].gameObject.activeSelf) continue;

            float scale = baseScale;

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
                if (dist == 0) scale = baseScale;
                else if (dist == 1) scale = midScale;
                else scale = farScale;
            }

            arr[i].localScale = Vector3.one * scale;
        }
    }

    // ---------- DEPTH ORDER ----------
    private void ApplyDepthOrder()
    {
        if (visibleIndices.Count == 0) return;

        visibleIndices.Sort((a, b) =>
        {
            int da = itemSlotMap.TryGetValue(a, out int sa) ? Mathf.Abs(sa) : 999;
            int db = itemSlotMap.TryGetValue(b, out int sb) ? Mathf.Abs(sb) : 999;

            int cmp = db.CompareTo(da); // dist DESC
            if (cmp != 0) return cmp;

            int sa2 = itemSlotMap[a];
            int sb2 = itemSlotMap[b];
            return sa2.CompareTo(sb2);
        });

        ApplyDepthOrderFor(itemsDark);
        if (itemsWhite != null) ApplyDepthOrderFor(itemsWhite);
    }

    private void ApplyDepthOrderFor(RectTransform[] arr)
    {
        for (int i = 0; i < visibleIndices.Count; i++)
        {
            int idx = visibleIndices[i];
            if (arr[idx] != null)
                arr[idx].SetSiblingIndex(i);
        }

        if (hoverInFrontOfDepth && hoveredIndex >= 0 && hoveredIndex < arr.Length &&
            arr[hoveredIndex] != null && arr[hoveredIndex].gameObject.activeSelf)
        {
            arr[hoveredIndex].SetAsLastSibling();
        }

        if (focusedIndex >= 0 && focusedIndex < arr.Length &&
            arr[focusedIndex] != null && arr[focusedIndex].gameObject.activeSelf)
        {
            arr[focusedIndex].SetAsLastSibling();
        }
    }

    private void SetFocusedOnTop()
    {
        if (focusedIndex < 0) return;

        if (itemsDark != null && focusedIndex < itemsDark.Length &&
            itemsDark[focusedIndex] != null && itemsDark[focusedIndex].gameObject.activeSelf)
        {
            itemsDark[focusedIndex].SetAsLastSibling();
        }

        if (itemsWhite != null && focusedIndex < itemsWhite.Length &&
            itemsWhite[focusedIndex] != null && itemsWhite[focusedIndex].gameObject.activeSelf)
        {
            itemsWhite[focusedIndex].SetAsLastSibling();
        }
    }

    // ---------- UI ----------
    private void ApplyDescription()
    {
        if (!descriptionText) return;

        descriptionText.text = focusedIndex < 0
            ? ""
            : (itemsDark[focusedIndex] != null
                ? itemsDark[focusedIndex].GetComponent<CarouselItem>()?.description ?? ""
                : "");
    }

    private int GetHoveredIndex()
    {
        Vector2 mouse = Input.mousePosition;

        for (int i = itemsDark.Length - 1; i >= 0; i--)
        {
            if (itemsDark[i] == null || !itemsDark[i].gameObject.activeSelf) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(itemsDark[i], mouse, null))
                return i;
        }

        return -1;
    }

    // ---------- INPUT ----------
    private void HandleMouseWheel()
    {
        if (!enableMouseWheel) return;
        if (disableScrollWhileFocused && focusedIndex >= 0) return;

        wheelTimer -= Time.unscaledDeltaTime;
        if (wheelTimer > 0f) return;

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        int dir = scroll > 0 ? -1 : 1;

        // IMPORTANT: if filtered, jump to next allowed
        currentCenterIndex = FindNextAllowed(currentCenterIndex + dir, dir);

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
