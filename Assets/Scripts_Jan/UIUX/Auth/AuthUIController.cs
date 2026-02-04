using TMPro;
using UnityEngine;

namespace RedDragon
{
    public class AuthUIController : MonoBehaviour
    {
        [Header("Canvas Roots")]
        [SerializeField] private GameObject authCanvas;
        [SerializeField] private GameObject appCanvas;

        [Header("Panels")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject signUpPanel;

        [Header("Login Inputs")]
        [SerializeField] private TMP_InputField loginUser;
        [SerializeField] private TMP_InputField loginPass;

        [Header("SignUp Inputs")]
        [SerializeField] private TMP_InputField signUser;
        [SerializeField] private TMP_InputField signPass;

        [Header("Feedback (optional)")]
        [SerializeField] private TMP_Text messageText;

        private void Start()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged += HandleAuthChanged;
            else
                Debug.LogWarning("[AuthUI] Start: AuthManager.Instance war NULL (sollte nicht passieren)");
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged -= HandleAuthChanged;
        }

        public void ShowLogin()
        {
            if (loginPanel != null) loginPanel.SetActive(true);
            if (signUpPanel != null) signUpPanel.SetActive(false);
            SetMessage("");
        }

        public void ShowSignUp()
        {
            if (loginPanel != null) loginPanel.SetActive(false);
            if (signUpPanel != null) signUpPanel.SetActive(true);
            SetMessage("");
        }

        public void Login()
        {
            if (AuthManager.Instance == null)
            {
                SetMessage("AuthManager fehlt in der Szene.");
                return;
            }

            var ok = AuthManager.Instance.TryLogin(loginUser?.text, loginPass?.text, out var err);
            if (!ok)
            {
                SetMessage(err);
                return;
            }

            SwitchToApp();
        }

        public void SignUp()
        {
            Debug.Log("[AuthUI] SignUp clicked");

            if (AuthManager.Instance == null)
            {
                Debug.LogError("[AuthUI] AuthManager.Instance is NULL");
                SetMessage("AuthManager fehlt in der Szene.");
                return;
            }

            var ok = AuthManager.Instance.TrySignUp(signUser?.text, signPass?.text, out var err);
            if (!ok)
            {
                SetMessage(err);
                return;
            }

            SwitchToApp();
        }

        public void UseFree()
        {
            Debug.Log("[AuthUI] UseFree clicked");

            if (AuthManager.Instance == null)
            {
                Debug.LogError("[AuthUI] AuthManager.Instance is NULL");
                SetMessage("AuthManager fehlt in der Szene.");
                return;
            }

            AuthManager.Instance.SetFree();

            SwitchToApp();
        }

        private void HandleAuthChanged(AuthMode mode, string user)
        {
            SwitchToApp();
            SetMessage("");
        }

        private void SwitchToApp()
        {
            if (authCanvas != null) authCanvas.SetActive(false);
            else Debug.LogWarning("[AuthUI] authCanvas nicht zugewiesen!");

            if (appCanvas != null) appCanvas.SetActive(true);
            else Debug.LogWarning("[AuthUI] appCanvas nicht zugewiesen!");
        }

        private void SetMessage(string msg)
        {
            if (messageText != null) messageText.text = msg ?? "";
        }
    }
}
