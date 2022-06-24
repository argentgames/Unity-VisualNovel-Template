#if PLATFORM_ANDROID || UNITY_ANDROID
using System;
using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.UI;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
public class AdManager : MonoBehaviour
{
    public static AdManager Instance;
    [SerializeField]
    GameObject ShowAdPopup, gdprConsentPopup; 
    Popup popup;
    private InterstitialAd interstitial;
    public bool IsTryingToRunAd;
    public void Start()
    {
        Instance = this;
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        popup = ShowAdPopup.GetComponent<Popup>();
        ShowAdPopup.SetActive(false);

        

    }

    public void Initialize()
    {
        MobileAds.Initialize(initStatus => { });
    }
    public void CloseAdPopup()
    {
        ShowAdPopup.SetActive(false);
    }

    

    public bool InterstitialAdIsLoaded { get { return this.interstitial.IsLoaded(); } }
    public void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-6975564727093442/6130190272";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-6975564727093442/6130190272";
#else
        string adUnitId = "unexpected_platform";
#endif

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

    }
    public async UniTask ShowInterstitial()
    {
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

    }
    public void CleanupInterstitial()
    {
        this.interstitial.Destroy();
        this.interstitial = null;
         GameManager.Instance.ResumeGame();
        IsTryingToRunAd = false;
    }
    [Button]
    public void ShowAdPopupToContinue()
    {
        IsTryingToRunAd = true;
        RequestInterstitial();
        GameManager.Instance.PauseGame();
        GameManager.Instance.SetSkipping(false);
        GameManager.Instance.SetAuto(false);

        ShowAdPopup.SetActive(true);
        DialogueSystem.Instance.dialogueUIManager.DisableCTC();
        DialogueSystem.Instance.dialogueUIManager.HideUI();
    }
    public void ShowGDPRConsent()
    {
        Initialize();
        gdprConsentPopup.SetActive(true);
    }



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




}


#endif