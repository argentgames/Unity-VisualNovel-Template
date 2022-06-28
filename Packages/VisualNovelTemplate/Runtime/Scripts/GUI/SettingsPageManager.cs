using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;

namespace com.argentgames.visualnoveltemplate
{
public class SettingsPageManager : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown resolutions;
    [SerializeField]
    Slider sfxVolume, musicVolume, textSpeed, autoSpeed;
    [SerializeField]
    Toggle seenText, allText, windowMode, fullscreenMode;
    [SerializeField]
    Button accessibilities, controls;
    [SerializeField]
    GameObject accessibilitiesPopup, controlsPopup;
    List<Vector2Int> availableResolutions = new List<Vector2Int>();
    private List<Vector2Int> allowedResolutions = new List<Vector2Int> {
//   new Vector2Int(2560, 1440),
  new Vector2Int(1920, 1080),
  new Vector2Int(1280, 720),
//   new Vector2Int(640, 360),
//   new Vector2Int(320, 180)
 };
 [SerializeField]
 LayoutGroup layoutGroup;
    Dictionary<string,Vector2Int> resolutionsMap = new Dictionary<string, Vector2Int>();
    // Start is called before the first frame update
    void Awake()
    {
        availableResolutions.Clear();
        resolutionsMap.Clear();
        #if !UNITY_STANDALONE_OSX
        foreach (var s in Screen.resolutions)
        {
            var res = new Vector2Int();
            res.x = s.width;
            res.y = s.height;
            if (allowedResolutions.Contains(res))
            {
                if (!availableResolutions.Contains(res))
                {
                    availableResolutions.Add(res);
                }
                else
                {
                    Debug.LogWarningFormat("how can there already be this res in availableResolutions: {0}",res);
                }
                if (!resolutionsMap.ContainsKey(string.Format("{0} x {1}",res.x,res.y)))
                {
                    resolutionsMap.Add(string.Format("{0} x {1}",res.x,res.y),res);
                }
                else
                {
                    Debug.LogWarningFormat("how can resmap already have res: {0}",res);
                }
                
            }
        }
        #else
        foreach (var s in allowedResolutions)
        {
            var res = new Vector2Int();
            res.x = s.x;
            res.y = s.y;
            if (allowedResolutions.Contains(res))
            {
                if (!availableResolutions.Contains(res))
                {
                    availableResolutions.Add(res);
                }
                else
                {
                    Debug.LogWarningFormat("how can there already be this res in availableResolutions: {0}",res);
                }
                if (!resolutionsMap.ContainsKey(string.Format("{0} x {1}",res.x,res.y)))
                {
                    resolutionsMap.Add(string.Format("{0} x {1}",res.x,res.y),res);
                }
                else
                {
                    Debug.LogWarningFormat("how can resmap already have res: {0}",res);
                }
                
            }
        }
        #endif
        // this thing is spawned so we know gm already exists
        SetInitialValues();
        layoutGroup.enabled = false;
        layoutGroup.enabled = true;
        SetRXSubscriptions();

        accessibilitiesPopup.SetActive(false);
        #if !PLATFORM_ANDROID && !UNITY_ANDROID
        controlsPopup.SetActive(false);
        #endif
        
    }

    void SetInitialValues()
    {
        Debug.Log("set initial values??");
        sfxVolume.value = GameManager.Instance.Settings.SFXVolume.Value;
        musicVolume.value = GameManager.Instance.Settings.MusicVolume.Value;
        textSpeed.value = GameManager.Instance.Settings.TextSpeed.Value;
        autoSpeed.value = GameManager.Instance.Settings.AutoSpeed.Value;
        if (GameManager.Instance.Settings.skipAllText)
        {
            allText.SetIsOnWithoutNotify(true);
        }
        else
        {
            seenText.SetIsOnWithoutNotify(true);
        }

        List<TMP_Dropdown.OptionData> allNewOptions = new List<TMP_Dropdown.OptionData>();
        for (int i=0; i < availableResolutions.Count; i++)
        {
            var newOption = new TMP_Dropdown.OptionData();
            newOption.text = string.Format("{0} x {1}",availableResolutions[i].x,availableResolutions[i].y);
            allNewOptions.Add(newOption);
        }
        resolutions.AddOptions(allNewOptions);
        resolutions.RefreshShownValue();
        var _resolutions = Screen.resolutions;

        var w = Screen.width;
        var h = Screen.height;
        var res = string.Format("{0} x {1}",w,h);
        Debug.Log("number of available resolutions: " + resolutions.options.Count.ToString());
        for (int i=0; i < resolutions.options.Count; i++)
        {
            Debug.LogFormat("resolutions option: [{0}], current resolution: [{1}]",resolutions.options[i].text,res);
            if (resolutions.options[i].text == res)
            {
                // Debug.LogError("the init resolution is: " + resolutions.options[i].text + "; " + res);
                resolutions.SetValueWithoutNotify(i);
                break;
            }
        }
        Debug.Log("are we in fullscreen mode? " + Screen.fullScreen.ToString());
        if (Screen.fullScreen)
        {
            fullscreenMode.SetIsOnWithoutNotify(true);
        }
        else
        {
            Debug.Log("setting windowMode to true");
            windowMode.SetIsOnWithoutNotify(true);
        }

    }

    void SetRXSubscriptions()
    {
        sfxVolume.onValueChanged.AsObservable().Subscribe(val =>
        {
            GameManager.Instance.Settings.SFXVolume.Value = val;
        }).AddTo(this);
        musicVolume.onValueChanged.AsObservable().Subscribe(val =>
       {
           GameManager.Instance.Settings.MusicVolume.Value = val;
       }).AddTo(this);
        textSpeed.onValueChanged.AsObservable().Subscribe(val =>
       {
           GameManager.Instance.Settings.TextSpeed.Value = val;
       }).AddTo(this);
        autoSpeed.onValueChanged.AsObservable().Subscribe(val =>
       {
           GameManager.Instance.Settings.AutoSpeed.Value = val;
       }).AddTo(this);
        seenText.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (val)
            {
                GameManager.Instance.Settings.skipAllText = false;
            }
        }).AddTo(this);
        allText.onValueChanged.AsObservable().Subscribe(val =>
        {
            if (val)
            {
                GameManager.Instance.Settings.skipAllText = true;
            }
        }).AddTo(this);
        windowMode.onValueChanged.AsObservable().Subscribe(val =>
        {
            Debug.Log("fire window subscription");
            Debug.LogFormat("window mode is on {0}",windowMode.isOn);
            if (val)
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
        }).AddTo(this);
        fullscreenMode.onValueChanged.AsObservable().Subscribe(val =>
        {
            Debug.Log("did fullscreen subscription fire");
            Debug.LogFormat("fullscreen mode is on {0}",fullscreenMode.isOn);
            if (val)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            }
        }).AddTo(this);

        resolutions.onValueChanged.AsObservable().Subscribe(val =>
        {
            var resText = resolutions.options[val].text;
            var newRes = resolutionsMap[resText];
            if (Screen.fullScreen)
            {
                Screen.SetResolution(newRes.x,newRes.y,FullScreenMode.ExclusiveFullScreen);
            }
            else
            {
                Screen.SetResolution(newRes.x,newRes.y,FullScreenMode.Windowed);
            }
            

        }).AddTo(this);

        accessibilities.OnClickAsObservable().Subscribe(val =>
        {
        accessibilitiesPopup.SetActive(true);
        }
        );

        controls.OnClickAsObservable().Subscribe(val =>
        #if !PLATFORM_ANDROID && !UNITY_ANDROID
        controlsPopup.SetActive(true)
        #else
        AdManager.Instance.ShowGDPRConsent()
        #endif
        );


    }
}
}