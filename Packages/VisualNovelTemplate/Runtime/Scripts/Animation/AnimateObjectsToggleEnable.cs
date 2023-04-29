using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    [DisallowMultipleComponent]
    public abstract class AnimateObjectsToggleEnable : MonoBehaviour
    {
        [SerializeField]
        public bool DestroyOnDisable = false;
        public bool AnimationComplete = false;
        [HideInInspector]
        public Color transparent = new Color(255, 255, 255, 0);

        public float disableAnimationDuration = .5f, enableAnimationDuration = .5f;
        public bool IsRunningEnableAnimation = false;
        public bool IsRunningDisableAnimation = false;
        CancellationTokenSource cts;
        CancellationToken ct;
        void Start()
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
        private void OnEnable()
        {
            // Enable(enableAnimationDuration);
        }
        private async UniTaskVoid OnDisable()
        {
            // await Disable(disableAnimationDuration);
            // gameObject.SetActive(false);
        }

        public void SetAllImagesTransparent()
        {
            foreach (var component in gameObject.transform.GetComponentsInChildren<Image>())
            {
                component.color = transparent;
            }
            var image = gameObject.GetComponent<Image>();
            if (image != null)
            {
                image.color = transparent;
            }
        }
  
        public abstract UniTask Enable(float duration);
        public abstract UniTask Disable(float duration, bool destroyOnDisable=false);
        public abstract void OnCompleteEnableAnimation();
        public abstract void OnCompleteDisableAnimation(bool destroyOnDisable=false);
        public abstract void SkipEnableAnimation();
        public abstract void SkipDisableAnimation();
        public virtual void CompleteAnimation()
        {
            AnimationComplete = true;
            IsRunningDisableAnimation = false;
            IsRunningEnableAnimation = false;
        }

        public virtual void ToggleState()
        {
            if (this.gameObject.activeSelf)
            {
                Disable(disableAnimationDuration);
            }
            else
            {
                Enable(enableAnimationDuration);
            }
        }

    }

}