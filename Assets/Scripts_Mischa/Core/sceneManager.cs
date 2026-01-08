using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneLoader : MonoBehaviour
{
    public void LoadHomepage()
    {
        SceneManager.LoadScene("Homepage_Mischa1");
    }

    public void LoadLogin()
    {
        SceneManager.LoadScene("Login_Mischa1");
    }

    public void LoadSignIn()
    {
        SceneManager.LoadScene("SignIn_Mischa1");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Spiel beendet.");
    }
}
