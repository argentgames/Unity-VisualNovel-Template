using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using NaughtyAttributes;
using UnityEngine.EventSystems;

namespace com.argentgames.visualnoveltemplate
{
    [RequireComponent(typeof(Selectable))]
    public class SelectablePlaySfx : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [SerializeField]
        bool onClick;
        [SerializeField]
        bool onDown;
        [SerializeField]
        bool onUp;
        [SerializeField]
        bool onExit;
        [SerializeField]
        bool onEnter;
        [SerializeField]
        [ShowIf("onClick")]
        string clickSound="";
        [SerializeField]
        [ShowIf("onDown")]
        string downSound="";
        [SerializeField]
        [ShowIf("onUp")]
        string upSound="";
        [SerializeField]
        [ShowIf("onExit")]
        string exitSound="";
        [SerializeField]
        [ShowIf("onEnter")]
        // [Dropdown("StringValues")]
        string enterSound="";

        // [ExecuteInEditMode]

        // private List<string> StringValues { get { return new List<string>() { "A", "B", "C", "D", "E" }; } }

        Selectable selectable;
        Toggle toggle;
        Button button;
        void Awake()
        {
            selectable = GetComponent<Selectable>();
            if (selectable is Toggle)
            {
                toggle = (Toggle)selectable;
            }
            else
            {
                button = (Button)selectable;
            }
            if (clickSound == "")
            {
                clickSound = GameManager.Instance.DefaultConfig.selectableClickSfx;
            }
            if (downSound == "")
            {
                downSound =GameManager.Instance.DefaultConfig.selectableDownSfx;
            }
            if (upSound == "")
            {
                upSound =GameManager.Instance.DefaultConfig.selectableUpSfx;
            }
            if (exitSound == "")
            {
                exitSound =GameManager.Instance.DefaultConfig.selectableExitSfx;
            }
            if (enterSound == "")
            {
                enterSound =GameManager.Instance.DefaultConfig.selectableEnterSfx;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick)
            {
                AudioManager.Instance.PlaySFX(clickSound);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (onDown)
            {
                AudioManager.Instance.PlaySFX(downSound);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (onUp)
            {
                AudioManager.Instance.PlaySFX(upSound);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (onEnter)
            {
                if (toggle != null)
                {
                    if (toggle.isOn)
                    {
                        return;
                    }
                }
                AudioManager.Instance.PlaySFX(enterSound);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (onExit)
            {
                if (toggle != null)
                {
                    if (toggle.isOn)
                    {
                        return;
                    }
                }
                AudioManager.Instance.PlaySFX(exitSound);
            }
        }




    }
}
