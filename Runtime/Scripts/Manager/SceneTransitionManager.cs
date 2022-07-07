using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Transition between Unity Scenes (e.g. main menu ==> ingame).
/// Turns on/off a transition image. If you attach an animation OnEnable/Disable to the transition
/// image, then the transition should use that animation.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; set; }

        [SerializeField]
        GameObject transitionObject;
        Color black = new Color(0, 0, 0, 255);
        Color transparent = new Color(0, 0, 0, 0);
        public bool IsLoading { get { return IsLoading; } }
        private bool isLoading = false;
        Tween tween;
        [SerializeField]
        AnimateObjectsToggleEnable animateObjectsToggleEnable;
        async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            // TODO
            // await UniTask.WaitUntil(() => LoadingScreenManager.Instance != null);

            // TECHDEBT: Why is this obj turned off on start <_<
            gameObject.SetActive(true);

            if (animateObjectsToggleEnable == null)
            {
                transitionObject.GetComponent<AnimateObjectsToggleEnable>();
            }

            // #if UNITY_EDITOR
            // FadeIn();
            // #endif

        }

        /// <summary>
        /// Barebones scene loading. Will load a Unity Scene and return as soon as it's loaded.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="loadSceneMode">Should the scene be loaded additively or singly?</param>
        /// <returns></returns>
        public async UniTask LoadScene(string level, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            await SceneManager.LoadSceneAsync(level, loadSceneMode).AsAsyncOperationObservable();
        }

        /// <summary>
        /// Load in a new scene with optional animation duration arguments.
        /// </summary>
        /// <param name="level">Name of the Unity Scene to load into</param>
        /// <param name="fadeOutDuration">Duration to start showing our transition image</param>
        /// <param name="fadeInDuration">Duration to start hiding our transition image after the scene is loaded</param>
        /// <param name="doFadeIn">Do we want to automatically hide the transition image after loading in the scene? Sometimes we want to manually hide it.</param>
        /// <param name="doStopSound">Do we need to stop current playing music when we load the scene? E.g. going from main menu ==> ingame.</param>
        /// <returns></returns>
        public async UniTask LoadScene(string level, float? fadeOutDuration = null, float? fadeInDuration = null, bool doFadeIn = true,
            bool doStopSound = true)
        {
            isLoading = true;
            MenuManager.Instance.DisableSettingsUIControls();
            if (fadeOutDuration == null)
            {
                fadeOutDuration = GameManager.Instance.DefaultConfig.sceneFadeOutDuration;
            }
            if (fadeInDuration == null)
            {
                fadeInDuration = GameManager.Instance.DefaultConfig.sceneFadeInDuration;
            }
            if (fadeOutDuration != 0)
            {
                await FadeToBlack(fadeOutDuration.Value);
            }

            if (doStopSound)
            {
                AudioManager.Instance.StopMusic(GameManager.Instance.DefaultConfig.loadSceneStopSoundDuration);
                AudioManager.Instance.StopAmbient(GameManager.Instance.DefaultConfig.loadSceneStopSoundDuration);
            }

            Debug.Log("load scene??");
            // TODO: prevent player from being able to move or open menus etc? or put this elsewhere
            await SceneManager.LoadSceneAsync(level, LoadSceneMode.Single).AsAsyncOperationObservable()
                .Do(x => { });
            // await LoadingScreenManager.Instance.HideLoadingScreen();
            if (doFadeIn && fadeInDuration != 0)
            {
                await FadeIn(fadeInDuration.Value);
            }

            isLoading = false;

        }
        /// <summary>
        /// Turn off the scene transition object.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async UniTask FadeIn(float? duration = null)
        {
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.sceneFadeInDuration;
            }
            // Debug.Log ("FADE IN");
            if (animateObjectsToggleEnable == null)
            {
                try
                {
                    var image = transitionObject.GetComponent<Image>();
                    DOTween.ToAlpha(() => image.color, x => image.color = x, 0, (float)duration).From(1);
                    await UniTask.WaitUntil(() => image.color.a == 0);

                }
                // TODO: add specific null compoennt exception
                catch
                {
                    transitionObject.SetActive(false);
                }


            }
            else
            {
                transitionObject.SetActive(false);
                await UniTask.WaitUntil(() => animateObjectsToggleEnable.AnimationComplete);
            }

        }
        /// <summary>
        /// Turn on the scene transition object.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public async UniTask FadeToBlack(float? duration = null)
        {
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.sceneFadeOutDuration;
            }
            // Debug.Log ("start fading to black");
            if (animateObjectsToggleEnable == null)
            {
                try
                {
                    transitionObject.GetComponent<Image>().color = transparent;
                    transitionObject.transform.parent.gameObject.SetActive(true);
                    var image = transitionObject.GetComponent<Image>();
                    DOTween.ToAlpha(() => image.color, x => image.color = x, 1, (float)duration).From(0);
                    await UniTask.WaitUntil(() => transitionObject.GetComponent<Image>().color.a == 1);
                }
                catch
                {
                    transitionObject.SetActive(false);
                }
            }
            else
            {
                transitionObject.SetActive(true);
                await UniTask.WaitUntil(() => animateObjectsToggleEnable.AnimationComplete);
            }

            Debug.Log("done fading to black");

        }

    }

}