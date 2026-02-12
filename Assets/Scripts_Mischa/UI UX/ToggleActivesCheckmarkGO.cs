using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleActivatesCheckmarkGO : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject checkmarkGO;

    private void Reset()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Awake()
    {
        if (!toggle) toggle = GetComponent<Toggle>();
        if (!checkmarkGO)
        {
            Debug.LogError("[ToggleActivatesCheckmarkGO] checkmarkGO fehlt.");
            return;
        }

        // Initial
        checkmarkGO.SetActive(toggle.isOn);

        // WICHTIG: KEIN RemoveAllListeners!
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        checkmarkGO.SetActive(isOn);
    }
}
