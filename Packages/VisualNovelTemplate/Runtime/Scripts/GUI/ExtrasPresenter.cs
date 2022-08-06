using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
using System;

namespace com.argentgames.visualnoveltemplate
{


    public class ExtrasPresenter : MonoBehaviour
    {

        [SerializeField]
        Button back;
        [SerializeField]
        Toggle cgNav, creditsNav, musicNav, aboutNav;


        [HorizontalGroup("extras page")]
        [VerticalGroup("extras page/Left")]
        [SerializeField]
        List<ExtrasPage> extrasPageIndex = new List<ExtrasPage>();
        [SerializeField]
        [HorizontalGroup("extras page")]
        [VerticalGroup("extras page/Right")]
        List<GameObject> extrasPageGOs = new List<GameObject>();

        Dictionary<ExtrasPage, GameObject> pages = new Dictionary<ExtrasPage, GameObject>();

        List<GameObject> activePages = new List<GameObject>();

        [SerializeField]
        CanvasGroup canvasGroup;
        PlayerControls _playerControls;
        void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.UI.Settings.performed += ctx =>
               {
                   ClosePages();

               };
            for (int i = 0; i < extrasPageIndex.Count; i++)
            {
                pages.Add(extrasPageIndex[i], extrasPageGOs[i]);
            }
            canvasGroup = GetComponentInChildren<CanvasGroup>();




            SetRXSubscriptions();

        }
        void Start()
        {
        }


        public void SetNavHeader(ExtrasPage page)
        {
            switch (page)
            {
                case ExtrasPage.CG:
                    cgNav.SetIsOnWithoutNotify(true);
                    cgNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(true);
                    creditsNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    aboutNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    break;
                case ExtrasPage.CREDITS:
                    creditsNav.SetIsOnWithoutNotify(true);
                    creditsNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(true);

                    cgNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    aboutNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    break;
                case ExtrasPage.ABOUT:
                    aboutNav.SetIsOnWithoutNotify(true);
                    aboutNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(true);

                    creditsNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    cgNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    break;
                default:

                    cgNav.SetIsOnWithoutNotify(true);
                    cgNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(true);

                    creditsNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    aboutNav.GetComponentInChildren<ToggleExtension>().SpriteSwapOnSelect(false);
                    break;
            }
        }


        private void OnDisable()
        {
            _playerControls.Disable();
        }




        void OnEnable()
        {
            _playerControls.Enable();
            foreach (var go in extrasPageGOs)
            {
                go.SetActive(false);
            }
        }

        void SetRXSubscriptions()
        {
            cgNav.onValueChanged.AsObservable().Subscribe(val =>
            {
                OpenPage(ExtrasPage.CG);
            // cgNav.transform.GetComponent<ToggleExtension>().SpriteSwapOnSelect(val);
        });
            creditsNav.onValueChanged.AsObservable().Subscribe(val =>
            {
                OpenPage(ExtrasPage.CREDITS);
            // creditsNav.transform.GetComponent<ToggleExtension>().SpriteSwapOnSelect(val);
        });
            aboutNav.onValueChanged.AsObservable().Subscribe(val =>
            {
                OpenPage(ExtrasPage.ABOUT);
            // aboutNav.transform.GetComponent<ToggleExtension>().SpriteSwapOnSelect(val);
        });
            back.OnClickAsObservable().Subscribe(val =>
            {
                ClosePages();
            }).AddTo(this);
        }

        public void OpenPage(ExtrasPage page)
        {

            if (!gameObject.activeSelf)
            {
                // canvasGroup.alpha = 0;
                gameObject.SetActive(true);
                // canvasGroup.DOFade(1, .4f);
            }

            pages[page].SetActive(true);
            foreach (var _page in activePages)
            {
                if (_page != pages[page])
                {
                    _page.SetActive(false);
                }

            }
            activePages.Clear();
            activePages.Add(pages[page]);

            // TECHDEBT: manually set the nav toggle to the right page <_<
            // EXCEPT I DONT NEED IT FOR EXTRAS NAV???
            // switch (page)
            // {
            //     case ExtrasPage.CG:
            //         cgNav.SetIsOnWithoutNotify(true);
            //         break;
            //     case ExtrasPage.CREDITS:
            //         creditsNav.SetIsOnWithoutNotify(true);
            //         break;
            //     case ExtrasPage.ABOUT:
            //         aboutNav.SetIsOnWithoutNotify(true);
            //         break;
            //     default:

            //         cgNav.SetIsOnWithoutNotify(true);
            //         break;
            // }
            this.gameObject.SetActive(true);


        }

        async UniTaskVoid ClosePages()
        {
            // canvasGroup.DOFade(0, .4f);
            // await UniTask.WaitUntil(() => canvasGroup.alpha == 0);
            this.gameObject.SetActive(false);
        }
    }
}
