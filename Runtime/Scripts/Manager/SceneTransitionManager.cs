using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace com.argentgames.visualnoveltemplate
{
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; set; }
    [SerializeField]
    Image blackscreen;
    Color black = new Color(0, 0, 0, 255);
    Color transparent = new Color(0, 0, 0, 0);
    public bool IsLoading = false;
    public bool isloading { get { return IsLoading; } }
    Tween tween;
    async void Awake()
    {
        // DOTween.Init();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        // TODO
        // await UniTask.WaitUntil(() => LoadingScreenManager.Instance != null);

        // TECHDEBT: Why is this obj turned off on start <_<
        gameObject.SetActive(true);

        // #if UNITY_EDITOR
        // FadeIn();
        // #endif

    }

    public async UniTask LoadScene(string level, float? fadeOutDuration = null, float? fadeInDuration = null, bool doFadeIn = true,
    bool doStopSound = true)
    {
        IsLoading = true;
        MenuManager.Instance.DisableSettingsUIControls();
        if (fadeOutDuration == null)
        {
            fadeOutDuration = GameManager.Instance.DefaultConfig.sceneFadeOutDuration;
        }
        if (fadeInDuration == null)
        {
            fadeInDuration = GameManager.Instance.DefaultConfig.sceneFadeInDuration;
        }
        if (fadeOutDuration != 0)
        {
            await FadeToBlack(fadeOutDuration.Value);
        }

        if (doStopSound)
        {
            AudioManager.Instance.StopMusic(2.5f);
            AudioManager.Instance.StopAmbient(2.5f, 0);
            AudioManager.Instance.StopAmbient(2.5f, 1);
            AudioManager.Instance.StopAmbient(2.5f, 2);
            AudioManager.Instance.StopAmbient(2.5f, 3);
        }


        Debug.Log("load scene??");
        // TODO: prevent player from being able to move or open menus etc? or put this elsewhere
        await SceneManager.LoadSceneAsync(level, LoadSceneMode.Single).AsAsyncOperationObservable()
        .Do(x =>
        {
        });
        // await LoadingScreenManager.Instance.HideLoadingScreen();
        if (doFadeIn && fadeInDuration != 0)
        {
            await FadeIn(fadeInDuration.Value);
        }

        IsLoading = false;



    }
    public async UniTask FadeIn(float dur = 6f, bool allowPlayerMove = true)
    {
        Debug.Log("FADE IN????");
        blackscreen.DOFade(0, dur);//.OnStart(() => {Debug.Log("start");}).OnUpdate(() => {Debug.Log("hello");});
        await UniTask.WaitUntil(() => blackscreen.color.a == 0);
        Instance.blackscreen.transform.parent.gameObject.SetActive(false);

    }
    public async UniTask FadeToBlack(float dur = 4f)
    {
        Debug.Log("start fading to black");
        Instance.blackscreen.color = transparent;
        Instance.blackscreen.transform.parent.gameObject.SetActive(true);
        blackscreen.DOFade(1, dur)
        .OnStart(() =>
        {
            // Debug.Log("start222");
        })
            .OnUpdate(() =>
            {
                // Debug.Log("hello22222")
                ;
            });
        await UniTask.WaitUntil(() => blackscreen.color.a == 1);
        Debug.Log("done fading to black");


    }


}

}