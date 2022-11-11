using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;
// using UnityEngine.UI.Extensions;
using System.Threading;

using Unity.Profiling;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public enum SettingsType
{
    INGAME,
    MAINMENU
}
namespace com.argentgames.visualnoveltemplate
{

    public class MenuManager : MonoBehaviour
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

        public bool IsMenuOpen { get { return currentMenu != null; } }
        async UniTaskVoid Awake()
        {
            Instance = this;
            cts = new CancellationTokenSource();
            ct = cts.Token;

            foreach (var menu in menuPrefabs)
            {
                menuMap[menu.internalName] = menu;

            }
            await UniTask.WaitUntil(() => Manager.allManagersLoaded.Value);
            SpawnAllMenus();


        }
        async UniTaskVoid RunCancellation()
        {
            cts.Cancel();
            await UniTask.Yield();
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
        // HACK: open every page and spawn every menu because otherwise sometimes pages don't open the first time??
        public void SpawnAllMenus()
        {
            foreach (var kv in menuMap)
            {
                var menuName = kv.Key;
                var go = Instantiate(menuMap[menuName].prefab, this.transform);
                openMenus[menuName] = go;
                OpenPage(menuName);
                // OpenEveryPage(menuName);
            }
            CloseAllMenus();

        }
        void OpenEveryPage(string menuName)
        {
            var menuPresenter = openMenus[menuName].GetComponentInChildren<MenuPresenter>();

            menuPresenter.OpenEveryPage();
        }

        /// <summary>
        /// We may want to manually enable/disable access to the UI during gameplay.
        /// </summary>
        public void EnableSettingsUIControls()
        {
            PlayerControlsManager.Instance.EnableSettingsControls();
        }
        public async UniTaskVoid DisableSettingsUIControls()
        {
            await UniTask.WaitUntil(() => Manager.allManagersLoaded.Value);
            PlayerControlsManager.Instance.DisableSettingsControls();
        }

        void Update() { }

        public async UniTask OpenPage(string menuName, string pageName = "")
        {
            Debug.LogFormat("trying to open menu: {0}, {1}", menuName, pageName);

            // we shouldn't need this, but we may need to check that saves are done loading
            // before we allow any opening of settings pages
            // TECHDEBT
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                // await UniTask.WaitUntil(() => SaveLoadManager.Instance.DoneLoadingSaves);
            }
            else if (!SaveLoadManager.Instance.DoneLoadingSaves)
            {
                // await SaveLoadManager.Instance.LoadSaveFiles();
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
                await GameManager.Instance.TakeScreenshot();

#if PLATFORM_ANDROID
                await UniTask.Delay (100);
#endif
            }

            // if we don't have any menus open right now, then we need to instantiate one
            // otherwise find the open menu and bring it forward and make it the focused menu
            Debug.LogFormat("currentMenu: {0}", currentMenu);
            if (currentMenu == null)
            {
                if (openMenus.Count == 0 || !openMenus.ContainsKey(menuName))
                {

                    // NO ADDRESSABLES SUPPORTED RIGHT NOW
                    // TODO
                    var go = Instantiate(menuMap[menuName].prefab, this.transform);
                    openMenus[menuName] = go;
                    currentMenu = go;
                    Debug.Log("spawning a new menu: " + menuName);
                }
                else
                {
                    currentMenu = openMenus[menuName];
                    // move the menu order to the top, which means making it the last child of menumanager
                    currentMenu.transform.SetSiblingIndex(this.transform.childCount - 1);
                    Debug.LogFormat("NO SPAWN NEW MENU currentMenu: {0}", currentMenu);
                }
            }
            else
            {
                currentMenu = openMenus[menuName];
                // move the menu order to the top, which means making it the last child of menumanager
                currentMenu.transform.SetSiblingIndex(this.transform.childCount - 1);
                Debug.LogFormat("NO SPAWN NEW MENU currentMenu: {0}", currentMenu);
            }
            Debug.LogFormat("currentMenu: {0}", currentMenu);
            // Debug.Break();
            try
            {
                var menuPresenter = currentMenu.GetComponent<MenuPresenter>();
            Debug.Log("PLEASE OPEN THE PAGE WITH NAME: " + pageName);
            menuPresenter.OpenPage(pageName);
            }
            catch
            {
                Debug.LogErrorFormat("unable to open menu?? {0}, {1}, {2}", menuName, pageName, currentMenu);
            }
            
        }

        public void CloseAllMenus()
        {
            // HACK: why do i have to reset this a bunch of places
            GameManager.Instance.SetSkipping(false);
            GameManager.Instance.SetAuto(false);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<MenuPresenter>().CloseMenu();
                Debug.Log("closing menu");
            }
            GameManager.Instance.ResumeGame();
            currentMenu = null;
        }

    }
}