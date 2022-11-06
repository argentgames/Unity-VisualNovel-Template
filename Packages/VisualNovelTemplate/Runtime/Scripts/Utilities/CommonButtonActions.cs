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
        [SerializeField]
        GameObject setGOEnableState;
        public void LoadScene()
        {
            SceneTransitionManager.Instance.LoadScene(sceneName,doStopSound: doStopSound);
        }
        public void OpenURL(string url)
        {
            Application.OpenURL(url);
        }
        public void SetGameObjectEnableState(bool state)
        {
            setGOEnableState.SetActive(state);
        }
        public void DisableGameObject(bool state)
        {
            setGOEnableState.SetActive(false);
        }
        public void DisableGameObject()
        {
            setGOEnableState.SetActive(false);
        }
        public void EnableGameObject(bool state)
        {
            setGOEnableState.SetActive(true);
        }
        public void EnableGameObject()
        {
            setGOEnableState.SetActive(true);
        }
    }
}
