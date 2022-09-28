using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    public class Preload : MonoBehaviour
    {
        [SerializeField]
        float delayBeforeLoadScene = .2f;
        [Scene]
        public string sceneToLoad = "MainMenu";
        // Start is called before the first frame update
        async UniTaskVoid Awake()
        {
            await UniTask.WaitUntil(() => Manager.allManagersLoaded.Value);
            SceneTransitionManager.Instance.FadeToBlack(0);
        }
        async UniTaskVoid Start()
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delayBeforeLoadScene));
            SceneTransitionManager.Instance.LoadScene(sceneToLoad,UnityEngine.SceneManagement.LoadSceneMode.Single);
            SceneTransitionManager.Instance.FadeIn(0);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
