using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using TMPro;
using Ink.Runtime;
using UniRx;
using Cysharp.Threading.Tasks;
// using UnityEngine.UI.Extensions;

using System.Threading;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
namespace com.argentgames.visualnoveltemplate
{

    /// <summary>
    /// A dialogue box used to display ink script. At minimum, we need a dialogue text object
    /// to display the ink line, a choices parent to hold choices, and a choice prefab to 
    /// instantiate choices.
    /// </summary>
    public abstract class DialogueUIManager : MonoBehaviour
    {
        [SerializeField]
        [PropertyTooltip("Prefab of choice to spawn when the player needs to make a choice.")]
        [AssetsOnly]
        public GameObject choicePrefab;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("GameObject transform that we spawn children choices to.")]
        public GameObject choiceParent;

        [SerializeField]
        // [SceneObjectsOnly]
        [PropertyTooltip("Parent wrapper for the entire dialogue UI box.")]
        public GameObject UIHolder;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("Parent wrapper for quick menu")]
        public GameObject QMenu;

        [SerializeField]
        [Tooltip("the literal ctc used to continue. usually not visible to player.")]
        public Button actualCTC;


        // NOTE: Unused. Inkscript lets you add tags to the end of lines, so instead of parsing information at the beginning of a line (e.g. speaker name)
        // you could attach stuff to tags.
        private List<string> currentTags = new List<string>();
        public List<string> CurrentTags { get { return currentTags; } set { currentTags = value; } }

        private bool isDisplayingLine = false;
        public bool IsDisplayingLine { get { return isDisplayingLine; } set { isDisplayingLine = value; } }

        private bool waitingForPlayerToSelectChoice = false;
        public bool WaitingForPlayerToSelectChoice { get { return waitingForPlayerToSelectChoice;} set { waitingForPlayerToSelectChoice = value;} }
        private bool waitingForPlayerContinueStory = false;
        public bool WaitingForPlayerContinueStory  { get {return waitingForPlayerContinueStory;} set {waitingForPlayerContinueStory = value;} }

        private bool playerHidUI = false;
        public bool PlayerHidUI { get { return playerHidUI; } set { playerHidUI = value; } }
        private bool playerAllowedToHideUI = false;
        public bool PlayerAllowedToHideUI { get { return playerAllowedToHideUI; } set { playerAllowedToHideUI = value; } }

        Coroutine ctcDelay;
        public float delayBeforeAllowCTCLogic = .1f;
        bool isRunningCTCDelay = false;

        #region Click to continue
        public virtual async UniTaskVoid CTCLogic()
        {
            var wasSkipping = GameManager.Instance.IsSkipping;
            GameManager.Instance.SetSkipping(false);


            GameManager.Instance.SetAuto(false);

            // prevent spam clicking too fast <_<
            if (isRunningCTCDelay)
            {
                Debug.Log("please stop spam clicking"); 
                return;
            }

            if (DialogueSystemManager.Instance.IsRunningActionFunction)
            {
                Debug.Log("running action function, do nothing except turn off skipping");
                ImageManager.Instance.ThrowSkipToken();
                GameManager.Instance.ThrowSkipToken();
                DialogueSystemManager.Instance.RunCancellationToken();
            }
            else if (WaitingForPlayerToSelectChoice)
            {
                Debug.Log("do nothing, waiting for player to select choice");
            }
            else
            {
                if (!wasSkipping)
                {
                    // DialogueSystemManager.Instance.InkContinueStory().Forget();
                    // HideCTC();
                    // DisableCTC();
                    ClearText();
                    waitingForPlayerContinueStory = false;
                    DialogueSystemManager.Instance.waitingToContinueStory = false;
                    Debug.Log("not was skipping, please continue the story as normal");
                }

            }

            isRunningCTCDelay = true;
            ctcDelay = StartCoroutine(RunCTCDelay());
            
        }
        public virtual void TurnOnSkipOrAuto()
        {
            if (DialogueSystemManager.Instance.IsRunningActionFunction)
            {
                Debug.Log("running action function, do nothing except turn off skipping");
                ImageManager.Instance.ThrowSkipToken();
                GameManager.Instance.ThrowSkipToken();
                DialogueSystemManager.Instance.RunCancellationToken();
            }

            // might need an else? not sure
            else

            {
                ClearText();
                    waitingForPlayerContinueStory = false;
                    DialogueSystemManager.Instance.waitingToContinueStory = false;
            }

             
        }
        IEnumerator RunCTCDelay()
        {
            Debug.Log("is runnin gtct delay");
            yield return new WaitForSeconds(delayBeforeAllowCTCLogic);
            isRunningCTCDelay = false;
            Debug.Log("done running tc delay");
        }
        /// <summary>
        /// Do not allow the player to interact with the CTC, even if it is currently visible.
        /// </summary>
        public virtual void DisableCTC()
        {
        }
        /// <summary>
        /// Allow the player to interact with the CTC.
        /// </summary>
        public virtual void EnableCTC()
        {
        }

        /// <summary>
        /// If we have a CTC for the player to select, then hide it.
        /// </summary>
        public virtual void HideCTC()
        {
            actualCTC.gameObject.SetActive(false);
        }
        /// <summary>
        /// If we have a CTC for the player to select, then show it.
        /// </summary>
        public virtual void ShowCTC()
        {
            actualCTC.gameObject.SetActive(true);
        }
        #endregion

        #region Manipulating UI visibility
        /// <summary>
        /// Hide the dialogue box UI from the player.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual async UniTask HideUI(float duration = .3f)
        {
            var animator = UIHolder.GetComponent<AnimateObjectsToggleEnable>();
            if (animator != null)
            {
                await animator.Disable(duration);
            }
            else
            {
                Debug.LogFormat("No animation available for hiding UI. Hiding instantly.");
                UIHolder.SetActive(false);
            }

        }

        /// <summary>
        /// Make the dialogue box UI visible to the player.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual async UniTask ShowUI(float duration = .3f)
        {
            var animator = UIHolder.GetComponent<AnimateObjectsToggleEnable>();
            if (animator != null)
            {
                if (duration == -1)
                {
                    duration = animator.enableAnimationDuration;
                }
                await animator.Enable(duration);
            }
            else
            {
                Debug.LogFormat("No animation available for showing UI. showing instantly.");
                UIHolder.SetActive(true);
            }
            Debug.Log("done showing dialogue ui");

        }
        /// <summary>
        /// If your dialogue window has a separate NVL panel, maybe you want to manually show it with different timing and pauses
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual async UniTask ShowNVL(float duration = .3f)
        {

        }
        public virtual async UniTask HideNVL(float duration = .3f)
        {

        }

        /// <summary>
        /// If UI is showing, then hide it, otherwise show it without.
        /// </summary>
        public virtual void ToggleUI()
        {
            if (UIHolder.activeSelf)
            {
                HideUI();
            }
            else
            {
                ShowUIWithoutClearing();
            }
        }

        /// <summary>
        /// Show UI in its current state. Same as ShowUI in this abstract class...
        /// </summary>
        public virtual void ShowUIWithoutClearing()
        {

        }
        public void TogglePlayerOpenedQMenu()
        {
            DialogueSystemManager.Instance.SetPlayerOpenedQmenu(!DialogueSystemManager.Instance.PlayerOpenedQMenu);
        }
        public virtual void OpenQMenu(float duration = -1)
        {
            // don't open if it's already open...
            if (!DialogueSystemManager.Instance.PlayerOpenedQMenu)
            {
                var animator = QMenu.GetComponent<AnimateObjectsToggleEnable>();
                if (animator != null)
                {
                    if (duration == -1)
                    {
                        duration = animator.enableAnimationDuration;
                    }
                    animator.Enable(duration).Forget();
                }
                else
                {
                    QMenu.SetActive(true);
                }
            }
            else
            {
                CloseQmenu(duration);
            }


        }
        public virtual void CloseQmenu(float duration = -1)
        {
            var animator = QMenu.GetComponent<AnimateObjectsToggleEnable>();
            if (animator != null)
            {
                if (duration == -1)
                {
                    duration = animator.disableAnimationDuration;
                }
                animator.Disable(duration).Forget();
            }
            else
            {
                Debug.LogFormat("No animation available for hiding UI. Hiding instantly.");
                QMenu.SetActive(false);
            }
        }

        /// <summary>
        /// Is our dialogue box UI visible?
        /// </summary>
        /// <value></value>
        public bool IsShowingUI
        {
            get
            {
                if (UIHolder != null)
                {
                    return UIHolder.activeSelf;
                }
                else
                {
                    return false;
                }

            }
        }
        #endregion



        /// <summary>
        /// How we display a line of ink text.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public abstract UniTask DisplayLine(CancellationToken ct);

        /// <summary>
        /// Clear the dialogue box of any visible text.
        /// </summary>
        public virtual void ClearText()
        {
        }

        /// <summary>
        /// How we display choice options to the player.
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="story"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public abstract UniTask DisplayChoices(List<Choice> choices, Story story, CancellationToken ct);
        /// <summary>
        /// What do we do when the player selects a choice?
        /// </summary>
        /// <returns></returns>
        public abstract UniTaskVoid SelectChoice();

        public virtual void ClearUI() { }
        public virtual void KillTypewriter() { }
        public virtual void PauseTypewriter() { }
        public virtual void ContinueTypewriter() { }

        /// <summary>
        /// Every UI Manager needs to set up its continue story logic, which is usually tied to buttons
        /// or by setting subscriptions to the PlayerControls VNGameplay input map. If we change between
        /// different UIs, then we need to remove our previous UI's control scheme otherwise we will start
        /// stacking up many UI continue logic subscriptions !
        /// 
        /// We usually want BOTH a button and a VNGameplay input map subscription because it can be difficult
        /// to select QMenu buttons when the VNGameplay input map is active, as there isn't anything blocking that input
        /// (and checking for mouse over UI elements is not very reliable). We want a VNGameplay input map so that
        /// players can spam click through even while the UI (and thus the continue button) is hidden.
        /// </summary>
        public abstract void RemoveVNControlSubscriptions();
        public abstract void AddVNControlSubscriptions();
        public virtual void HideUILogic(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            if (DialogueSystemManager.Instance.DialogueUIManager.PlayerAllowedToHideUI)
            {
                Debug.Log("hide ui logic from boom");
                ToggleUI();
                GameManager.Instance.SetAuto(false);
                GameManager.Instance.SetSkipping(false);
                PlayerHidUI = !PlayerHidUI;

            }
        }



    }
}

