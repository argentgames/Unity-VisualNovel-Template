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

    public class IngameHUDControllerPC : MonoBehaviour
    {

        [SerializeField]
        Button settings, save, history, load;

        [SerializeField]
        public Toggle menuVisibility, skip, auto;
        [SerializeField]
        GameObject menuWrapper;
        [SerializeField]
        CanvasGroup menuWrapperHolder;

        // Start is called before the first frame update
        async UniTaskVoid Awake()
        {
            // wait for all our managers to exist because we are subscribing to their values
            await UniTask.WaitUntil(() => MenuManager.Instance != null);
            await UniTask.WaitUntil(() => GameManager.Instance != null);
            await UniTask.WaitUntil(() => DialogueSystemManager.Instance != null);

            // GameManager.Instance.ingameHUDPresenter = this;

            menuVisibility.SetIsOnWithoutNotify(false);
            menuWrapperHolder.DOFade(0, 0f).OnComplete(() => menuWrapper.SetActive(false));


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


        }

        public void ToggleMenuOpen(bool val)
        {

            if (val)
            {
                menuWrapper.SetActive(true);
                menuWrapperHolder.DOFade(1, .35f);

            }
            else
            {
                menuWrapperHolder.DOFade(0, .35f).OnComplete(() => menuWrapper.SetActive(false));

            }

        }
    }

}