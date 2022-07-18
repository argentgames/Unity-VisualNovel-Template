using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using Sirenix.OdinInspector;

// TODO: replace using the GameObject name with a NamedGameObject
// so that we can have arbitrary naming in our hierarchy for easy readability...
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleMainMenuSettingsPresenter : MenuPresenter
    {
        [SerializeField]
        // [AllowNesting]
        List<GameObject> pages = new List<GameObject>();
        Dictionary<string,GameObject> pagesMap = new Dictionary<string, GameObject>();

        void Awake()
        {
            foreach (var page in pages)
            {
                pagesMap[page.name] = page.gameObject;
            }

            // make sure all pages are not visible except for our default page
            foreach (var page in pages)
            {
                if (page.name != defaultPage)
                {
                    page.gameObject.SetActive(false);
                }
                
            }
        }
        public override async UniTask OpenPage(string pageName="")
        {
            foreach (var page in pages)
            {
                if (page.name != pageName)
                {
                    page.gameObject.SetActive(false);
                }
                else
                {
                    page.gameObject.SetActive(true);
                }
                
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
}
