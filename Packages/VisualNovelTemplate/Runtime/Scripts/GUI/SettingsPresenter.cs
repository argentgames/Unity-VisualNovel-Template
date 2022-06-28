using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{
public class SettingsPresenter : SerializedMonoBehaviour
{
    SettingsLogic settingsLogic;
    [SerializeField]
    Button back;
    [SerializeField]
    Toggle cgNav, settingsNav, glossaryNav, creditsNav, saveNav, loadNav, historyNav, musicNav;
    [SerializeField]
    Button mainMenuNav;
    [SerializeField]
    GameObject confirmMMPanel;

    [SerializeField]
    Slider SFX, Music, Ambient;
    [SerializeField]
    CanvasGroup canvasGroup;
    public CanvasGroup CanvasGroup { get { return canvasGroup; } }

    [SerializeField]
    Dictionary<SettingsPage, GameObject> pages = new Dictionary<SettingsPage, GameObject>();

    List<GameObject> activePages = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        settingsLogic = GetComponent<SettingsLogic>();
        foreach (var confirm in this.GetComponentsInChildren<ConfirmPanel>())
        {
            confirm.gameObject.SetActive(false);
        }
        canvasGroup.alpha = 0;
        foreach (var page in pages.Values)
        {
            page.SetActive(false);
        }
        confirmMMPanel.GetComponent<ConfirmPanel>().yes.OnClickAsObservable().Subscribe(val =>
        {

            ConfirmMMPanelLogic();

        }).AddTo(this);
        confirmMMPanel.GetComponent<ConfirmPanel>().no.OnClickAsObservable().Subscribe(val =>
        {

           confirmMMPanel.gameObject.SetActive(false);

        }).AddTo(this);
        gameObject.SetActive(false);

    }

    async UniTaskVoid ConfirmMMPanelLogic()
    {
        if (DialogueSystemManager.Instance != null)
        {
            DialogueSystemManager.Instance.RunCancellationToken();
        }
        GameManager.Instance.ResumeGame();
        AudioManager.Instance.StopMusic(1f);
        AudioManager.Instance.StopAllAmbient(1f);
        await SceneTransitionManager.Instance.FadeToBlack(2);
        MenuManager.Instance.CloseSettings();
        await UniTask.Delay(System.TimeSpan.FromSeconds(1));
        SceneTransitionManager.Instance.LoadScene("MainMenu", 0);
    }
    void Start()
    {



        SetRXSubscriptions();
        SetUIListeners();
    }

    void SetRXSubscriptions()
    {
        GameManager.Instance.Settings.SFXVolume.Subscribe(val =>
        {
            SFX.value = val;

        }).AddTo(this);
        GameManager.Instance.Settings.MusicVolume.Subscribe(val =>
        {
            Music.value = val;

        }).AddTo(this);

        // nav stuff //
        settingsNav.onValueChanged.AsObservable().Subscribe(val =>
        {
            OpenPage(SettingsPage.RegularSettings);


        }).AddTo(this);
        saveNav.onValueChanged.AsObservable().Subscribe(val =>
        {
            OpenPage(SettingsPage.Save);
        }).AddTo(this);
        loadNav.onValueChanged.AsObservable().Subscribe(val =>
        {
            OpenPage(SettingsPage.Load);
        }).AddTo(this);
        historyNav.onValueChanged.AsObservable().Subscribe(val =>
        {
            OpenPage(SettingsPage.History);
        }).AddTo(this);
        mainMenuNav.OnClickAsObservable().Subscribe(val =>
        {
            confirmMMPanel.SetActive(true);
        }).AddTo(this);

        creditsNav.onValueChanged.AsObservable().Subscribe(val =>
        {
            OpenPage(SettingsPage.Credits);
        }).AddTo(this);

        back.OnClickAsObservable().Subscribe(val =>
        {
            MenuManager.Instance.CloseSettings();
        }).AddTo(this);
    }

    void SetUIListeners()
    {
        SFX.onValueChanged.AddListener((val) =>
        GameManager.Instance.Settings.SFXVolume.Value = val);
        Music.onValueChanged.AddListener((val) =>
        GameManager.Instance.Settings.MusicVolume.Value = val);
    }

    public async UniTaskVoid OpenPage(SettingsPage page)
    {
        GameManager.Instance.PauseGame();
        Debug.Log("open da page?");
        Debug.Log(activePages.Count);
        // open settings background obj if it's not active yet
        if (!canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(true);
        }
        // open the actual page
        await UniTask.Yield();
        pages[page].SetActive(true);
        foreach (var _page in activePages)
        {
            if (_page != pages[page])
            {
                _page.SetActive(false);
            }

        }
        activePages.Clear();
        activePages.Add(pages[page]);

        // TECHDEBT: manually set the nav toggle to the right page <_<
        switch(page)
        {
            case SettingsPage.Save:
                saveNav.SetIsOnWithoutNotify(true);
                break;
            case SettingsPage.Load:
                loadNav.SetIsOnWithoutNotify(true);
                break;
            case SettingsPage.RegularSettings:
                settingsNav.SetIsOnWithoutNotify(true);
                break;
            case SettingsPage.History:
                historyNav.SetIsOnWithoutNotify(true);
                break;
        }
        this.gameObject.SetActive(true);
    }

}

}