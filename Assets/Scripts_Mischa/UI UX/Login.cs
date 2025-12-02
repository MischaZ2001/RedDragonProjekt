using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Login : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [Header("Server URLs")]
    public string loginUrl = "https://deinserver.de/api/login";     // anpassen
    public string registerUrl = "https://deinserver.de/api/register"; // anpassen

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

    // Button: Registrieren
    public void OnClickRegister()
    {
        SendAuth(registerUrl);
    }

    // Button: Login
    public void OnClickLogin()
    {
        SendAuth(loginUrl);
    }

    void SendAuth(string url)
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            messageText.text = "Bitte Benutzername und Passwort eingeben.";
            return;
        }

        AuthRequest req = new AuthRequest
        {
            username = username,
            password = password
        };

        string json = JsonUtility.ToJson(req);
        StartCoroutine(PostJson(url, json));
    }

    IEnumerator PostJson(string url, string json)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                messageText.text = "Netzwerkfehler: " + www.error;
                yield break;
            }

            string responseJson = www.downloadHandler.text;
            Debug.Log("Response: " + responseJson);

            AuthResponse resp = null;
            try
            {
                resp = JsonUtility.FromJson<AuthResponse>(responseJson);
            }
            catch
            {
                messageText.text = "Fehler beim Lesen der Server-Antwort.";
                yield break;
            }

            if (resp != null && resp.success)
            {
                messageText.text = "Erfolg: " + resp.message;
                
            }
            else
            {
                messageText.text = "Fehler: " + (resp != null ? resp.message : "Unbekannter Fehler");
            }
        }
    }
}
