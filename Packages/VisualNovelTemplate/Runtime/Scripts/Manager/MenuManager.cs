using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
// using UnityEngine.UI.Extensions;
using System.Threading;
using DG.Tweening;
using Unity.Profiling;
using UnityEngine.SceneManagement;

public enum SettingsType
{
    INGAME,
    MAINMENU
}
namespace com.argentgames.visualnoveltemplate
{

    public class MenuManager : SerializedMonoBehaviour
    {
        public static MenuManager Instance;

        [SerializeField]
        List<MenuPrefab_SO> menuPrefabs;
        Dictionary<string, MenuPrefab_SO> menuMap = new Dictionary<string, MenuPrefab_SO>();
        GameObject ingameSettingsPrefab, mainmenuSettingsPrefab, extrasPrefab;

        PlayerControls _playerControls;

        CancellationTokenSource cts;
        CancellationToken ct;

        Dictionary<string, GameObject> openMenus = new Dictionary<string, GameObject>();
        GameObject currentMenu;

        async UniTaskVoid Awake()
        {
            Instance = this;
            cts = new CancellationTokenSource();
            ct = cts.Token;

            foreach (var menu in menuPrefabs)
            {
                menuMap[menu.internalName] = menu;
            }

            _playerControls = new PlayerControls();
            _playerControls.UI.Settings.performed += ctx =>
            {
                try
                {
                    // Don't let player skip SplashScreen scene because we are doing loading during it.
                    if (SceneManager.GetActiveScene().name == "SplashScreens")
                    {
                        return;
                    }
                    // Don't open menus while a video is playing
                    if (VideoPlayerManager.Instance != null)
                    {
                        if (VideoPlayerManager.Instance.IsVideoPlaying)
                        {
                            return;
                        }
                    }
                    if (currentMenu != null)
                    {
                        CloseAllMenus();
                    }

                    GameManager.Instance.SetSkipping(false);
                    GameManager.Instance.SetAuto(false);
                }
                catch
                {
                    Debug.Log("someon is spam clicking D:<");
                }

            };

        }
        async UniTaskVoid RunCancellation()
        {
            cts.Cancel();
            await UniTask.Yield();
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }

        /// <summary>
        /// We may want to manually enable/disable access to the UI during gameplay.
        /// </summary>
        public void EnableSettingsUIControls()
        {
            _playerControls.Enable();
        }
        public void DisableSettingsUIControls()
        {
            _playerControls.Disable();
        }

        private void OnEnable()
        {
            _playerControls.Enable();
        }
        private void OnDisable()
        {
            _playerControls.Disable();
        }
        void Update() { }

        public async UniTask OpenPage(string menuName, string pageName = "")
        {

            // we shouldn't need this, but we may need to check that saves are done loading
            // before we allow any opening of settings pages
            // TECHDEBT
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                await UniTask.WaitUntil(() => SaveLoadManager.Instance.DoneLoadingSaves);
            }
            else if (!SaveLoadManager.Instance.DoneLoadingSaves)
            {
                await SaveLoadManager.Instance.LoadSaveFiles();
            }

            // if we are ingame, we need to take a screenshot before the settings page shows up and covers the screen
            // in theory we would not be doing this take screenshot nonsense but just reading a camera texture composite
            // android screenshot has some extra delay needed because it returns immediately.
            // TECHDEBT
            if (SceneManager.GetActiveScene().name == "Ingame")
            {
#if PLATFORM_ANDROID
                await UniTask.Delay (100);
#endif
                GameManager.Instance.TakeScreenshot();

#if PLATFORM_ANDROID
                await UniTask.Delay (100);
#endif
            }

            // if we don't have any menus open right now, then we need to instantiate one
            // otherwise find the open menu and bring it forward and make it the focused menu
            if (currentMenu == null)
            {
                if (openMenus.Count == 0)
                {
                    // NO ADDRESSABLES SUPPORTED RIGHT NOW
                    // TODO
                    var go = Instantiate(menuMap[menuName].prefab, this.transform);
                    openMenus[menuName] = go;
                }
                else
                {
                    currentMenu = openMenus[menuName];
                    // move the menu order to the top, which means making it the last child of menumanager
                    currentMenu.transform.SetSiblingIndex(this.transform.childCount - 1);
                }
            }
            var menuPresenter = currentMenu.GetComponent<MenuPresenter>();
            menuPresenter.OpenPage(pageName);
        }

        public void CloseAllMenus()
        {
            // HACK: why do i have to reset this a bunch of places
            GameManager.Instance.SetSkipping(false);
            GameManager.Instance.SetAuto(false);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<MenuPresenter>().CloseMenu();
            }
            GameManager.Instance.ResumeGame();
        }

    }
}