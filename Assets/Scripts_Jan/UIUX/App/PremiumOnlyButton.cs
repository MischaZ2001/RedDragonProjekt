using UnityEngine;
using UnityEngine.UI;

namespace RedDragon
{
    [RequireComponent(typeof(Button))]
    public class PremiumOnlyButton : MonoBehaviour
    {
        [SerializeField] private Graphic[] dimGraphics; // Image/Text vom Button reinziehen
        [SerializeField, Range(0f, 1f)] private float freeAlpha = 0.55f;

        private Button button;
        private Color[] originalColors;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (dimGraphics != null && dimGraphics.Length > 0)
            {
                originalColors = new Color[dimGraphics.Length];
                for (int i = 0; i < dimGraphics.Length; i++)
                    originalColors[i] = dimGraphics[i] != null ? dimGraphics[i].color : Color.white;
            }

            ApplyNow();
        }

        private void OnEnable()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged += HandleAuthChanged;

            ApplyNow();
        }

        private void OnDisable()
        {
            if (AuthManager.Instance != null)
                AuthManager.Instance.OnAuthStateChanged -= HandleAuthChanged;
        }

        private void HandleAuthChanged(AuthMode mode, string user)
        {
            Apply(mode == AuthMode.LoggedIn);
        }

        private void ApplyNow()
        {
            bool loggedIn = AuthManager.Instance != null && AuthManager.Instance.Mode == AuthMode.LoggedIn;
            Apply(loggedIn);
        }

        private void Apply(bool loggedIn)
        {
            button.interactable = loggedIn;

            if (dimGraphics == null || originalColors == null) return;

            for (int i = 0; i < dimGraphics.Length; i++)
            {
                if (dimGraphics[i] == null) continue;
                var c = originalColors[i];
                c.a = loggedIn ? originalColors[i].a : Mathf.Min(originalColors[i].a, freeAlpha);
                dimGraphics[i].color = c;
            }
        }
    }
}
