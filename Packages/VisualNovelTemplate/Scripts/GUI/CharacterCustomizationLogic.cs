using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CharacterCustomizationLogic : MonoBehaviour
{
    // Start is called before the first frame update
    async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
        SettingsManager.Instance.EnableSettingsUIControls();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
