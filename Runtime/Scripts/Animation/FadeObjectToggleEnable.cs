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


    public class FadeObjectToggleEnable : AnimateObjectsToggleEnable
    {

        [SerializeField]
        CanvasGroup canvasGroup;
        [SerializeField]
        Image image;
        SpriteRenderer sprite;
        float endAlpha = 1;

        void Awake()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            image = GetComponentInChildren<Image>();
            if (image.color.a != 1 && image.color.a != 0)
            {
                endAlpha = image.color.a;
            }
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        
        public async override UniTask Disable(float duration=-1)
        {
            Debug.Log("who is calling me...");
            AnimationComplete = false;
            if (duration == -1)
            {
                duration = disableAnimationDuration;
            }
            if (canvasGroup != null)
            {

                await Easing.Create<Linear>(start: endAlpha, end: 0f, duration).ToColorA(canvasGroup);
                OnCompleteDisableAnimation();

            }

            else if (image != null)
            {
                await Easing.Create<Linear>(start: endAlpha, end: 0f, duration).ToColorA(image);
                OnCompleteDisableAnimation();
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
        public async override UniTask Enable(float duration=-1)
        {
            Debug.Log("running fade object toggle enable now");
            this.gameObject.SetActive(true);
            // Debug.Log(canvasGroup == null);
            AnimationComplete = false;
            if (duration == -1)
            {
                duration = enableAnimationDuration;
            }
            if (canvasGroup != null)
            {

                await Easing.Create<Linear>(start: 0f, end: endAlpha, duration).ToColorA(canvasGroup);
                OnCompleteEnableAnimation();

            }

            else if (image != null)
            {
                await Easing.Create<Linear>(start: 0f, end: endAlpha, duration).ToColorA(image);
                OnCompleteEnableAnimation();
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


        public override void OnCompleteDisableAnimation()
        {
            AnimationComplete = true;
            this.gameObject.SetActive(false);
            if (DestroyOnDisable)
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