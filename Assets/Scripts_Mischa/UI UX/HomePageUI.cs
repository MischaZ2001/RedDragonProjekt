using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HomepageUI : MonoBehaviour
{
    [Header("UI Elemente")]
    public UnityEngine.UI.Image profileCircle;
    public TMP_Text profileLetterText;

    public GameObject loginButton;
    public GameObject signInButton;
    public GameObject logoutButton;

    void Start()
    {
        Debug.Log("[HOMEPAGE] Start -> SessionData.CurrentUserName = " + SessionData.CurrentUserName);

        bool isLoggedIn = !string.IsNullOrEmpty(SessionData.CurrentUserName);

        if (isLoggedIn)
        {
            string user = SessionData.CurrentUserName;

            profileLetterText.text = user.Substring(0, 1).ToUpper();
            profileCircle.color = Color.red;

            loginButton.SetActive(false);
            signInButton.SetActive(false);
            logoutButton.SetActive(true);
        }
        else
        {
            profileLetterText.text = "";
            profileCircle.color = Color.white;

            loginButton.SetActive(true);
            signInButton.SetActive(true);
            logoutButton.SetActive(false);
        }
    }

    public void OnLogoutClicked()
    {
        Debug.Log("[HOMEPAGE] Logout geklickt");

        SessionData.CurrentUserName = null;

        profileLetterText.text = "";
        profileCircle.color = Color.white;

        loginButton.SetActive(true);
        signInButton.SetActive(true);
        logoutButton.SetActive(false);

        SceneManager.LoadScene("Homepage-Mischa2");
    }
}
