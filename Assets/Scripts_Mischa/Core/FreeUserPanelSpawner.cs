using UnityEngine;

public class FreeUserPanelSpawner : MonoBehaviour
{
    [Header("Target Canvas")]
    [SerializeField] private Canvas freeUserCanvas;

    [Header("Prefab")]
    [SerializeField] private RectTransform panelPrefab;

    [Header("Optional Parent")]
    [SerializeField] private RectTransform parentOverride; 

    private RectTransform spawned;

    void Start()
    {
        if (!freeUserCanvas || !panelPrefab)
        {
            Debug.LogError("FreeUserPanelSpawner: freeUserCanvas oder panelPrefab fehlt.");
            return;
        }

        RectTransform parent = parentOverride ? parentOverride : freeUserCanvas.transform as RectTransform;

        spawned = Instantiate(panelPrefab, parent);
        spawned.gameObject.name = panelPrefab.name + "_FreeUser";
        spawned.anchoredPosition = Vector2.zero; 
        spawned.localScale = Vector3.one;
    }
}
