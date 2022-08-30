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
    float disableAnimationDuration = .5f, enableAnimationDuration = .5f;
    [SerializeField]
    Vector3 enableEndPosition, disableEndPosition;
    // Start is called before the first frame update



    [Button]
    public async override UniTask Disable()
    {
        await Easing.Create<InQuad>(to: disableEndPosition, duration: disableAnimationDuration).ToLocalPosition(transform);
        OnCompleteDisableAnimation();

    }

    [Button]
    public async override UniTask Enable()
    {

        await Easing.Create<InQuad>(to: enableEndPosition, duration: enableAnimationDuration).ToLocalPosition(transform);
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