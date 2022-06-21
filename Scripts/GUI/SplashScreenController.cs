using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
public class SplashScreenController : MonoBehaviour
{
    
    async UniTaskVoid Start()
    {
        
        await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
        SceneTransitionManager.Instance.LoadScene("MainMenu",1.5f,1.5f);
        SaveLoadManager.Instance.LoadSaveFiles().Forget();
        AudioManager.Instance.GetFMODBuses();
    }
}
