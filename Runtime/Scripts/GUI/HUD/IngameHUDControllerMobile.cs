using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace com.argentgames.visualnoveltemplate
{

    public class IngameHUDControllerMobile : MonoBehaviour
    {

        [SerializeField]
        Button settings, save, history, load;

    [SerializeField]
    Button hideUI;

        [SerializeField]
        public Toggle menuVisibility, skip, auto;
        [SerializeField]
        GameObject menuWrapper;
        IngameHUDLogic logic;
        [SerializeField]
        CanvasGroup menuWrapperHolder;
        public TweenPosition tweenPosition;

        // Start is called before the first frame update
        async UniTaskVoid Awake()
        {
             // wait for all our managers to exist because we are subscribing to their values
            await UniTask.WaitUntil(() => Manager.allManagersLoaded.Value);

            // GameManager.Instance.ingameHUDPresenter = this;

            logic = GetComponent<IngameHUDLogic>();

            await UniTask.Yield();

            SetRXSubscriptions();
        }

        void Start()
        {

        }

        void SetRXSubscriptions()
        {
            Debug.Log("setting ingame hud rx subs");
            settings.OnClickAsObservable()
                .Subscribe(_ =>
               {
                   MenuManager.Instance.OpenPage("ingameSettings");
               }
                )
                .AddTo(this);
            save.OnClickAsObservable()
                .Subscribe(_ =>
               {
                   MenuManager.Instance.OpenPage("ingameSettings", "save");
               }
                )
                .AddTo(this);
            load.OnClickAsObservable()
                .Subscribe(_ =>
               {
                   MenuManager.Instance.OpenPage("ingameSettings", "load");
               }
                )
                .AddTo(this);
            history.OnClickAsObservable()
                .Subscribe(_ =>
               {
                   MenuManager.Instance.OpenPage("ingameSettings", "history");
               }
                )
                .AddTo(this);
            menuVisibility.onValueChanged.AsObservable().Subscribe(val =>
            {
                ToggleMenuOpen(val);
            });
            skip.onValueChanged.AsObservable().Subscribe(val =>
            {
                GameManager.Instance.SetSkipping(val);
                if (val)
                {
                    DialogueSystemManager.Instance.InkContinueStory();
                }
            });
            auto.onValueChanged.AsObservable().Subscribe(val =>
            {
                GameManager.Instance.SetAuto(val);
                if (val)
                {
                    if (!DialogueSystemManager.Instance.IsDisplayingLine)
                    {
                        DialogueSystemManager.Instance.InkContinueStory();
                    }
                }
            });

Debug.LogFormat("hideUI is null? {0}", hideUI);
        hideUI.OnClickAsObservable().Subscribe(val =>
        {
            GameManager.Instance.SetSkipping(false);
            GameManager.Instance.SetAuto(false);
            if (DialogueSystemManager.Instance.IsDisplayingLine)
            {
                // DialogueSystemManager.Instance.dialogueUIManager.KillTypewriter();
            }
            Debug.Log("calling toggle ui");
            // DialogueSystemManager.Instance.dialogueUIManager.ToggleUI();
            
        });

        }

        public void ToggleMenuOpen(bool val)
        {

        Debug.Log("run tween position");
        if (val)
        {
            tweenPosition.Enable();
        }
        else
        {
            tweenPosition.Disable();
        }

        }
    }

}