using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using AnimeTask;
using UnityEngine.Events;
namespace com.argentgames.visualnoveltemplate
{


    public class FadeObjectToggleEnable : AnimateObjectsToggleEnable
    {

        [SerializeField]
        CanvasGroup canvasGroup;
        [SerializeField]
        Image image;
        SpriteRenderer sprite;
        float endAlpha = 1;

        [SerializeField]
        AnimationCurve transitionInCurve = AnimationCurve.Linear(0, 0, 1, 1), transitionOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        Coroutine animate;
        public UnityEvent OnAnimationStart, OnAnimationComplete;
        void Awake()
        {
            // sprite = GetComponentInChildren<SpriteRenderer>();
            // image = GetComponentInChildren<Image>();
            // Debug.Log("current alpha a value: " + image.color.a.ToString());
            if (image != null)
            {
                if (image.color.a != 1 && image.color.a != 0)
                {
                    endAlpha = image.color.a;
                }
            }
            // canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        public override void CompleteAnimation()
        {
            base.CompleteAnimation();
            if (animate != null)
            {
                // TECHDEBT: SAFETY??? sometiems the coroutine just dies before completing animation?
                if (IsRunningEnableAnimation)
                {
                    SkipEnableAnimation();
                }
                else if (IsRunningDisableAnimation)
                {
                    SkipDisableAnimation();
                }
                // StopCoroutine(animate);
                // animate = null;
            }
        }
        public override void SkipDisableAnimation()
        {
            if (canvasGroup != null)
            {
                Debug.Log("skipping disable animation through canvas group");
                canvasGroup.alpha = 0;
            }
            if (image != null)
            {
                Debug.Log("skipping disable animation through image alpha");
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            }

        }
        public override void SkipEnableAnimation()
        {
            if (canvasGroup != null)
            {
                Debug.Log("skipping enable animation through canvas group");
                canvasGroup.alpha = endAlpha;
            }
            if (image != null)
            {
                Debug.Log("skipping enable animation through image alpha");
                image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            }
        }
        IEnumerator I_FadeInCanvasGroup(float duration = -1)
        {
            if (duration == -1)
            {
                duration = enableAnimationDuration;
            }
            float elapsedTime = 0;
            if (duration == 0)
            {
                canvasGroup.alpha = endAlpha;
            }
            else
            {
                while (elapsedTime < duration &&
            canvasGroup.alpha != endAlpha)
                {
                    elapsedTime += Time.deltaTime;
                    // Debug.Log("fading in");
                    float percent = Mathf.Clamp01(canvasGroup.alpha / duration);


                    float curvePercent = transitionInCurve.Evaluate(elapsedTime / duration);
                    canvasGroup.alpha = curvePercent * endAlpha;
                    // Debug.LogFormat("fading in canvas group: {0}", curvePercent);
                    yield return null;
                }
            }

        }
        IEnumerator I_FadeInImage(float duration = -1)
        {
            if (duration == -1)
            {
                duration = enableAnimationDuration;
            }
            float elapsedTime = 0;
            Color targetColor = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
            if (duration == 0)
            {
                image.color = targetColor;
                yield break;
            }
            while (elapsedTime < duration &&
            image.color != targetColor)
            {
                elapsedTime += Time.deltaTime;
                // Debug.Log("fading in");
                float percent = Mathf.Clamp01(image.color.a / duration);


                float curvePercent = transitionInCurve.Evaluate(elapsedTime / duration);

                image.color = Color.LerpUnclamped(image.color, targetColor, curvePercent);
                Debug.LogFormat("targetColor {0} curvePercent {1}", targetColor, curvePercent);
                yield return null;
            }
        }
        IEnumerator I_FadeOutCanvasGroup(float duration = -1)
        {
            if (duration == -1)
            {
                duration = disableAnimationDuration;
            }
            float elapsedTime = 0;
            var startAlpha = canvasGroup.alpha;
            if (duration == 0)
            {
                canvasGroup.alpha = 0;
                yield break;
            }
            while (elapsedTime < disableAnimationDuration &&
            canvasGroup.alpha > 0)
            {
                elapsedTime += Time.deltaTime;
                // Debug.Log("fading in");
                float percent = Mathf.Clamp01(canvasGroup.alpha / disableAnimationDuration);


                float curvePercent = transitionInCurve.Evaluate(elapsedTime / disableAnimationDuration);
                canvasGroup.alpha -= startAlpha * curvePercent;
                Debug.LogFormat("fading out canvas group: {0}", curvePercent);
                yield return null;
            }
        }
        IEnumerator I_FadeOutImage(float duration = -1)
        {
            if (duration == -1)
            {
                duration = disableAnimationDuration;
            }
            float elapsedTime = 0;
            Color targetColor = new Color(image.color.r, image.color.g, image.color.b, 0);
            if (duration == 0)
            {
                image.color = targetColor;
                yield break;
            }
            Debug.LogFormat("elapsedTime {0}, disableAnimationDuration {1}, image.color {2}, targetColor {3}",
            elapsedTime,disableAnimationDuration,image.color,targetColor);
            while (elapsedTime < disableAnimationDuration ||
            image.color != targetColor)
            {
                elapsedTime += Time.deltaTime;
                // Debug.Log("fading in");
                float percent = Mathf.Clamp01(image.color.a / disableAnimationDuration);


                float curvePercent = transitionInCurve.Evaluate(elapsedTime / disableAnimationDuration);

                image.color = Color.LerpUnclamped(image.color, targetColor, curvePercent);
                Debug.LogFormat("targetColor {0} curvePercent {1}", targetColor, curvePercent);
                yield return null;
                Debug.LogFormat("elapsedTime {0}, disableAnimationDuration {1}, image.color {2}, targetColor {3}",
            elapsedTime,disableAnimationDuration,image.color,targetColor);
            }
        }

        public async override UniTask Disable(float duration = -1, bool destroyOnDisable = false)
        {
            try
            {
                Debug.Log("who is calling me...");
                if (image != null)
                {
                    if (image.color.a != 1 && image.color.a != 0)
                    {
                        endAlpha = image.color.a;
                    }
                }

                AnimationComplete = false;
                IsRunningDisableAnimation = true;
                if (duration == -1)
                {
                    duration = disableAnimationDuration;
                }
                Debug.LogFormat("disable duration: {0}", duration);

                if (canvasGroup != null)
                {

                    // Easing.Create<Linear>(start: endAlpha, end: 0f, duration).ToColorA(canvasGroup, skipToken: GameManager.Instance.SkipToken);
                    if (this.gameObject.activeSelf
                    && Utilities.GetRootParent(this.gameObject).activeSelf
                    && duration != 0)
                    {
                        animate = StartCoroutine(I_FadeOutCanvasGroup(duration));
                    }
                    else
                    {
                        canvasGroup.alpha = 0;
                    }

                    await UniTask.WaitUntil(() => canvasGroup.alpha == 0);
                    // CompleteAnimation();
                    // OnCompleteDisableAnimation(destroyOnDisable);

                }

                else if (image != null)
                {
                    // Easing.Create<Linear>(start: endAlpha, end: 0f, duration).ToColorA(image, skipToken: GameManager.Instance.SkipToken);
                    if (this.gameObject.activeSelf
                    && Utilities.GetRootParent(this.gameObject).activeSelf
                    && duration != 0)
                    {
                        animate = StartCoroutine(I_FadeOutImage(duration));
                    }
                    else
                    {
                        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
                    }

                    await UniTask.WaitUntil(() => image.color.a == 0);
                    // CompleteAnimation();
                    // OnCompleteDisableAnimation(destroyOnDisable);
                }
                // else if (sprite != null)
                // {
                //     if (sprite.material.HasProperty("AlphaAmount"))
                //     {
                //         sprite.material.DOFloat(0, "AlphaAmount", disableAnimationDuration).OnComplete(OnCompleteDisableAnimation);
                //     }
                //     else
                //     {
                //         sprite.DOFade(0, disableAnimationDuration)
                //                 .OnComplete(OnCompleteDisableAnimation);
                //     }

                // }
                else
                {
                    Debug.LogWarning("No image or canvasGroup or spriterenderr component found, and gameObject fading not supported yet.");
                }

                // await UniTask.WaitUntil(() => AnimationComplete);
            }
            catch (System.Exception e)
            {
                Debug.LogWarningFormat("failed to run fade object toggle enable's DISABLE function from gameObject: {0}, {1}", gameObject.name, e);
            }

            CompleteAnimation();
            OnCompleteDisableAnimation(destroyOnDisable);

        }
        public async override UniTask Enable(float duration = -1)
        {
            try
            {
                Debug.LogFormat("running fade object toggle enable now with duration {0}", duration);
                this.gameObject.SetActive(true);
                // Debug.Log(canvasGroup == null);
                AnimationComplete = false;
                IsRunningEnableAnimation = true;
                // Debug.Log("current alpha a value: " + image.color.a.ToString());
                if (image != null)
                {
                    if (image.color.a != 1 && image.color.a != 0)
                    {
                        endAlpha = image.color.a;
                    }
                }
                if (duration == -1)
                {
                    duration = enableAnimationDuration;
                }

                if (canvasGroup != null)
                {
                    // Easing.Create<Linear>(start: 0f, end: endAlpha, duration).ToColorA(canvasGroup, skipToken: GameManager.Instance.SkipToken);
                    if (this.gameObject.activeSelf 
                    && Utilities.GetRootParent(this.gameObject).activeSelf
                    && duration != 0)
                    {
                        animate = StartCoroutine(I_FadeInCanvasGroup(duration));
                    }
                    else
                    {
                        canvasGroup.alpha = endAlpha;
                    }

                    await UniTask.WaitUntil(() => canvasGroup.alpha == endAlpha);
                    // CompleteAnimation();
                    // OnCompleteEnableAnimation();

                }

                else if (image != null)
                {
                    // Easing.Create<Linear>(start: 0f, end: endAlpha, duration).ToColorA(image, skipToken: GameManager.Instance.SkipToken);
                    if (this.gameObject.activeSelf
                    && Utilities.GetRootParent(this.gameObject).activeSelf
                    && duration != 0)
                    {
                        animate = StartCoroutine(I_FadeInImage(duration));
                    }
                    else
                    {
                        image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
                    }

                    await UniTask.WaitUntil(() => image.color.a == endAlpha);
                    // CompleteAnimation();
                    // OnCompleteEnableAnimation();
                }
                // else if (sprite != null)
                // {
                //     if (sprite.material.HasProperty("AlphaAmount"))
                //     {
                //         sprite.material.DOFloat(1, "AlphaAmount", enableAnimationDuration).OnComplete(OnCompleteEnableAnimation);
                //     }
                //     else
                //     {
                //         sprite.DOFade(1, enableAnimationDuration)
                //                 .OnComplete(OnCompleteEnableAnimation);
                //     }
                // }
                else
                {
                    Debug.LogWarning("No image or canvasGroup or spriterenderr component found, and gameObject fading not supported yet.");
                }


                // await UniTask.WaitUntil(() => AnimationComplete);
            }
            catch (System.Exception e)
            {
                Debug.LogWarningFormat("failed to run fade object toggle enable's ENABLE function from gameObject: {0}, {1}", gameObject.name, e);
            }

            CompleteAnimation();
            OnCompleteEnableAnimation();

        }


        public override void OnCompleteDisableAnimation(bool destroyOnDisable = false)
        {
            AnimationComplete = true;
            this.gameObject.SetActive(false);
            if (destroyOnDisable)
            {
                Destroy(this.gameObject);
            }
        }
        public override void OnCompleteEnableAnimation()
        {
            AnimationComplete = true;
        }

    }

}