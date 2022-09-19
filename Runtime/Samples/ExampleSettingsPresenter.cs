using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;

// TODO: replace using the GameObject name with a NamedGameObject
// so that we can have arbitrary naming in our hierarchy for easy readability...
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleSettingsPresenter : MenuPresenter
    {
        [SerializeField]
        // [AllowNesting]
        List<GameObject> pages = new List<GameObject>();
        Dictionary<string, GameObject> pagesMap = new Dictionary<string, GameObject>();

        [SerializeField]
        [Tooltip("Menus usually have a navigation toolbar somewhere so we're going to grab that and open pages by selecting stuff in the nav")]
        Navigation navigation;


        void Awake()
        {
            foreach (var page in pages)
            {
                var metadata = page.GetComponentInChildren<GameObjectMetadata>();
                string pageName = page.name;
                if (metadata != null)
                {
                    pageName = metadata.InternalName;
                }
                pagesMap[pageName] = page;
            }

            // make sure all pages are not visible except for our default page
            foreach (var page in pages)
            {
                if (page.name != defaultPage)
                {
                    page.gameObject.SetActive(false);
                }

            }

            menuContainer.SetActive(false);
        }
        public override async UniTask OpenPage(string pageName = "")
        {
            // wait a couple frames for our rx subscriptsiont to set?
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            // TECHDEBT: Beautiful transitions animations don't run when game is paused <_<
            // GameManager.Instance.PauseGame();
            if (pageName == "")
            {
                pageName = defaultPage;
            }


            if (navigation != null)
            {
                // Debug.Log("using navigation openNavPage");
                // Debug.LogFormat("state of menucontainer: {0}", menuContainer.activeSelf);
                // menuContainer.SetActive(true);
                // Debug.LogFormat("state of menucontainer: {0}", menuContainer.activeSelf);
                navigation.OpenNavPage(pageName);

                if (!menuContainer.activeSelf)
                {
                    Debug.Log("setting menu container active0");
                    menuContainer.SetActive(true);
                }

            }
            else
            {
                // fallback to just manually turn on the specific page if no Navigation is present
                var newPage = pagesMap[pageName];
                newPage.SetActive(true);

                foreach (var page in pages)
                {
                    var metadata = page.GetComponentInChildren<GameObjectMetadata>();
                    string _pageName = page.name;
                    if (metadata != null)
                    {
                        _pageName = metadata.InternalName;
                    }

                    var animator = page.GetComponentInChildren<AnimateObjectsToggleEnable>();

                    if (_pageName != pageName)
                    {
                        if (animator != null)
                        {
                            animator.Disable(0);
                        }
                        else
                        {
                            page.gameObject.SetActive(false);
                        }

                    }

                }

                if (!menuContainer.activeSelf)
                {
                    Debug.Log("setting menu container active0");
                    menuContainer.SetActive(true);
                }

            }




        }
    }
}
