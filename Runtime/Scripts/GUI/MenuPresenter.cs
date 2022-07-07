using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Opens and closes a menu. Works together with the MenuManager.
/// Requires at least one child object (that is the actual visible menu).
/// The visible menu is on a child object so that we can toggle it on/off while
/// still finding a reference to this menuPresenter, since Unity has some difficulties
/// finding components on disabled objects...
/// </summary>

namespace com.argentgames.visualnoveltemplate {
    public abstract class MenuPresenter : MonoBehaviour {
        public string defaultPage;
        public GameObject menuContainer;
        public abstract UniTask OpenPage(string pageName="");
        public virtual void CloseMenu()
        {
            menuContainer.SetActive(false);
        }
    }
}