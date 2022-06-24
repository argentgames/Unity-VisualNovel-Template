using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
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
    [SerializeField]
    Ease ease = Ease.InQuad;
    [SerializeField]
    float enabledAnimationDuration = .5f, disabledAnimationDuration = .5f;
    public bool AnimationComplete = false;
         private CancellationTokenSource _source = new CancellationTokenSource();
    CancellationTokenSource cts = new CancellationTokenSource();
    CancellationToken ct;
    Sequence sequence;
    void Start()
    {
        
        ct = cts.Token;
        if (objToAnimate == null)
        {
            objToAnimate = this.gameObject;
        }
    }
    public void Enable(float? dur=null)
    {
        sequence = DOTween.Sequence();
        Debug.Log("run enable");
        if (dur == null)
        {
            dur = enabledAnimationDuration;
        }
        if (doTweenX)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveX(enabledX,(float)dur).SetEase(ease));
        }
        if (doTweenY)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveY(enabledY,(float)dur).SetEase(ease));
        }
        if (doTweenZ)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveZ(enabledZ,(float)dur).SetEase(ease));
        }
        sequence.Play();

    }
    public void Disable(float? dur=null)
    {
        sequence = DOTween.Sequence();
        Debug.Log("run disable");
        if (dur == null)
        {
            dur = disabledAnimationDuration;
        }
        if (doTweenX)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveX(disabledX,(float)dur).SetEase(ease));
        }
        if (doTweenY)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveY(disabledY,(float)dur).SetEase(ease));
        }
        if (doTweenZ)
        {
            sequence.Join(objToAnimate.transform.DOLocalMoveZ(disabledZ,(float)dur).SetEase(ease));
        }
        sequence.Play();
    }
}
