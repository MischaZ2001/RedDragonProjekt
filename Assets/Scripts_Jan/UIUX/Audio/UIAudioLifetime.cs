using UnityEngine;

public class UIaudioLifetime : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}