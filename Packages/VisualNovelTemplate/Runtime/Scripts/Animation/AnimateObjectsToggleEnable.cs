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

        private CancellationTokenSource _source = new CancellationTokenSource();
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;
        void Start()
        {
            ct = cts.Token;
        }
        private void OnEnable()
        {
            Enable();
        }
        private async UniTaskVoid OnDisable()
        {
            await Disable();
            gameObject.SetActive(false);
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
        public abstract UniTask Enable();
        public abstract UniTask Disable();
        public abstract void OnCompleteEnableAnimation();
        public abstract void OnCompleteDisableAnimation();

        public virtual void ToggleState()
        {
            if (this.gameObject.activeSelf)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

    }

}