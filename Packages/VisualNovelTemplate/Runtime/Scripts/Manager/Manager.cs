using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
namespace com.argentgames.visualnoveltemplate
{
    public class Manager : MonoBehaviour
    {
        public static BoolReactiveProperty allManagersLoaded = new BoolReactiveProperty(false);
        
        async UniTaskVoid Awake()
        {
            // TODO: add in bools for managers to say they are done loading, not just for them to have an instance!!!
            await UniTask.WaitUntil(() =>
            
                AudioManager.Instance != null &&
                DialogueSystemManager.Instance != null &&
                GameManager.Instance != null &&
                SaveLoadManager.Instance != null &&
                SceneTransitionManager.Instance != null &&
                MenuManager.Instance != null &&
                VideoPlayerManager.Instance != null &&
                ImageManager.Instance != null

            );
            #if UNITY_ANDROID || UNITY_IOS
            await UniTask.WaitUntil(() => AdManager.Instance != null);
            #endif
            allManagersLoaded.Value = true;
        }
    }
}
