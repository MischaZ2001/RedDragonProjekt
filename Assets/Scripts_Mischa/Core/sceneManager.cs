using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneLoader : MonoBehaviour
{
    public void LoadHomepage()
    {
        SceneManager.LoadScene("Homepage-Mischa2");
    }

    // TEMP: bis wir Login/SignIn als Panels haben
    public void LoadLogin()
    {
        SceneManager.LoadScene("SignIn_Mischa1");
    }

    public void LoadSignIn()
    {
        SceneManager.LoadScene("SignIn_Mischa1");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("Spiel beendet (Editor).");
#endif
    }
}
