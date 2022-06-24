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
public class MainMenuPresenter : MonoBehaviour
{
    GlobalDefinitions globals;
    [SerializeField]
    private Button NewGame, Options, Quit, LoadGame, Extras, About;
    private MainMenuLogic mainMenuLogic;
    [SerializeField]
    private CanvasGroup canvasGroup;

    private async UniTaskVoid Awake()
    {
        SettingsManager.Instance.DisableSettingsUIControls();
        // SaveLoadManager.Instance.LoadSaveFiles().Forget();
        globals = GameManager.Instance.GlobalDefinitions;
        canvasGroup.alpha = 1;

        await UniTask.WaitUntil(() => SceneTransitionManager.Instance != null);
        await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
        SettingsManager.Instance.EnableSettingsUIControls();
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
        Debug.Log("gdpr consent value: " + GameManager.Instance.Settings.gdprConsent.ToString());
        #if PLATFORM_ANDROID
        if (!GameManager.Instance.Settings.gdprConsent)
        {
            AdManager.Instance.ShowGDPRConsent();
        }
        #endif

        await UniTask.WaitWhile(() => GameManager.Instance.Settings.gdprConsent == false);

        #if PLATFORM_ANDROID
        AdManager.Instance.RequestInterstitial();
        #endif

    }

}
