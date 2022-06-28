using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UniRx;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{


    public class MainMenuPresenter : MonoBehaviour
    {
        DefaultConfig globals;
        [SerializeField]
        private Button NewGame, Options, LoadGame, Extras, About, Quit;
        private MainMenuLogic mainMenuLogic;
        [SerializeField]
        private CanvasGroup canvasGroup;

        private async UniTaskVoid Awake()
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
        private async UniTaskVoid Start()
        {
            mainMenuLogic = GetComponent<MainMenuLogic>();
            NewGame.onClick.AddListener(() => mainMenuLogic.StartNewGame());
            Options.onClick.AddListener(() => mainMenuLogic.ShowSettings(SettingsPage.RegularSettings));
            LoadGame.onClick.AddListener(() => mainMenuLogic.ShowSettings(SettingsPage.Load));
            Extras.onClick.AddListener(() => mainMenuLogic.ShowExtras(ExtrasPage.CG));
            About.onClick.AddListener(() => mainMenuLogic.ShowExtras(ExtrasPage.ABOUT));
            Quit.onClick.AddListener(() => mainMenuLogic.QuitGame());
            Debug.Log("gdpr consent value: " + GameManager.Instance.PersistentGameData.gdprConsent.ToString());

#if PLATFORM_ANDROID // || gamemanager.visualnoveltemplateconfig.adsenabled
        if (!GameManager.Instance.PersistentGameData.gdprConsent)
        {
            AdManager.Instance.ShowGDPRConsent();
        }
#endif

            await UniTask.WaitWhile(() => GameManager.Instance.PersistentGameData.gdprConsent == false);

#if PLATFORM_ANDROID // || gamemanager.visualnoveltemplateconfig.adsenabled // convert to platform flag?
        AdManager.Instance.RequestInterstitial();
#endif

        }

    }

}