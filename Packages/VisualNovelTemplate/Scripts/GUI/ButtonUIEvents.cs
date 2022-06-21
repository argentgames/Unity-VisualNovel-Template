using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using System.Collections.Generic;
using System;
using UniRx;
using UnityEngine.UI;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

// [RequireComponent(typeof(Selectable))]
public class ButtonUIEvents : MonoBehaviour
{
    [SerializeField]
    GameObject onClickEffect, onHoverEffect;
    Selectable selectable;
    // Start is called before the first frame update
    void Start()
    {

        selectable = GetComponent<Selectable>();
        SetRXSubscriptions();


        //     ObservablePointerEnterTrigger trigger = gameObject.AddComponent<ObservablePointerEnterTrigger>();
        // IObservable<PointerEventData> observable = trigger.OnPointerEnterAsObservable();
        // observable.Subscribe(data => Debug.Log("Pointer down " + data));


    }
    void OnDisable()
    {
        if (onHoverEffect != null)
        {
            onHoverEffect.SetActive(false);
        }
        if (onClickEffect != null)
        {
            onClickEffect.SetActive(false);
        }
    }
    void OnEnable()
    {
        if (onHoverEffect != null)
        {
            onHoverEffect.SetActive(false);
        }
        if (onClickEffect != null)
        {
            onClickEffect.SetActive(false);
        }
    }

    private void SetRXSubscriptions()
    {
        selectable.OnPointerEnterAsObservable().Subscribe(x =>
        {
            if (onHoverEffect != null)
            {
                onHoverEffect.SetActive(true);
            }

        }).AddTo(this);
        selectable.OnPointerExitAsObservable().Subscribe(x =>
        {
            if (onHoverEffect != null)
            {
                onHoverEffect.SetActive(false);
            }
        }).AddTo(this);

        selectable.OnPointerClickAsObservable().Subscribe(x =>
       {

           SelectableLogic();
       }).AddTo(this);

    }
    async UniTaskVoid SelectableLogic()
    {
        if (onClickEffect != null)
        {
            Debug.Log("click");
            onClickEffect.SetActive(true);
            if (selectable.GetType() == typeof(Button))
            {
                // await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.GlobalDefinitions.delayBeforHideClickUIFX));
                try
                {
                    onClickEffect.SetActive(false);
                }
                catch
                {
                    Debug.LogFormat("I have no idea why buttonuievents gameobject becomes null");
                }

            }
        }
    }
    public void DisableUIStyles()
    {
        if (onHoverEffect != null)
        {
            onHoverEffect.SetActive(false);
        }
        if (onClickEffect != null)
        {
            onClickEffect.SetActive(false);
        }

    }
}
