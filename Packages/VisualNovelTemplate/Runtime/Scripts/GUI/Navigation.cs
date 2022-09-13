using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.argentgames.visualnoveltemplate;
using TMPro;
using UnityEngine.UI;
using UniRx;

namespace com.argentgames.visualnoveltemplate
{
    /// <summary>
    /// Selectable navigation toolbar for settings pages to switch between pages.
    /// Will update heading text if metadata is available.
    /// </summary>
    public class Navigation : MonoBehaviour
    {
        [SerializeField]
        TMP_Text heading;
        [SerializeField]
        Selectable settings, history, saveLoad, save, load;
        [SerializeField]
        Selectable cgGallery, musicGallery, credits, about, mainMenu, closeMenu;
        [SerializeField]
        MenuPresenter menuPresenter;
        [SerializeField]
        ToggleGroupExtension toggleGroupExtension;

        Dictionary<string,Selectable> navMap = new Dictionary<string, Selectable>();

        void Awake()
        {
             navMap = new Dictionary<string, Selectable>();
            navMap["Settings"] = settings;
            navMap["History"] = history;
            navMap["SaveLoad"] = saveLoad;
            navMap["Save"] = save;
            navMap["Load"] = load;
            navMap["CGGallery"] = cgGallery;
            navMap["MusicGallery"] = musicGallery;
            navMap["Credits"] = credits;
            navMap["About"] = about;
            
            foreach (var kv in navMap.Keys)
            {
                Debug.Log(kv);
            } SetRXSubscriptions();
        }

        void Start()
        {
           
           
        }
        void DisplayHeadingText(string text)
        {
            heading.text = text;
        }
        void SetRXSubscriptions()
        {
            if (settings != null)
            {
                if (settings is Toggle)
                {
                    ((Toggle)settings).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = settings.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Settings";
                            var displayName = "Settings";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = settings.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to Settings!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (settings.GetType() == typeof(Button))
                {
                    ((Button)settings).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = settings.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Settings";
                            var displayName = "Settings";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", settings);
                }
            }

            if (history != null)
            {
                if (history is Toggle)
                {
                    ((Toggle)history).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = history.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "History";
                            var displayName = "History";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = history.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to History!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (history.GetType() == typeof(Button))
                {
                    ((Button)history).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = history.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "History";
                            var displayName = "History";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", history);
                }
            }

            if (saveLoad != null)
            {
                if (saveLoad is Toggle)
                {
                    ((Toggle)saveLoad).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = saveLoad.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "SaveLoad";
                            var displayName = "Save/Load";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = saveLoad.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to SaveLoad!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (saveLoad.GetType() == typeof(Button))
                {
                    ((Button)saveLoad).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = saveLoad.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "SaveLoad";
                            var displayName = "Save/Load";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", saveLoad);
                }
            }
            if (load != null)
            {
                if (load is Toggle)
                {
                    ((Toggle)load).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = load.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Load";
                            var displayName = "Load";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                           var toggleExtension = load.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to Load!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (load.GetType() == typeof(Button))
                {
                    ((Button)load).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = load.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Load";
                            var displayName = "Load";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", load);
                }
            }
            if (save != null)
            {
                if (save is Toggle)
                {
                    ((Toggle)save).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = save.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Save";
                            var displayName = "Save";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = save.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to Save!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (save.GetType() == typeof(Button))
                {
                    ((Button)save).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = save.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Save";
                            var displayName = "Save";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", save);
                }
            }

            if (cgGallery != null)
            {
                if (cgGallery is Toggle)
                {
                    ((Toggle)cgGallery).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = cgGallery.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "CGGallery";
                            var displayName = "CG Gallery";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                           var toggleExtension = cgGallery.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to CGGallery!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (cgGallery.GetType() == typeof(Button))
                {
                    ((Button)cgGallery).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = cgGallery.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "CGGallery";
                            var displayName = "CG Gallery";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", cgGallery);
                }
            }

            if (musicGallery != null)
            {
                if (musicGallery is Toggle)
                {
                    ((Toggle)musicGallery).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = musicGallery.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "MusicGallery";
                            var displayName = "Music Room";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = musicGallery.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to MusicGallery!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (musicGallery.GetType() == typeof(Button))
                {
                    ((Button)musicGallery).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = musicGallery.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "MusicGallery";
                            var displayName = "Music Room";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", musicGallery);
                }
            }

            if (credits != null)
            {
                if (credits is Toggle)
                {
                    ((Toggle)credits).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = credits.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Credits";
                            var displayName = "Credits";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            var toggleExtension = credits.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to Credits!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (credits.GetType() == typeof(Button))
                {
                    ((Button)credits).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = credits.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "Credits";
                            var displayName = "Credits";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", credits);
                }
            }

            if (about != null)
            {
                if (about is Toggle)
                {
                    ((Toggle)about).onValueChanged.AsObservable().Subscribe(val =>
                    {
                        if (val)
                        {
                            var metadata = about.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "About";
                            var displayName = "About";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                           var toggleExtension = about.GetComponentInChildren<ToggleExtension>();
                            if (toggleExtension != null)
                            {
                                toggleGroupExtension.EnableGameObject(toggleExtension.GameObjectToToggleOn);
                            }
                            else
                            {
                                Debug.Log("don't know what page to turn on; add a toggle extension to About!");
                            }
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (about.GetType() == typeof(Button))
                {
                    ((Button)about).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = about.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "About";
                            var displayName = "About";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", about);
                }
            }

            if (mainMenu != null)
            {
                if (mainMenu is Toggle)
                {
                    Debug.LogWarning("Did you mean to make MainMenu a Toggle? Not setting any subscriptions!");
                    return;
                }
                else if (mainMenu.GetType() == typeof(Button))
                {
                    ((Button)mainMenu).OnClickAsObservable().Subscribe(val =>
                    {
                        // TODO: add confirm panel to confirm wanting to return to mM
                    });
                }
                else
                {
                    Debug.LogWarningFormat("Selectable type not implemented. No subscription made for {0}", mainMenu);
                }
            }
            
            if (closeMenu != null)
            {
                if (closeMenu is Toggle)
                {
                    Debug.LogWarning("Did you mean to make CloseMenu a Toggle? Not setting any subscriptions!");
                }
                else if (closeMenu is Button)
                {
                    ((Button)closeMenu).OnClickAsObservable().Subscribe(val =>
                    {
                        menuPresenter.CloseMenu();
                    });
                }
            }

        }
        public void OpenNavPage(string navPage)
        {
            foreach (var k in navMap.Keys)
            {
                Debug.Log(k);
            }
            var page = navMap[navPage];
            if (page is Toggle)
            {
                Debug.Log("settting toggle on for navPage " + navPage);
                ((Toggle)page).isOn = true;
            }
            else if (page is Button)
            {
                Debug.Log("page is button?");
                ((Button)page).Select(); // I don't think this actually submits the Button?
            }
        }
    }
}
