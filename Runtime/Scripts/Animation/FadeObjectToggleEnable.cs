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


    public class FadeObjectToggleEnable : AnimateObjectsToggleEnable
    {
        [SerializeField]
        float disableAnimationDuration = .5f, enableAnimationDuration = .5f;

        [SerializeField]
        CanvasGroup canvasGroup;
        [SerializeField]
        Image image;
        SpriteRenderer sprite;

        void Awake()
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
            image = GetComponentInChildren<Image>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        public async override UniTask Disable()
        {
            Debug.Log("who is calling me...");
            AnimationComplete = false;
            if (canvasGroup != null)
            {

                canvasGroup.DOFade(0, disableAnimationDuration)
                .OnComplete(OnCompleteDisableAnimation);

            }

            else if (image != null)
            {
                image.DOFade(0, disableAnimationDuration)
            .OnComplete(OnCompleteDisableAnimation);
            }
            else if (sprite != null)
            {
                if (sprite.material.HasProperty("AlphaAmount"))
                {
                    sprite.material.DOFloat(0, "AlphaAmount", disableAnimationDuration).OnComplete(OnCompleteDisableAnimation);
                }
                else
                {
                    sprite.DOFade(0, disableAnimationDuration)
                            .OnComplete(OnCompleteDisableAnimation);
                }

            }
            else
            {
                Debug.LogWarning("No image or canvasGroup or spriterenderr component found, and gameObject fading not supported yet.");
            }

            // await UniTask.WaitUntil(() => AnimationComplete);
        }
        public async override UniTask Enable()
        {
            // Debug.Log(canvasGroup == null);
            AnimationComplete = false;
            if (canvasGroup != null)
            {

                canvasGroup.DOFade(1, enableAnimationDuration)
                .OnComplete(OnCompleteEnableAnimation);

            }

            else if (image != null)
            {
                image.DOFade(1, enableAnimationDuration)
            .OnComplete(OnCompleteEnableAnimation);
            }
            else if (sprite != null)
            {
                if (sprite.material.HasProperty("AlphaAmount"))
                {
                    sprite.material.DOFloat(1, "AlphaAmount", enableAnimationDuration).OnComplete(OnCompleteEnableAnimation);
                }
                else
                {
                    sprite.DOFade(1, enableAnimationDuration)
                            .OnComplete(OnCompleteEnableAnimation);
                }
            }
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