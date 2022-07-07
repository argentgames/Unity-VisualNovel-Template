using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

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
        AnimationComplete = false;
        this.transform.DOLocalMove(disableEndPosition, disableAnimationDuration).SetEase(Ease.InQuad).OnComplete(OnCompleteDisableAnimation);
        await UniTask.WaitUntil(() => AnimationComplete);

    }

    [Button]
    public async override UniTask Enable()
    {

        AnimationComplete = false;
        this.transform.DOLocalMove(enableEndPosition, enableAnimationDuration).SetEase(Ease.InQuad).OnComplete(OnCompleteEnableAnimation);
        await UniTask.WaitUntil(() => AnimationComplete);
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