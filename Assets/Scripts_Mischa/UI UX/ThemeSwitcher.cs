using UnityEngine;
using TMPro;

public class ThemeSwitcher : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject appCanvas;      // Dark
    [SerializeField] private GameObject appCanvasWith;  // White

    [Header("Dropdowns (je Canvas eins)")]
    [SerializeField] private TMP_Dropdown dropdownDarkCanvas;
    [SerializeField] private TMP_Dropdown dropdownWhiteCanvas;

    private bool _isApplying;

    private void Start()
    {
        if (dropdownDarkCanvas != null)
            dropdownDarkCanvas.onValueChanged.AddListener(OnThemeChanged);

        if (dropdownWhiteCanvas != null)
            dropdownWhiteCanvas.onValueChanged.AddListener(OnThemeChanged);

        // Initial anwenden (nimmt den Wert vom vorhandenen Dropdown)
        int startValue = GetCurrentDropdownValue();
        ApplyTheme(startValue);
    }

    private int GetCurrentDropdownValue()
    {
        if (dropdownDarkCanvas != null && dropdownDarkCanvas.gameObject.activeInHierarchy)
            return dropdownDarkCanvas.value;

        if (dropdownWhiteCanvas != null && dropdownWhiteCanvas.gameObject.activeInHierarchy)
            return dropdownWhiteCanvas.value;

        // Fallback: Dark
        return 0;
    }

    private void OnThemeChanged(int index)
    {
        ApplyTheme(index);
    }

    private void ApplyTheme(int index)
    {
        if (_isApplying) return;
        _isApplying = true;

        // index: 0 = Dark, 1 = White (so muss dein Dropdown aufgebaut sein)
        bool whiteMode = (index == 1);

        appCanvas.SetActive(!whiteMode);
        appCanvasWith.SetActive(whiteMode);

        // Beide Dropdowns auf den gleichen Wert setzen, ohne erneut Events zu feuern
        if (dropdownDarkCanvas != null) dropdownDarkCanvas.SetValueWithoutNotify(index);
        if (dropdownWhiteCanvas != null) dropdownWhiteCanvas.SetValueWithoutNotify(index);

        _isApplying = false;
    }
}

