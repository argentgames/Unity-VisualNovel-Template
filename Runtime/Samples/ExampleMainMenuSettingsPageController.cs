using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleMainMenuSettingsPageController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("What should the fullscreen mode be? Don't select Windowed!!!")]
        FullScreenMode fullScreenMode = FullScreenMode.ExclusiveFullScreen;
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
            int width,height;
            var res = Screen.currentResolution;
            width = res.width;
            height = res.height;
            // we're only using exlcusive full screen, but you could expose every fs mode
            Screen.SetResolution(width,height,fullScreenMode);
        }
        public void SetWindowedMode(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            int width,height;
            var res = Screen.currentResolution;
            width = res.width;
            height = res.height;
            Screen.SetResolution(width,height,FullScreenMode.Windowed);
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
            Screen.SetResolution(1920,1080,Screen.fullScreenMode);
        }
        public void Set720p(System.Boolean val)
        {
            if (!val)
            {
                return;
            }
            Screen.SetResolution(1280,720,Screen.fullScreenMode);
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
