using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AnimeTask;

/// <summary>
/// Transition between Unity Scenes (e.g. main menu ==> ingame).
/// Transition within a scene by covering the camera with an overlay
/// Turns on/off a transition image. If you attach an animation OnEnable/Disable to the transition
/// image, then the transition should use that animation.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; set; }

        [SerializeField]
        GameObject transitionObject, insceneTransitionObject;
        /// <summary>
        /// By default a black image that can be wiped/faded in. 
        /// We might want to assign a render texture to it instead though
        /// </summary>
        [SerializeField]
        GameObject projectedInsceneTransitionImage;
        Renderer insceneTransitionImageRenderer;
        [SerializeField]
        [Tooltip("We may want to change the sort order of the image in case we do want it underneath the dialogue box.")]
        Canvas insceneTransitionCanvas;
        private MaterialPropertyBlock _propBlock;
        Color black = new Color(0, 0, 0, 255);
        Color transparent = new Color(0, 0, 0, 0);
        public bool IsLoading { get { return isLoading; } }
        private bool isLoading = false;
        [SerializeField]
        AnimateObjectsToggleEnable animateObjectsToggleEnable;
        [SerializeField]
        Texture2D defaultInsceneTransitionImage, defaultInsceneTransitionWipe,transparentImage;
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

            _propBlock = new MaterialPropertyBlock();
            insceneTransitionImageRenderer = projectedInsceneTransitionImage.GetComponent<Renderer>();
            insceneTransitionObject.SetActive(false);

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
                    var image = transitionObject.GetComponentInChildren<Image>();
                    await Easing.Create<Linear>(start: 1f, end: 0f, (float)duration).ToColorA(image);

                }
                // TODO: add specific null compoennt exception
                catch
                {

                }
                transitionObject.SetActive(false);

            }
            else
            {
                await animateObjectsToggleEnable.Disable((float)duration);
                // await UniTask.WaitUntil(() => animateObjectsToggleEnable.AnimationComplete);
            }
            transitionObject.SetActive(false);

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
                    transitionObject.SetActive(true);
                    var image = transitionObject.GetComponentInChildren<Image>();
                    await Easing.Create<Linear>(start: 0f, end: 1f, (float)duration).ToColorA(image);
                }
                catch
                {

                }
                transitionObject.SetActive(true);
            }
            else
            {
                transitionObject.SetActive(true);
                await animateObjectsToggleEnable.Enable((float)duration);
                // await UniTask.WaitUntil(() => animateObjectsToggleEnable.AnimationComplete);
            }

            Debug.Log("done fading to black");

        }

        /// <summary>
        /// Go from transparent image ==> transition image
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="transitionImage"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public async UniTask ActivateInSceneTransition(float? duration = null, string transitionImage = "", string transition = "w9", int sortOrder = 10000)
        {
            insceneTransitionImageRenderer.GetPropertyBlock(_propBlock);
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.sceneFadeOutDuration;
            }

            insceneTransitionCanvas.sortingOrder = sortOrder;
            
            _propBlock.SetFloat("TransitionAmount", 0);
            Texture2D img;

            if (transitionImage == "")
            {   
                img = defaultInsceneTransitionImage;
            }
            else
            {
                img = GameManager.Instance.GetWipe(transitionImage).wipePrefab;
            }

            _propBlock.SetTexture("_MainTex",transparentImage);
            _propBlock.SetTexture("NewTex", img);
            
            if (transition == "fade")
            {
                // 1 is true
                _propBlock.SetFloat("_DoAlpha",1);
            }
            else
            {
                _propBlock.SetFloat("_DoAlpha",0);
                 _propBlock.SetTexture("Wipe",GameManager.Instance.GetWipe(transition).wipePrefab);
            }

           

            insceneTransitionImageRenderer.SetPropertyBlock(_propBlock);

            // QUESTION: in theory could run both transitions at the same time since it's now up to the bool DoAlpha whether
            // one or the other is shown?
            insceneTransitionObject.SetActive(true);

            if (transition != "fade")
            {
                await Easing.Create<InCubic>(start: 0f, end: 1f, duration: (float)duration)
                    .ToMaterialPropertyFloat(insceneTransitionImageRenderer, "TransitionAmount",skipToken: GameManager.Instance.SkipToken);
            }
            else
            {
                await Easing.Create<InCubic>(start: 1f, end: 0f, duration: (float)duration)
                    .ToMaterialPropertyFloat(insceneTransitionImageRenderer, "Alpha",skipToken: GameManager.Instance.SkipToken);
            }

            // reset transitionAmount 
            insceneTransitionImageRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("TransitionAmount", 0);
            if (transition == "fade")
            {
                _propBlock.SetFloat("Alpha", 1);
            }
            _propBlock.SetFloat("_DoAlpha",0);
            _propBlock.SetTexture("_MainTex",img);

            insceneTransitionImageRenderer.SetPropertyBlock(_propBlock);

        }
        /// <summary>
        /// Go from transition image ==> transparent image
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="transitionImage"></param>
        /// <param name="transition"></param>
        /// <returns></returns>
        public async UniTask DeactivateInSceneTransition(float? duration = null, string transitionImage = "", string transition = "w9")
        {
            insceneTransitionImageRenderer.GetPropertyBlock(_propBlock);
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.sceneFadeOutDuration;
            }
            
            _propBlock.SetFloat("TransitionAmount", 0);
            Texture2D img;

            if (transitionImage == "")
            {   
                img = transparentImage;
            }
            else
            {
                img = GameManager.Instance.GetWipe(transitionImage).wipePrefab;
            }

            _propBlock.SetTexture("NewTex", img);
            
            
            if (transition == "fade")
            {
                // 1 is true
                _propBlock.SetFloat("_DoAlpha",1);
                
            }
            else
            {
                _propBlock.SetFloat("_DoAlpha",0);
                _propBlock.SetTexture("Wipe",GameManager.Instance.GetWipe(transition).wipePrefab);
            }

            

            insceneTransitionImageRenderer.SetPropertyBlock(_propBlock);

            // QUESTION: in theory could run both transitions at the same time since it's now up to the bool DoAlpha whether
            // one or the other is shown?
            if (transition != "fade")
            {
                await Easing.Create<InCubic>(start: 0f, end: 1f, duration: (float)duration)
                    .ToMaterialPropertyFloat(insceneTransitionImageRenderer, "TransitionAmount",skipToken: GameManager.Instance.SkipToken);
            }
            else
            {
                await Easing.Create<InCubic>(start: 1f, end: 0f, duration: (float)duration)
                    .ToMaterialPropertyFloat(insceneTransitionImageRenderer, "Alpha",skipToken: GameManager.Instance.SkipToken);
            }

            // reset transitionAmount 
            insceneTransitionImageRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("TransitionAmount", 0);
            if (transition == "fade")
            {
                _propBlock.SetFloat("Alpha", 1);
            }
            _propBlock.SetFloat("_DoAlpha",0);
            _propBlock.SetTexture("_MainTex",img);

            insceneTransitionImageRenderer.SetPropertyBlock(_propBlock);

            insceneTransitionObject.SetActive(false);

        }

    }

}