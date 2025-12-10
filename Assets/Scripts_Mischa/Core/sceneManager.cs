using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneLoader : MonoBehaviour
{
    public void LoadHomepage()
    {
        SceneManager.LoadScene("Homepage_Mischa");
    }

    public void LoadLogin()
    {
        SceneManager.LoadScene("Login_Mischa");
    }

    public void LoadSignIn()
    {
        SceneManager.LoadScene("SignIn_Mischa");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Spiel beendet.");
    }
}
