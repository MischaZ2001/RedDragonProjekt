using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    [Header("UI Referenzen")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [System.Serializable]
    public class AuthRequest
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class AuthResponse
    {
        public bool success;
        public string message;
        public int userId;
        public string token;
    }

    void Update()
    {
        // ENTER = Login bestätigen
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnLoginClicked();
        }
    }

    // Wird vom Login-Button aufgerufen (und von ENTER)
    public void OnLoginClicked()
    {
        string username = usernameInput != null ? usernameInput.text : "";
        string password = passwordInput != null ? passwordInput.text : "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("Bitte Benutzername und Passwort eingeben.");
            return;
        }

        // Fake-Request (nur für Debug)
        AuthRequest req = new AuthRequest { username = username, password = password };
        string reqJson = JsonUtility.ToJson(req);
        Debug.Log("[LOGIN] Würde JSON senden: " + reqJson);

        // Fake-Response
        AuthResponse resp = new AuthResponse
        {
            success = true,
            message = "Login erfolgreich!",
            userId = 1,
            token = "FAKE_TOKEN"
        };

        string respJson = JsonUtility.ToJson(resp);
        Debug.Log("[LOGIN] Würde JSON zurückbekommen: " + respJson);

        ShowMessage(resp.message);

        if (resp.success)
        {
            // 🔥 Benutzername für die Session merken
            SessionData.CurrentUserName = username;
            Debug.Log("[LOGIN] SessionData.CurrentUserName = " + SessionData.CurrentUserName);

            // Zur Homepage wechseln
            SceneManager.LoadScene("Homepage_Mischa");
        }
    }

    private void ShowMessage(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
        }

        Debug.Log("[UI] " + text);
    }
}
