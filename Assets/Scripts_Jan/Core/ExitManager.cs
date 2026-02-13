using UnityEngine;

public class ExitManager : MonoBehaviour
{
    public void ExitApplication()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
