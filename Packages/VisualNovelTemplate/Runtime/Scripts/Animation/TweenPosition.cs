using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;
using AnimeTask;
using Cysharp.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    public class TweenPosition : MonoBehaviour
    {
        [SerializeField]
        GameObject objToAnimate;
        [SerializeField]
        float enabledX, enabledY, enabledZ;
        [SerializeField]
        float disabledX, disabledY, disabledZ;
        [SerializeField]
        bool doTweenX = false, doTweenY = false, doTweenZ = false;
        // TODO:
        // When switching dotween ==> animeTask tweening library, need a way to select
        // easing type via inspector
        // [SerializeField]
        // AnimeTask.Easing ease = InQuart;
        [SerializeField]
        float enabledAnimationDuration = .5f, disabledAnimationDuration = .5f;
        public bool AnimationComplete = false;
        private CancellationTokenSource _source = new CancellationTokenSource();
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;
        List<UniTask> tasks = new List<UniTask>();
        void Start()
        {

            ct = cts.Token;
            if (objToAnimate == null)
            {
                objToAnimate = this.gameObject;
            }
        }
        public async UniTask Enable(float? dur = null)
        {
            tasks.Clear();
            Debug.Log("run enable");
            if (dur == null)
            {
                dur = enabledAnimationDuration;
            }
            if (doTweenX)
            {
                tasks.Add(Easing.Create<InQuart>(to: enabledX, duration: (float)dur).ToLocalPositionX(objToAnimate));
            }
            if (doTweenY)
            {
                tasks.Add(Easing.Create<InQuart>(to: enabledY, duration: (float)dur).ToLocalPositionY(objToAnimate));

            }
            if (doTweenZ)
            {
                tasks.Add(Easing.Create<InQuart>(to: enabledZ, duration: (float)dur).ToLocalPositionZ(objToAnimate));
            }
            await UniTask.WhenAll(tasks);

        }
        public async UniTask Disable(float? dur = null)
        {
            tasks.Clear();
            Debug.Log("run disable");
            if (dur == null)
            {
                dur = disabledAnimationDuration;
            }
            if (doTweenX)
            {
                tasks.Add(Easing.Create<InQuart>(to: disabledX, duration: (float)dur).ToLocalPositionX(objToAnimate));
            }
            if (doTweenY)
            {
                tasks.Add(Easing.Create<InQuart>(to: disabledY, duration: (float)dur).ToLocalPositionY(objToAnimate));
            }
            if (doTweenZ)
            {
                tasks.Add(Easing.Create<InQuart>(to: disabledZ, duration: (float)dur).ToLocalPositionZ(objToAnimate));
            }
            await UniTask.WhenAll(tasks);
        }
    }

}