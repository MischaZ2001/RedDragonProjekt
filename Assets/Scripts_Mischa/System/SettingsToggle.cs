using UnityEngine;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
}
