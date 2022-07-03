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

public enum SettingsType {
    INGAME,
    MAINMENU
}
namespace com.argentgames.visualnoveltemplate {

    public class MenuManager : SerializedMonoBehaviour {
        public static MenuManager Instance;

        [SerializeField]
        List<MenuPrefab_SO> menuPrefabs;
        Dictionary<string,MenuPrefab_SO> menuMap = new Dictionary<string, MenuPrefab_SO>();
        GameObject ingameSettingsPrefab, mainmenuSettingsPrefab, extrasPrefab;

        [SerializeField] GameObject settings;

        PlayerControls _playerControls;

        CancellationTokenSource cts;
        CancellationToken ct;

        List<GameObject> openMenus = new List<GameObject>();
        GameObject currentMenu;

        async UniTaskVoid Awake () {
            Instance = this;
            cts = new CancellationTokenSource ();
            ct = cts.Token;

            foreach (var menu in menuPrefabs)
            {
                menuMap[menu.internalName] = menu;
            }

            _playerControls = new PlayerControls ();
            _playerControls.UI.Settings.performed += ctx => {
                try {
                    // Don't let player skip SplashScreen scene because we are doing loading during it.
                    if (SceneManager.GetActiveScene ().name == "SplashScreens") {
                        return;
                    }
                    // Don't open menus while a video is playing
                    if (VideoManager.Instance != null) {
                        if (VideoManager.Instance.IsVideoPlaying) {
                            return;
                        }
                    }
                    if (settings == null) // TECHDEBT: HACKY TO NOT TRIGGER IF EXTRAS IS OPEN
                    {
                        if (GameManager.Instance.IsExtrasOpen) {
                            return;
                        }

                        // s_PreparePerfMarker.Begin();

                        if (SceneManager.GetActiveScene ().name == "MainMenu") {
                            OpenPage (SettingsPage.RegularSettings, SettingsType.MAINMENU);
                        } else {
                            OpenPage (SettingsPage.RegularSettings, SettingsType.INGAME);
                        }
                        //  s_PreparePerfMarker.End();
                    } else {
                        CloseSettings ();
                    }

                    GameManager.Instance.SetSkipping (false);
                    GameManager.Instance.SetAuto (false);
                } catch {
                    Debug.Log ("someon eis spam clicking D:<");
                }

            };

        }
        async UniTaskVoid RunCancellation () {
            cts.Cancel ();
            await UniTask.Yield ();
            cts = new CancellationTokenSource ();
            ct = cts.Token;
        }

        public void EnableSettingsUIControls () {
            _playerControls.Enable ();
        }
        public void DisableSettingsUIControls () {
            _playerControls.Disable ();
        }

        private void OnEnable () {
            _playerControls.Enable ();
        }
        private void OnDisable () {
            _playerControls.Disable ();
        }
        void Update () { }

        public async UniTask OpenPage(string menuName, string pageName="")
        {

            // we shouldn't need this, but we may need to check that saves are done loading
            // before we allow any opening of settings pages
            // TECHDEBT
            if (SceneManager.GetActiveScene ().name == "MainMenu") {
                await UniTask.WaitUntil (() => SaveLoadManager.Instance.DoneLoadingSaves);
            } else if (!SaveLoadManager.Instance.DoneLoadingSaves) {
                await SaveLoadManager.Instance.LoadSaveFiles ();
            }

            if (currentMenu == null)
            {
                if (openMenus.Count == 0)
                {
                    var go = Instantiate(menuMap[menuName].prefab,this.transform);
                    openMenus.Add(go);
                }
                else
                {
                    currentMenu = openMenus[openMenus.Count -1];
                }
            }
            var settingsPresenter = currentMenu.GetComponent<SettingsPresenter>();
            settingsPresenter.OpenPage(pageName);
        }

        [Button]
        public async UniTask OpenPage (SettingsPage page, SettingsType _type) {
            // this should only occur if you open the settings in main menu
            // we need to make sure all the saves have been loaded in before someone tries to hit the load button 0;
            if (SceneManager.GetActiveScene ().name == "MainMenu") {
                await UniTask.WaitUntil (() => SaveLoadManager.Instance.DoneLoadingSaves);
            } else if (!SaveLoadManager.Instance.DoneLoadingSaves) {
                await SaveLoadManager.Instance.LoadSaveFiles ();
            }

            if (settings == null) {
                if (SceneManager.GetActiveScene ().name == "Ingame") {
#if PLATFORM_ANDROID
                    await UniTask.Delay (100);
#endif
                    GameManager.Instance.TakeScreenshot ();

#if PLATFORM_ANDROID
                    await UniTask.Delay (100);
#endif
                }

                await UniTask.Yield ();

                switch (_type) {
                    case SettingsType.INGAME:
                        settings = await AssetRefLoader.Instance.LoadAsset (ingameSettingsPrefab, this.gameObject.transform);
                        break;
                    case SettingsType.MAINMENU:
                        settings = await AssetRefLoader.Instance.LoadAsset (mainmenuSettingsPrefab, this.gameObject.transform);
                        break;

                }

            }

            var settingsPresenter = settings.GetComponent<SettingsPresenter> ();
            // settings.SetActive(false);

            settingsPresenter.OpenPage (page);
            // settings.SetActive(true);
            // s_PreparePerfMarker.End();
            // Debug.LogErrorFormat("took {0} time to open settings", start.ElapsedMilliseconds);

        }
        public async UniTaskVoid CloseSettings () {
            // HACK: why do i have to reset this a bunch of places
            GameManager.Instance.SetSkipping (false);
            GameManager.Instance.SetAuto (false);

            try {
                await UniTask.Yield ();
                if (settings != null) {

                    settings.GetComponentInChildren<CanvasGroup> ().DOFade (0, .4f);
                    await UniTask.WaitUntil (() => settings.GetComponentInChildren<CanvasGroup> ().alpha == 0);
                    try {
                        if (settings.gameObject != null) {
                            Destroy (settings.gameObject);
                        }
                    } catch {
                        Debug.Log ("settings gameobject reference already dead");
                    }

                    // await UniTask.WaitUntil(() => settings.GetComponent<AnimateObjectsToggleEnable>().AnimationComplete);
                    settings = null;
                    await UniTask.WaitWhile (() => SceneTransitionManager.Instance.IsLoading);

                }
            } catch {
                Debug.Log ("PLEASE STOP SPAM CLICKING");
            }

            GameManager.Instance.ResumeGame ();
            SaveLoadManager.Instance.SaveSettings ();

        }

    }
}