using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
namespace com.argentgames.visualnoveltemplate
{
    public class ExampleMainMenuSettingsPresenter : MenuPresenter
    {
        [SerializeField]
        List<NamedGameObject> pages = new List<NamedGameObject>();
        Dictionary<string,GameObject> pagesMap = new Dictionary<string, GameObject>();

        void Awake()
        {
            foreach (var page in pages)
            {
                pagesMap[page.internalName] = page.gameObject;
            }

            // make sure all pages are not visible except for our default page
            foreach (var page in pages)
            {
                if (page.internalName != defaultPage)
                {
                    page.gameObject.SetActive(false);
                }
                
            }
        }
        public override async UniTask OpenPage(string pageName="")
        {
            foreach (var page in pages)
            {
                if (page.internalName != pageName)
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
