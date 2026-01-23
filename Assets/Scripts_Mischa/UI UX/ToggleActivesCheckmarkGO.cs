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
            Debug.LogError("[ToggleActivatesCheckmarkGO] checkmarkGO nicht zugewiesen. Zieh Background/Checkmark rein.");
            return;
        }

        // Initialzustand: Checkmark an/aus passend zum Toggle
        checkmarkGO.SetActive(toggle.isOn);

        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        // GENAU dein Wunsch:
        // Klick -> Toggle isOn wechselt -> Checkmark GameObject an/aus
        checkmarkGO.SetActive(isOn);
    }
}
