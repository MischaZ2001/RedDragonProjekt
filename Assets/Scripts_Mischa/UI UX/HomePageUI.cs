using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HomepageUI : MonoBehaviour
{
    [Header("UI Elemente")]
    public TMP_Text usernameText;

    public GameObject loginButton;
    public GameObject signInButton;
    public GameObject logoutButton;

    void Start()
    {
        Debug.Log("[HOMEPAGE] Start -> SessionData.CurrentUserName = " + SessionData.CurrentUserName);

        bool isLoggedIn = !string.IsNullOrEmpty(SessionData.CurrentUserName);

        if (isLoggedIn)
        {
            // Benutzer eingeloggt → alle Login/SignIn Buttons verstecken
            usernameText.text = SessionData.CurrentUserName;

            loginButton.SetActive(false);
            signInButton.SetActive(false);
            logoutButton.SetActive(true);
        }
        else
        {
            // Kein Benutzer eingeloggt → alle Login/SignIn Buttons anzeigen
            usernameText.text = "";

            loginButton.SetActive(true);
            signInButton.SetActive(true);
            logoutButton.SetActive(false);
        }
    }

    public void OnLogoutClicked()
    {
        Debug.Log("[HOMEPAGE] Logout geklickt");

        // Session löschen
        SessionData.CurrentUserName = null;

        // UI zurücksetzen
        usernameText.text = "";

        loginButton.SetActive(true);
        signInButton.SetActive(true);
        logoutButton.SetActive(false);

        // Optional: zurück zur Login-Szene
        SceneManager.LoadScene("Homepage_Mischa");
    }
}
