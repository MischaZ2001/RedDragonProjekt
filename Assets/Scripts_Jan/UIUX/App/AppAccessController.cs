using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedDragon
{
    public class AppAccessController : MonoBehaviour
    {
        [Header("Ads")]
        [SerializeField] private GameObject adsRoot;

        [Header("Buttons only for logged-in users")]
        [SerializeField] private Button[] premiumOnlyButtons;

        [Header("Profile UI")]
        [SerializeField] private Image profileCircle;
        [SerializeField] private TMP_Text profileLetter;

        [Header("Colors")]
        [SerializeField] private Color freeColor = Color.white;
        [SerializeField] private Color loggedInColor = Color.red;

        [Header("AppCanvas Auth Buttons (Free Mode)")]
        [SerializeField] private GameObject loginButtonObj;   // Button GameObject (nicht Button-Komponente)
        [SerializeField] private GameObject signInButtonObj;  // Button GameObject

        [Header("AppCanvas Buttons (Mode Dependent)")]
        [SerializeField] private Button settingsButton;       // soll in Free NICHT funktionieren
        [SerializeField] private GameObject logoutButtonObj;   // Logout nur bei LoggedIn sichtbar

        [Header("Auth Canvas References (for opening panels from AppCanvas)")]
        [SerializeField] private GameObject authCanvas;
        [SerializeField] private GameObject appCanvas;
        [SerializeField] private AuthUIController authUI;

        private void OnEnable()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged += Apply;

            Apply(AuthManager.Instance != null ? AuthManager.Instance.Mode : AuthMode.Free,
                  AuthManager.Instance != null ? AuthManager.Instance.CurrentUser : null);
        }

        private void OnDisable()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged -= Apply;
        }

        // AppCanvas Login Button -> ruft das LoginPanel auf
        public void OpenLogin()
        {
            if (authCanvas != null) authCanvas.SetActive(true);
            if (appCanvas != null) appCanvas.SetActive(false);

            if (authUI != null) authUI.ShowLogin();
        }

        // AppCanvas SignIn Button -> ruft das SignUpPanel auf
        public void OpenSignUp()
        {
            if (authCanvas != null) authCanvas.SetActive(true);
            if (appCanvas != null) appCanvas.SetActive(false);

            if (authUI != null) authUI.ShowSignUp();
        }

        // AppCanvas Logout Button
        public void LogoutToFree()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.LogoutToFree();

            // du willst: direkt in Free-Version springen (ohne zurück zum Login)
            // -> AppCanvas bleibt an, AuthCanvas bleibt aus
            if (authCanvas != null) authCanvas.SetActive(false);
            if (appCanvas != null) appCanvas.SetActive(true);
        }

        private void Apply(AuthMode mode, string user)
        {
            bool loggedIn = mode == AuthMode.LoggedIn;

            // Werbung
            if (adsRoot != null) adsRoot.SetActive(!loggedIn);

            // Premium Buttons
            if (premiumOnlyButtons != null)
            {
                foreach (var b in premiumOnlyButtons)
                    if (b != null) b.interactable = loggedIn;
            }

            // Settings nur bei LoggedIn
            if (settingsButton != null)
                settingsButton.interactable = loggedIn;

            // Login/SignIn Buttons nur in Free sichtbar
            if (loginButtonObj != null) loginButtonObj.SetActive(!loggedIn);
            if (signInButtonObj != null) signInButtonObj.SetActive(!loggedIn);

            // Logout Button nur bei LoggedIn sichtbar
            if (logoutButtonObj != null) logoutButtonObj.SetActive(loggedIn);

            // Profil UI
            if (profileCircle != null)
                profileCircle.color = loggedIn ? loggedInColor : freeColor;

            if (profileLetter != null)
            {
                profileLetter.text = loggedIn && !string.IsNullOrWhiteSpace(user)
                    ? user.Trim()[0].ToString().ToUpper()
                    : "";
            }
        }
    }
}
