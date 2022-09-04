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
        Selectable cgGallery, musicGallery, credits, about, mainMenu;
        [SerializeField]
        MenuPresenter menuPresenter;

        void Start()
        {
            SetRXSubscriptions();
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
                            var pageName = "settings";
                            var displayName = "Settings";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (settings.GetType() == typeof(Button))
                {
                    ((Button)settings).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = settings.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "settings";
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
                            var pageName = "history";
                            var displayName = "History";
                            if (metadata != null)
                            {
                                pageName = metadata.InternalName;
                                displayName = metadata.DisplayName;
                            }
                            menuPresenter.OpenPage(pageName);
                            DisplayHeadingText(displayName);
                        }
                    });
                }
                else if (history.GetType() == typeof(Button))
                {
                    ((Button)history).OnClickAsObservable().Subscribe(val =>
                    {
                        var metadata = history.GetComponentInChildren<GameObjectMetadata>();
                            var pageName = "history";
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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
                            menuPresenter.OpenPage(pageName);
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

        }
    }
}
