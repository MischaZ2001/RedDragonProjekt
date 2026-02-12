using UnityEngine;

public class SettingsToggle : MonoBehaviour
{

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject settingsPanel2;

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        settingsPanel2.SetActive(!settingsPanel2.activeSelf);
    }
}
