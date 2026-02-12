using UnityEngine;
using TMPro;

public class ThemeSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject appCanvas;
    [SerializeField] private GameObject appCanvasWith;
    [SerializeField] private TMP_Dropdown themeDropdown;

    private void Start()
    {
        themeDropdown.onValueChanged.AddListener(OnThemeChanged);
    }

    private void OnThemeChanged(int index)
    {
        if (index == 0) // Dark Mode
        {
            appCanvas.SetActive(true);
            appCanvasWith.SetActive(false);
        }
        else if (index == 1) // White Mode
        {
            appCanvas.SetActive(false);
            appCanvasWith.SetActive(true);
        }
    }
}
