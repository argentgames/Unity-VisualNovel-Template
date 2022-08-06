using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;

using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{


    public class MainMenuPresenter : MonoBehaviour
    {
        DefaultConfig globals;
        [SerializeField]
        private Button NewGame, Options, LoadGame, Extras, About, Quit;
        [SerializeField]
        private string optionsMenuName = "", loadMenuName = "Load", extrasMenuName = "Extras", aboutMenuName = "About";
        private MainMenuLogic mainMenuLogic;
        [SerializeField]
        private CanvasGroup canvasGroup;

        private async UniTaskVoid Awake()
        {
            try
            {
                MenuManager.Instance.DisableSettingsUIControls();
                // SaveLoadManager.Instance.LoadSaveFiles().Forget();
                globals = GameManager.Instance.DefaultConfig;
                canvasGroup.alpha = 1;

                await UniTask.WaitUntil(() => SceneTransitionManager.Instance != null);
                await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
                MenuManager.Instance.EnableSettingsUIControls();
                // await SceneTransitionManager.Instance.FadeIn(globals.sceneFadeInDuration,false);

            }
            catch
            {
                Debug.LogFormat("You are running MM without preload managers!");
            }
        }
        private async UniTaskVoid Start()
        {
            mainMenuLogic = GetComponent<MainMenuLogic>();
            NewGame.onClick.AddListener(() => mainMenuLogic.StartNewGame());
            if (Options != null)
            {
                Options.onClick.AddListener(() => mainMenuLogic.OpenMenu(optionsMenuName));
            }
            if (LoadGame != null)
            {
                LoadGame.onClick.AddListener(() => mainMenuLogic.OpenMenu(loadMenuName));
            }

            if (Extras != null)
            {
                Extras.onClick.AddListener(() => mainMenuLogic.OpenMenu(extrasMenuName));
            }
            if (About != null)
            {
                About.onClick.AddListener(() => mainMenuLogic.OpenMenu(aboutMenuName));
            }


            Quit.onClick.AddListener(() => mainMenuLogic.QuitGame());

#if PLATFORM_ANDROID // || gamemanager.visualnoveltemplateconfig.adsenabled
        if (!GameManager.Instance.PersistentGameData.gdprConsent)
        {
            AdManager.Instance.ShowGDPRConsent();
        }
        await UniTask.WaitWhile(() => GameManager.Instance.PersistentGameData.gdprConsent == false);
#endif



#if PLATFORM_ANDROID // || gamemanager.visualnoveltemplateconfig.adsenabled // convert to platform flag?
        AdManager.Instance.RequestInterstitial();
#endif

        }

    }

}