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
        [PropertyTooltip("The TMP_Text that will display an ink line.")]
        public TMP_Text dialogueText;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("GameObject transform that we spawn children choices to.")]
        public GameObject choiceParent;

        [SerializeField]
        // [SceneObjectsOnly]
        [PropertyTooltip("Parent wrapper for the entire dialogue UI box.")]
        public GameObject UIHolder;


        // NOTE: Unused. Inkscript lets you add tags to the end of lines, so instead of parsing information at the beginning of a line (e.g. speaker name)
        // you could attach stuff to tags.
        private List<string> currentTags = new List<string>();
        public List<string> CurrentTags { get { return currentTags;} set { currentTags = value;}}

        private bool isDisplayingLine = false;
        public bool IsDisplayingLine { get { return isDisplayingLine; } set {isDisplayingLine = value;}}


        #region Click to continue
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
        }
        /// <summary>
        /// If we have a CTC for the player to select, then show it.
        /// </summary>
        public virtual void ShowCTC()
        {
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
           

        }
        
        /// <summary>
        /// Make the dialogue box UI visible to the player.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual async UniTask ShowUI(float duration = .3f)
        {
           UIHolder.SetActive(true);
           Debug.Log("done showing dialogue ui");

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
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = 0;
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

        public virtual void ClearUI() {}
        public virtual void KillTypewriter() {}
        public virtual void PauseTypewriter() {}
        public virtual void ContinueTypewriter() {}



    }
}

