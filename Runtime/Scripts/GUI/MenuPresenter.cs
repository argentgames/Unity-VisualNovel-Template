using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// Opens and closes a menu. Works together with the MenuManager.
/// Requires at least one child object (that is the actual visible menu).
/// The visible menu is on a child object so that we can toggle it on/off while
/// still finding a reference to this menuPresenter, since Unity has some difficulties
/// finding components on disabled objects...
/// </summary>

namespace com.argentgames.visualnoveltemplate
{
    public abstract class MenuPresenter : SerializedMonoBehaviour
    {
        [Tooltip("The page we open to if we just call generic OpenPage()")]
        public string defaultPage;
        [Tooltip("The parent that we show/hide when opening/closing the menu.")]
        public GameObject menuContainer;

        [SerializeField]
        // [AllowNesting]
        public List<GameObject> pages = new List<GameObject>();
        public Dictionary<string, GameObject> pagesMap = new Dictionary<string, GameObject>();

        [SerializeField]
        [Tooltip("Menus usually have a navigation toolbar somewhere so we're going to grab that and open pages by selecting stuff in the nav")]
        public Navigation navigation;

        public abstract UniTask OpenPage(string pageName = "");
        public virtual void CloseMenu()
        {
            menuContainer.SetActive(false);
        }
        public void OpenEveryPage()
        {
            foreach (var page in pages)
            {
                page.SetActive(true);
            }
            // TECHDEBT: need to yield a frame maybe?
            // foreach (var page in pages)
            // {
            //     page.SetActive(false);
            // }
        }
    }
}