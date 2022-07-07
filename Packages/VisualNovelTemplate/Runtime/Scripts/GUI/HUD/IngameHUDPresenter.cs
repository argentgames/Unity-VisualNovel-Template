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


public class IngameHUDPresenter : MonoBehaviour
{

    [SerializeField]
    Button settings, save, history, load;
#if PLATFORM_ANDROID
    [SerializeField]
    Button hideUI;
#endif
    [SerializeField]
    public Toggle menuVisibility, skip, auto;
    [SerializeField]
    GameObject menuWrapper;
    IngameHUDLogic logic;
    [SerializeField]
    CanvasGroup menuWrapperHolder;
#if PLATFORM_ANDROID || UNITY_ANDROID
    public TweenPosition tweenPosition;
#endif
    // Start is called before the first frame update
    async UniTaskVoid Awake()
    {
        await UniTask.WaitUntil(() => MenuManager.Instance != null);
        await UniTask.WaitUntil(() => GameManager.Instance != null);
        await UniTask.WaitUntil(() => DialogueSystemManager.Instance != null);

        GameManager.Instance.ingameHUDPresenter = this;

        logic = GetComponent<IngameHUDLogic>();
        
#if !PLATFORM_ANDROID && !UNITY_ANDROID
        menuVisibility.SetIsOnWithoutNotify(false);
        menuWrapperHolder.DOFade(0,0f).OnComplete(() => menuWrapper.SetActive(false));
#endif
await UniTask.Yield();
#if PLATFORM_ANDROID
await UniTask.WaitWhile(() => hideUI == null); // TECHDEBT: this is dumb? it's a hard ref so it should never be null???
#endif
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
               MenuManager.Instance.OpenPage("ingameSettings","save");
           }
            )
            .AddTo(this);
        load.OnClickAsObservable()
            .Subscribe(_ =>
           {
               MenuManager.Instance.OpenPage("ingameSettings","load");
           }
            )
            .AddTo(this);
        history.OnClickAsObservable()
            .Subscribe(_ =>
           {
               MenuManager.Instance.OpenPage("ingameSettings","history");
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

#if PLATFORM_ANDROID
Debug.LogFormat("hideUI is null? {0}", hideUI);
        hideUI.OnClickAsObservable().Subscribe(val =>
        {
            GameManager.Instance.SetSkipping(false);
            GameManager.Instance.SetAuto(false);
            if (DialogueSystemManager.Instance.IsDisplayingLine)
            {
                DialogueSystemManager.Instance.dialogueUIManager.KillTypewriter();
            }
            Debug.Log("calling toggle ui");
            DialogueSystemManager.Instance.dialogueUIManager.ToggleUI();
            
        });
#endif
    }

    public void ToggleMenuOpen(bool val)
    {
#if !ANDROID_PLATFORM && !UNITY_ANDROID

            if (val)
            {
                menuWrapper.SetActive(true);
                menuWrapperHolder.DOFade(1,.35f);
                
            }
            else
            {
                menuWrapperHolder.DOFade(0,.35f).OnComplete(() => menuWrapper.SetActive(false));
                
            }
#else
        Debug.Log("run tween position");
        if (val)
        {
            tweenPosition.Enable();
        }
        else
        {
            tweenPosition.Disable();
        }
#endif
    }
}

}