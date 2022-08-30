using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using AnimeTask;
namespace com.argentgames.visualnoveltemplate
{
public class SlideObjectToggleEnable : AnimateObjectsToggleEnable
{
    [SerializeField]
    Vector3 enableEndPosition, disableEndPosition;
    // Start is called before the first frame update



    [Button]
    public async override UniTask Disable(float duration=-1)
    {
        if (duration == -1)
        {
            duration = disableAnimationDuration;
        }
        await Easing.Create<InQuad>(to: disableEndPosition, duration: duration).ToLocalPosition(transform);
        OnCompleteDisableAnimation();

    }

    [Button]
    public async override UniTask Enable(float duration =-1)
    {
        if (duration == -1)
        {
            duration = enableAnimationDuration;
        }

        await Easing.Create<InQuad>(to: enableEndPosition, duration: duration).ToLocalPosition(transform);
        OnCompleteEnableAnimation();
    }

    public override void OnCompleteEnableAnimation()
    {
        AnimationComplete = true;
    }
    public override void OnCompleteDisableAnimation()
    {
        AnimationComplete = true;
    }

}

}