using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;

public class Popup : MonoBehaviour
{
    public 
    Button close;
    public CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Awake()
    {
        // gameObject.SetActive(false);
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }
    void Start()
    {
        close.OnClickAsObservable().Subscribe((val) =>
        gameObject.SetActive(false));
    }

    // public void Enable()
    // {
    //     canvasGroup.DOFade(1,.5f);
    // }
    // public void Disable()
    // {
    //     canvasGroup.DOFade(0,.5f);
    // }
    // void OnEnable()
    // {
    //     Enable();
    // }
    // void OnDisable()
    // {
    //     Disable();
    // }
}
