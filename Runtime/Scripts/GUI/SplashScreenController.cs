using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace com.argentgames.visualnoveltemplate
{
public class SplashScreenController : MonoBehaviour
{
    [SerializeField]
    AnimateObjectsToggleEnable animateObjectsToggleEnable;

    void Awake()
    {
        SaveLoadManager.Instance.LoadSaveFiles().Forget();
        AudioManager.Instance.GetFMODBuses();
    }
    
    async UniTaskVoid Start()
    {
        
        await animateObjectsToggleEnable.Disable(-1);
        await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
        SceneTransitionManager.Instance.LoadScene("MainMenu",1.5f,1.5f);
        
    }
}
}