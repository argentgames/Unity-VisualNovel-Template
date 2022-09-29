using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using TMPro;
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleSettingsPage : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("What should the fullscreen mode be? Don't select Windowed!!!")]
        FullScreenMode fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        /// <summary>
        /// Resolution format WxH. Order used to generate dropdown resolution options
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        [SerializeField]
        List<string> resolutionsOrder = new List<string>();
        Dictionary<string,int>resolutionsMap = new Dictionary<string, int>();
        [SerializeField]
        TMP_Dropdown resolutionsDropdown;

        [SerializeField]
        Slider masterVolume, ambientVolume, musicVolume, sfxVolume, textSpeed, autoSpeed;
        [SerializeField]
        Toggle skipAllText, fullscreen;
        private void Awake()
        {
            if (resolutionsDropdown != null)
            {
                resolutionsDropdown.ClearOptions();
            resolutionsDropdown.AddOptions(resolutionsOrder);
            }
            for (int i=0; i < resolutionsOrder.Count;i++)
            {
                resolutionsMap[resolutionsOrder[i]] = i;
            }
            



        }
        void OnEnable()
        {
            if (masterVolume != null)
            {
                
                masterVolume.value = GameManager.Instance.Settings.MasterVolume.Value;
            }
            if (ambientVolume != null)
            {
                ambientVolume.value = GameManager.Instance.Settings.AmbientVolume.Value;
            }
            if (musicVolume != null)
            {
                Debug.Log("current music volume: " + GameManager.Instance.Settings.MusicVolume.Value.ToString());
                musicVolume.value = GameManager.Instance.Settings.MusicVolume.Value;
            }
            if (sfxVolume != null)
            {
                sfxVolume.value = GameManager.Instance.Settings.SFXVolume.Value;
            }
            if (textSpeed != null)
            {
                textSpeed.value = GameManager.Instance.Settings.TextSpeed.Value;
            }
            if (autoSpeed != null)
            {
                autoSpeed.value = GameManager.Instance.Settings.AutoSpeed.Value;
            }
            if (skipAllText != null)
            {
                skipAllText.isOn = GameManager.Instance.Settings.skipAllText;
            }
            if (fullscreen != null)
            {
                fullscreen.isOn = Screen.fullScreen;
            }
        }
        private void Start()
        {
            // set init values in Start so any toggle extensions can register first
            if (masterVolume != null)
            {
                
                masterVolume.value = GameManager.Instance.Settings.MasterVolume.Value;
            }
            if (ambientVolume != null)
            {
                ambientVolume.value = GameManager.Instance.Settings.AmbientVolume.Value;
            }
            if (musicVolume != null)
            {
                Debug.Log("current music volume: " + GameManager.Instance.Settings.MusicVolume.Value.ToString());
                musicVolume.value = GameManager.Instance.Settings.MusicVolume.Value;
            }
            if (sfxVolume != null)
            {
                sfxVolume.value = GameManager.Instance.Settings.SFXVolume.Value;
            }
            if (textSpeed != null)
            {
                textSpeed.value = GameManager.Instance.Settings.TextSpeed.Value;
            }
            if (autoSpeed != null)
            {
                autoSpeed.value = GameManager.Instance.Settings.AutoSpeed.Value;
            }
            if (skipAllText != null)
            {
                skipAllText.isOn = GameManager.Instance.Settings.skipAllText;
            }
            if (fullscreen != null)
            {
                fullscreen.isOn = Screen.fullScreen;
            }

            // if resolutions isn't null, then set current resolutions value
            if (resolutionsDropdown != null)
            {
                var currentResolution = Screen.currentResolution;
                var res = string.Format("{0}x{1}",currentResolution.width,currentResolution.height);
                try
                {
                    SetResolutionDropdown(resolutionsMap[res]);
                }
                catch
                {
                    Debug.LogWarningFormat("trying to set an unsupportred resolution~! {0}",res);
                }
                
            }
        }
        public void UpdateMusicVolume(System.Single val)
        {
            GameManager.Instance.Settings.MusicVolume.Value = val;
        }
        public void UpdateSFXVolume(System.Single val)
        {
            GameManager.Instance.Settings.SFXVolume.Value = val;
        }
        public void UpdateTextSpeed(System.Single val)
        {
            GameManager.Instance.Settings.TextSpeed.Value = val;
        }
        public void UpdateAutoSpeed(System.Single val)
        {
            GameManager.Instance.Settings.AutoSpeed.Value = val;
        }
        public void SetFullscreenMode(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            int width, height;
            var res = Screen.currentResolution;
            width = res.width;
            height = res.height;
            // we're only using exlcusive full screen, but you could expose every fs mode
            Screen.SetResolution(width, height, fullScreenMode);
        }
        public void SetWindowedMode(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            int width, height;
            var res = Screen.currentResolution;
            width = res.width;
            height = res.height;
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
        }
        public void SetResolutionDropdown(System.Int32 index)
        {
            if (index > resolutionsOrder.Count)
            {
                Debug.LogErrorFormat("trying to set a dropdown resolution that doesn't exist in the list of available resolutions!");
            }
            else
            {
                var res = StringExtensions.ParseResolution(resolutionsOrder[index]);
                if (res != null)
                {
                    Screen.SetResolution(res.Item1, res.Item2, Screen.fullScreenMode);
                }
                else
                {
                    Debug.LogErrorFormat("unable to set resolution dropdown to index {0} due to parseResolution failure", index);
                }
            }
        }
        public void SkipAllText(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            GameManager.Instance.Settings.skipAllText = true;
        }
        public void SkipSeenText(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            GameManager.Instance.Settings.skipAllText = false;
        }
        public void Set1080p(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            Screen.SetResolution(1920, 1080, Screen.fullScreenMode);
        }
        public void Set720p(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            Screen.SetResolution(1280, 720, Screen.fullScreenMode);
        }


        void OnDisable()
        {
            SaveLoadManager.Instance.SaveSettings();
        }
        void OnDestroy()
        {
            SaveLoadManager.Instance.SaveSettings();
        }
    }
}
