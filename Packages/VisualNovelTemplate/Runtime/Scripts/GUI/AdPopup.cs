#if PLATFORM_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
public class AdPopup : Popup
{
    [SerializeField]
    Button watchAd, buyGame;
    // Start is called before the first frame update
    void Awake()
    {
    }
    void Start()
    {
        // return to main menu if person doesnt wanna watch ads/buy game :(
        close.OnClickAsObservable().Subscribe((val) =>
        {

            ReturnToMM();
        }

        );
        // GameManager.Instance.ResumeGame();
        watchAd.OnClickAsObservable().Subscribe(val =>
        {
            RunInterstitialAd();

        }).AddTo(this);
    }
    async UniTaskVoid ReturnToMM()
    {
        AudioManager.Instance.StopAllAmbient(1);
        AudioManager.Instance.StopMusic(1);
        AdManager.Instance.CloseAdPopup();
        GameManager.Instance.ResumeGame();
        await SceneTransitionManager.Instance.FadeToBlack(1);
        Debug.Break();
        SceneTransitionManager.Instance.LoadScene("MainMenu", 0, 2, doStopSound: false);
        
    }
    public async UniTask RunInterstitialAd()
    {
        AdManager.Instance.CloseAdPopup();

        AdManager.Instance.ShowInterstitial();
        // GameManager.Instance.ResumeGame();

    }

    // public void Enable()
    // {
    //     canvasGroup.DOFade(1,.5f);
    // }
    // public void Disable()
    // {
    //     canvasGroup.DOFade(0,.5f);
    // }
    // void OnEnable()
    // {
    //     Enable();
    // }
    // void OnDisable()
    // {
    //     Disable();
    // }
}

#endif