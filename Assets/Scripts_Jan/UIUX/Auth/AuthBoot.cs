using UnityEngine;

namespace RedDragon
{
    public class AuthBoot : MonoBehaviour
    {
        [SerializeField] private GameObject authCanvas;
        [SerializeField] private GameObject appCanvas;
        [SerializeField] private GameObject appCanvasWith;
        [SerializeField] private AuthUIController ui;

        private void Start()
        {
            // Start immer im Login-Bereich
            if (authCanvas != null) authCanvas.SetActive(true);
            if (appCanvas != null) appCanvas.SetActive(false);
            if (appCanvasWith != null) appCanvasWith.SetActive(true);

            if (ui != null) ui.ShowLogin();
        }
    }
}
