using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace com.argentgames.visualnoveltemplate
{
    public class PlayerControlsManager : MonoBehaviour
    {
        public static PlayerControlsManager Instance { get; set; }
        public static PlayerControls defaultControls;
        void Awake()
        {
            defaultControls = new PlayerControls();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        void ToggleSettings(InputAction.CallbackContext ctx)
        {
            if (MenuManager.Instance.IsMenuOpen)
            {
                MenuManager.Instance.CloseAllMenus();
                return;
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainMenu")
            {
                MenuManager.Instance.OpenPage("ingameSettings");
            }
            else
            {
                MenuManager.Instance.OpenPage("mmSettings");
            }
GameManager.Instance.SetSkipping(false);
                    GameManager.Instance.SetAuto(false);

        }
        void OnEnable()
        {
            defaultControls.Enable();
            defaultControls.UI.Settings.performed += ToggleSettings;
        }
        void OnDisable()
        {
            defaultControls.Disable();
            defaultControls.UI.Settings.performed -= ToggleSettings;
        }
        public void EnableSettingsControls()
        {
            defaultControls.UI.Settings.Enable();
        }
        public void DisableSettingsControls()
        {
            defaultControls.UI.Settings.Disable();
        }
        public static void EnableActionMap(InputActionMap actionMap)
        {
            actionMap.Enable();
        }
        public static void DisableActionMap(InputActionMap actionMap)
        {
            actionMap.Disable();
        }
    }
}
