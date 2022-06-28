using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

namespace com.argentgames.visualnoveltemplate
{
[RequireComponent(typeof(Selectable))]
public class UIButtonInteraction : MonoBehaviour
{
    [SerializeField]
    Toggle toggle;
    [SerializeField]
    Button button;
    [SerializeField]
    string sfx;
    bool defaultActiveStatus;
    void Awake()
    {
        
    }
    void Start()
    {
        defaultActiveStatus = this.gameObject.activeSelf;
        this.gameObject.SetActive(false);
        SetRXSubscriptions();
        this.gameObject.SetActive(defaultActiveStatus);

    }

    public void SetRXSubscriptions()
    {
        if (toggle != null)
        {
            toggle.OnValueChangedAsObservable().Subscribe(val =>
            {
                if (val)
                {
                    PlaySound(sfx);
                }
                    
                
            }).AddTo(this);
        }
        if (button != null)
        {
            button.OnClickAsObservable().Subscribe(val =>
            {
                PlaySound(sfx);
            }).AddTo(this);
        }
    }

    public void PlaySound(string EventSound)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        Debug.Log("who is playing a ui sound: " + this.gameObject.name);
        AudioManager.Instance.PlaySFX(EventSound);
    }
}
}