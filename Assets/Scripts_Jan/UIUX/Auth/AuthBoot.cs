using UnityEngine;

namespace RedDragon
{
    public class AuthBoot : MonoBehaviour
    {
        [SerializeField] private GameObject authCanvas;
        [SerializeField] private GameObject appCanvas;
        [SerializeField] private AuthUIController ui;

        private void Start()
        {
            // Start immer im Login-Bereich
            if (authCanvas != null) authCanvas.SetActive(true);
            if (appCanvas != null) appCanvas.SetActive(false);

            if (ui != null) ui.ShowLogin();
        }
    }
}
