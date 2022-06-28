using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{
public class CharacterCustomizationLogic : MonoBehaviour
{
    // Start is called before the first frame update
    async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
        MenuManager.Instance.EnableSettingsUIControls();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
}