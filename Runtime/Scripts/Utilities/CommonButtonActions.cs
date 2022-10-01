using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;
using NaughtyAttributes;

namespace com.argentgames.visualnoveltemplate
{
    public class CommonButtonActions : MonoBehaviour
    {
        [Scene]
        [SerializeField]
        string sceneName;
        [SerializeField]
        bool doStopSound = true;
        public void LoadScene()
        {
            SceneTransitionManager.Instance.LoadScene(sceneName,doStopSound: doStopSound);
        }
        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
    }
}
