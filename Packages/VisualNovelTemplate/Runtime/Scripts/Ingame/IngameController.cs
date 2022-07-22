using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Active game scene controller. Knows when to call ContinueStory from the DS and 
    /// how to wait for any current save to load before doing so.
    /// 
    /// Pretty much just calls StartDialogue with the appropriate node.
    /// </summary>
    public class IngameController : MonoBehaviour
    {
        public float? fadeInDuration = null;
        async UniTaskVoid Awake()
        {
            // don't let user interact with anything  until we are done settin gup the scene!
            MenuManager.Instance.DisableSettingsUIControls();
            // TODO: when we have save load working again, we do need
            // to wait for the scene to get set up before we hide the scene transition screen
            await UniTask.Yield();
        }
        // Start is called before the first frame update
        async UniTaskVoid Start()
        {
            DialogueSystemManager.Instance.RunStory().Forget();
        }

    }
}
