using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

#if PLATFORM_ANDROID || UNITY_ANDROID
using GoogleMobileAds.Api;
#endif
public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    [SerializeField]
    GameObject ShowAdPopup, gdprConsentPopup;
    Popup popup;
#if PLATFORM_ANDROID || UNITY_ANDROID
    private InterstitialAd interstitial;
#endif
    public bool IsTryingToRunAd;

    private void Awake()
    {
        // TODO: DEACTIVATE SELF IF ADS NOT ENABLED
        gameObject.SetActive(false);
    }
    public void Start()
    {
        Instance = this;
#if PLATFORM_ANDROID || UNITY_ANDROID
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });
#endif

        popup = ShowAdPopup.GetComponent<Popup>();
        ShowAdPopup.SetActive(false);



    }

    public void Initialize()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        MobileAds.Initialize(initStatus => { });
#endif
    }
    public void CloseAdPopup()
    {
        ShowAdPopup.SetActive(false);
    }


#if PLATFORM_ANDROID || UNITY_ANDROID
    public bool InterstitialAdIsLoaded { get { return this.interstitial.IsLoaded(); } }
#endif
    public void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-6975564727093442/6130190272";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-6975564727093442/6130190272";
#else
        string adUnitId = "unexpected_platform";
#endif

#if PLATFORM_ANDROID || UNITY_ANDROID
        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);

        // Called when an ad request has successfully loaded.
        this.interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        this.interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        this.interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        // this.interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        // Create an empty ad request.
        // Add npa 1 for non-personalized requests |:
        AdRequest request = new AdRequest.Builder().AddExtra("npa", "1")
        .Build();

        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);

#endif

    }
    public async UniTask ShowInterstitial()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        if (interstitial == null)
        {
            RequestInterstitial();
        }
        GameManager.Instance.PauseGame();
        await UniTask.WaitUntil(() => InterstitialAdIsLoaded);
        this.interstitial.Show();
        // var adGO = GameObject.Find("Ad");
        // if (adGO != null)
        // {
        //     var canvas = adGO.GetComponentInParent<Canvas>();
        //     canvas.sortingOrder = 10000;
        // }
        await UniTask.WaitUntil(() => interstitial == null);

#endif

    }
    public void CleanupInterstitial()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        this.interstitial.Destroy();
        this.interstitial = null;
         GameManager.Instance.ResumeGame();
        IsTryingToRunAd = false;
#endif
    }
    [Button]
    public void ShowAdPopupToContinue()
    {
#if PLATFORM_ANDROID || UNITY_ANDROID
        IsTryingToRunAd = true;
        RequestInterstitial();
        GameManager.Instance.PauseGame();
        GameManager.Instance.SetSkipping(false);
        GameManager.Instance.SetAuto(false);

        ShowAdPopup.SetActive(true);
        DialogueSystemManager.Instance.dialogueUIManager.DisableCTC();
        DialogueSystemManager.Instance.dialogueUIManager.HideUI();
#endif
    }
    public void ShowGDPRConsent()
    {
        Initialize();
        gdprConsentPopup.SetActive(true);
    }


#if PLATFORM_ANDROID || UNITY_ANDROID
    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: ");
                            // + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        CleanupInterstitial();
       
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
        CleanupInterstitial();
    }
#endif




}
