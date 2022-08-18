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
using AnimeTask;
namespace com.argentgames.visualnoveltemplate
{
    public class ADVController : DialogueUIManager
    {
        [SerializeField]
        [SceneObjectsOnly]
        Image textboxBG, speakerNameBG;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("The TMP_Text object that displays the name of the speaker.")]
        TMP_Text speakerText, dialogueText;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("Image to display when the dialogue is done being shown and indicates to the player that they should click to continue. Does not function as an actual interactable button.")]
        GameObject ctcImage;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("")]
        GameObject speakerNameHolder;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("The actual button that you click to continue the text.")]
        Button clickToContinue;

        [SerializeField]
        [SceneObjectsOnly]
        [PropertyTooltip("Controller of single side portrait")]
        PortraitPresenter portraitPresenter;

        [SerializeField]
        [PropertyTooltip("If the portrait is being shown, do we need to move the dialogue text and name cards?")]
        float dialogueTextLeftMarginForPortraitMode = 402.2328f, portraitNameX = -391f, namecardX = -808f;

        [PropertySpace(SpaceBefore = 15)]
        [SerializeField]
        [InfoBox("When we run HideUI/ShowUI, we will toggle all these elements on/off. For now, we only support fading the canvas group. In future, we need to support an arbitrary animation!")]
        List<CanvasGroup> uiElementsToToggle = new List<CanvasGroup>();

        // // TECHDEBT: hacky way of applying inline <wait> to lines to pause dialogue unwrapping
        // List<Tween> dialogueUnwrapTweens = new List<Tween>();
        // TECHDEBT: these sequences control text unwrapping and ui show/hiding, but it's questionably functional...
        // Sequence sequence, dialogueBoxSequence;

        [InfoBox("The parent that holds all choice options. Use this to show/hide all the choices simultaneously. For now, only supports fade. In future, need to support an arbitrary animation of choices!")]
        CanvasGroup choiceParentCanvasGroup;

        // Used to enable player input to control UI, such as hiding/showing textbox with a keybinding.
        PlayerControls _playerControls;
        private bool portraitCurrentlyShowing = false;
        private bool waitingForPlayerToSelectChoice = false;
        bool KillTypewriterRequested = false;
        List<UniTask> tasks = new List<UniTask>();
        void Awake()
        {
            _playerControls = new PlayerControls();
            _playerControls.UI.HideIngameUI.performed += ctx =>
           {
               ToggleUI();
               GameManager.Instance.SetAuto(false);
               GameManager.Instance.SetSkipping(false);
               PlayerHidUI = !PlayerHidUI;
           };
            _playerControls.UI.Click.performed += ctx =>
            {
                if (PlayerHidUI)
                {
                    ToggleUI();
                }
                // Debug.Log("player click so toggle UI");
                // if (!UIHolder.activeSelf)
                // {
                //     if (GameObject.Find("Popup") == null && GameObject.Find("Ad") == null && DialogueSystemManager.Instance.PlayerCanContinue)
                //     {

                //         ToggleUI();
                //     }
                // }
            };
        }
        // Start is called before the first frame update
        void Start()
        {

            clickToContinue.OnClickAsObservable()
        .Subscribe(_ =>
            {
                CTCLogic();

            }).AddTo(this);

            tasks.Clear();
            HideUI(0);
            choiceParentCanvasGroup = choiceParent.GetComponentInChildren<CanvasGroup>();
        }

        /// <summary>
        /// What logic do we run when the player clicks to continue
        /// </summary>
        /// <returns></returns>
        async UniTaskVoid CTCLogic()
        {
            Debug.Log("clicked");
            var wasSkipping = GameManager.Instance.IsSkipping;
            GameManager.Instance.SetSkipping(false);


            GameManager.Instance.SetAuto(false);

            if (IsDisplayingLine)
            {
                Debug.Log("is displaying line, killing typewriter!");
                // DialogueSystemManager.Instance.RunCancellationToken();
                // TODO: CAN"T SKIP BY SPAM CLICKING AT THE MOMENT :(((((
                KillTypewriter();
            }

            else if (DialogueSystemManager.Instance.IsRunningActionFunction)
            {
                Debug.Log("running action function, do nothing except turn off skipping");
                return;
            }
            else if (waitingForPlayerToSelectChoice)
            {
                Debug.Log("do nothing, waiting for player to select choice");
                return;
            }
            else
            {
                if (!wasSkipping)
                {
                    DialogueSystemManager.Instance.InkContinueStory();
                    HideCTC();
                    // DisableCTC();
                    ClearText();
                }

            }
        }

        public override void DisableCTC()
        {
            Debug.Log("disable ctc");
            clickToContinue.gameObject.SetActive(false);
        }
        public override void EnableCTC()
        {
            Debug.Log("Enable ctc");
            clickToContinue.gameObject.SetActive(true);
        }
        public override void HideCTC()
        {
            ctcImage.SetActive(false);
        }
        public override void ShowCTC()
        {
            ctcImage.SetActive(true);
            EnableCTC();
        }



        public override async UniTask HideUI(float duration = .3f)
        {
            if (UIHolder.activeSelf)
            {
                // TECHDEBT: hax for skipping bugs?
                // if (GameManager.Instance.IsSkipping)
                // {
                //     duration = 0.002f;
                // }
                // if (!dialogueBoxSequence.IsActive())
                // {
                    
                // }
                tasks.Clear();
                foreach (var uiElement in uiElementsToToggle)
                {
                    tasks.Add(
                        Easing.Create<Linear>(start: 1f, end: 0f, duration: duration).ToColorA(uiElement)
                    );
                }

                await UniTask.WhenAll(tasks);
                UIHolder.SetActive(false);

                portraitPresenter.HidePortrait();
            }

        }

        public void ToggleUI()
        {
            Debug.Log("calling toggle ui");
            if (UIHolder.activeSelf)
            {
                if (portraitPresenter.IsShowingPortrait)
                {
                    portraitCurrentlyShowing = true;
                }
                HideUI();
            }
            else
            {
                ShowUIWithoutClearing();
                if (portraitCurrentlyShowing)
                {
                    portraitPresenter.ShowPortrait();
                    portraitCurrentlyShowing = false;
                }
            }
        }
        /// <summary>
        /// Show the UI without clearing the text from the dialogue box
        /// </summary>
        /// <param name="duration"></param>
        public async UniTask ShowUIWithoutClearing(float duration = .3f)
        {
            UIHolder.SetActive(true);

            tasks.Clear();
            foreach (var uiElement in uiElementsToToggle)
            {
                tasks.Add(
                        Easing.Create<Linear>(start: 0f, end: 1f, duration: duration).ToColorA(uiElement)
                    );
            }

            await UniTask.WhenAll(tasks);

        }

        /// <summary>
        /// Clear the dialogue text from the box and then show an empty UI
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public override async UniTask ShowUI(float duration = .3f)
        {
            dialogueText.text = "";
            HideCTC();
            speakerText.text = "";
            portraitPresenter.HidePortrait();
            speakerNameHolder.SetActive(false);
            UIHolder.SetActive(true);
            // if (dialogueBoxSequence.IsActive())
            // {
            //     dialogueBoxSequence.Complete();
            // }

            tasks.Clear();

            if (GameManager.Instance.IsSkipping)
            {
                duration = 0.002f;
            }
            foreach (var uiElement in uiElementsToToggle)
            {
                tasks.Add(
                        Easing.Create<Linear>(start: 0f, end: 1f, duration: duration).ToColorA(uiElement)
                    );
            }

            await UniTask.WhenAll(tasks);


        }
        /// <summary>
        /// Are we currently showing the UI?
        /// </summary>
        /// <value></value>
        public new bool IsShowingUI
        {
            get
            {
                if (UIHolder != null)
                {
                    return UIHolder.activeSelf && UIHolder.GetComponentInChildren<CanvasGroup>().alpha == 1;
                }
                else
                {
                    return false;
                }

            }
        }

        public override async UniTask DisplayLine(CancellationToken ct)
        {
            ClearText();
            IsDisplayingLine = true;
            bool needToShowSpeakerName = false;
            HideCTC();
            // Debug.Log(text);
            var dialogue = DialogueSystemManager.Instance.CurrentProcessedDialogue;
            Debug.LogFormat("dialogue line: {0}",dialogue);

            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText), cancellationToken: this.GetCancellationTokenOnDestroy());

            // set namecard if available
            if (dialogue.speaker == "narrator")
            {
                speakerText.gameObject.transform.parent.gameObject.SetActive(false);


            }
            else
            {
                needToShowSpeakerName = true;
            }
            bool needToShowPortrait = false;
            // Debug.LogFormat("dialogue.npc is SpriteNPC_SO?: {0}", dialogue.npc is SpriteNPC_SO);
            if (dialogue.npc.HasSpriteImages)
            {
                needToShowPortrait = await ImageManager.Instance.ExpressionChange(dialogue.npc.internalName, dialogue.expression, dialogue.duration);
                Debug.Log(needToShowPortrait);
                if (!needToShowPortrait)
                {
                    portraitPresenter.HidePortrait();
                }

            }
            else
            {
                if (!needToShowPortrait)
                {
                    portraitPresenter.HidePortrait();
                }

            }
            if (needToShowPortrait)
            // if (needToShowPortrait && !(bool)DialogueSystemManager.Instance.Story.variablesState["cgmode"])
            {
                dialogueText.margin = new Vector4(dialogueTextLeftMarginForPortraitMode, 0, 0, 0);
            }
            else
            {
                dialogueText.margin = new Vector4(0, 0, 0, 0);
            }
            // TODO: add npc specific color?
            // set dialogue
            // Assign the text so TMP can calculate characters correctly
            dialogueText.text = dialogue.text;

            // add to history
            // TODO: format speaker name later
            var speakerName = "";
            if (dialogue.speaker != "narrator")
            {
                speakerName = dialogue.npc.DisplayName;
            }
            var historyLog = new DialogueHistoryLine(speakerName, dialogue.text);
            DialogueSystemManager.Instance.AddDialogueToHistory(historyLog);

            // TODO: add toggleable needToShowPortrait instead of having an inference with cgmode
            // if ((bool)DialogueSystemManager.Instance.Story.variablesState["cgmode"])
            // {
            //     needToShowPortrait = false;
            // }

            // Debug.LogFormat("do i need to show portrait?: {0}", needToShowPortrait);
            if (needToShowPortrait)
            {
                speakerNameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(portraitNameX, speakerNameHolder.GetComponent<RectTransform>().anchoredPosition.y);
            }
            else
            {
                speakerNameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(namecardX, speakerNameHolder.GetComponent<RectTransform>().anchoredPosition.y);

            }
            if ((GameManager.Instance.IsSkipping && GameManager.Instance.Settings.skipAllText) ||
            (GameManager.Instance.IsSkipping && (DialogueSystemManager.Instance.CurrentTextSeenBefore())) ||
            KillTypewriterRequested)
            {
                Debug.LogFormat("WHY ARE YOU SKIPING: isSkipping {0}, skipAllText {1}, seenText {2}",
                GameManager.Instance.IsSkipping, GameManager.Instance.Settings.skipAllText, DialogueSystemManager.Instance.CurrentTextSeenBefore());
                if (needToShowPortrait)
                {
                    portraitPresenter.ShowChar(dialogue.npc.internalName);
                }
                if (needToShowSpeakerName)
                {
                    // TODO: add npc specific color?
                    speakerText.text = dialogue.npc.DisplayName;
                    speakerText.color = dialogue.npc.NameColor;
                    speakerText.gameObject.transform.parent.gameObject.SetActive(true);
                }

                dialogueText.maxVisibleCharacters = dialogueText.text.Length;
                KillTypewriter();
                // DialogueSystemManager.Instance.InkContinueStory();
            }
            else
            {
                GameManager.Instance.SetSkipping(false);
                // Set TMP maxVisibleCharacters to 0
                dialogueText.maxVisibleCharacters = 0;

                // if (portraitPresenter.IsShowingPortrait)
                // {
                    if (needToShowPortrait)
                // if (needToShowPortrait && !(bool)DialogueSystemManager.Instance.Story.variablesState["cgmode"])
                {
                    portraitPresenter.ShowChar(dialogue.npc.internalName);
                }
                // }

                if (needToShowSpeakerName)
                {
                    // TODO: add npc specific color?
                    speakerText.text = dialogue.npc.DisplayName;
                    speakerText.color = dialogue.npc.NameColor;
                    speakerText.gameObject.transform.parent.gameObject.SetActive(true);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText));

                // TODO: the split reveal for WAIT is incorrect character counting.
                // split text into multiple tweens if there are any Waits, <<wait=float>>
                string[] separatingStringsStart = { "<<" };
                string[] separatingStringsEnd = { ">>" };
                string[] closingTagSplit;
                var trySplitWaits = dialogueText.text.Split(separatingStringsStart, System.StringSplitOptions.RemoveEmptyEntries);
                List<string> textSplits = new List<string>();
                List<float> textSplitWaits = new List<float>();
                if (trySplitWaits.Length > 1)
                {
                    // there are some <<command>>s inline. every other index will contain the closing brackets
                    // we onlyu support WAIT command...
                    // v: Don't bother <<wait=2>> bringing <<wait=2>>those <<wait=2>>cushions <<wait=5>>over, Maja.
                    // split[0] = v: Don't bother 
                    // split[1] = wait=4>> bringing those 
                    // split[2] = wait=.2>>cushions over, Maja.
                    for (int i = 0; i < trySplitWaits.Length; i++)
                    {
                        closingTagSplit = trySplitWaits[i].Split(separatingStringsEnd, System.StringSplitOptions.RemoveEmptyEntries);
                        if (closingTagSplit.Length == 1)
                        {
                            textSplits.Add(trySplitWaits[i]);
                        }
                        else
                        {


                            textSplitWaits.Add(StringExtensions.ParseFloat(closingTagSplit[0].Split('=')[1]));
                            textSplits.Add(closingTagSplit[1]);
                        }
                    }

                    string finalText = "";
                    for (int i = 0; i < textSplits.Count; i++)
                    {
                        finalText += textSplits[i];
                    }
                    dialogueText.text = finalText;
                }
                else
                {
                    textSplits.Add(dialogueText.text);
                }
                // sequence.Kill(false);
                if (!KillTypewriterRequested)
                {

                }

                // TECHDEBT: deal with typewriter later; not needed for gh demo.
            //     sequence = DOTween.Sequence();
            //     sequence.Pause();
            //     Tween t;
            //     // no inline waits
            //     if (textSplitWaits.Count == 0)
            //     {
            //         t = DOTween.To(() => dialogueText.maxVisibleCharacters,
            //                        x => dialogueText.maxVisibleCharacters = x,
            //                        dialogueText.text.Length, dialogueText.text.Length /
            //                        GameManager.Instance.Settings.TextSpeed.Value)
            //                .SetEase(Ease.Linear).OnUpdate(() =>
            //    {
            //    });
            //         dialogueUnwrapTweens.Add(t);
            //         sequence.Append(t);
            //     }
            //     else
            //     {
            //         Debug.Log("there are some inline Waits");
            //         // TODO: the split reveal for WAIT is incorrect character counting.
            //         // so after the first WAIT, it won't show up as expected :^
            //         // example: v: Don't bother <<wait=2>> bringing <<wait=2>>those <<wait=2>>cushions <<wait=5>>over, Maja.
            //         int runningCharacterCount = textSplits[0].Length;
            //         for (int i = 0; i < textSplitWaits.Count; i++)
            //         {

            //             if (i != 0)
            //             {
            //                 t = DOTween.To(() => dialogueText.maxVisibleCharacters,
            //                x => dialogueText.maxVisibleCharacters = x,
            //                runningCharacterCount,
            //                textSplits[i].Length / GameManager.Instance.Settings.TextSpeed.Value)
            //                .SetEase(Ease.Linear).SetDelay(textSplitWaits[i]);
            //                 dialogueUnwrapTweens.Add(t);
            //             }
            //             else
            //             {
            //                 t = DOTween.To(() => dialogueText.maxVisibleCharacters,
            //                x => dialogueText.maxVisibleCharacters = x,
            //                runningCharacterCount,
            //                textSplits[i].Length / GameManager.Instance.Settings.TextSpeed.Value)
            //                .SetEase(Ease.Linear);
            //                 dialogueUnwrapTweens.Add(t);
            //             }

            //             sequence.Append(t);

            //             // sequence.AppendInterval(textSplitWaits[i]);
            //             runningCharacterCount += textSplits[i].Length;

            //         }

            //         sequence.Append(DOTween.To(() => dialogueText.maxVisibleCharacters,
            //            x => dialogueText.maxVisibleCharacters = x,
            //            dialogueText.text.Length,
            //            textSplits[textSplits.Count - 1].Length / GameManager.Instance.Settings.TextSpeed.Value)
            //            .SetEase(Ease.Linear));

            //     }
            //     Debug.LogFormat("num text split: {0}, num wait {1}", textSplits.Count, textSplitWaits.Count);

            // }

            // // Debug.Break();
            // Debug.Log("now running sequence");
            // while (!KillTypewriterRequested)
            // {
            //     if (sequence.IsActive())
            //     {
            //         sequence.Play();
            //     }
            //     else
            //     {
            //         break;
            //     }

            //     await UniTask.WaitUntil(() => !sequence.IsActive());
            //     EndLine();
            //     return;
            //     // break;
            }



            EndLine();


        }


        public bool ChoicesStillExist()
        {
            try
            {
                return choiceParent.transform.childCount > 0;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("trying to load scene so choiceParent obj gets desctroyed");
                return false;
            }

        }
        public override void PauseTypewriter()
        {
            // if (sequence.IsActive())
            // {
            //     if (sequence.IsPlaying())
            //     {
            //         sequence.Pause();
            //     }
            // }


        }
        public override void ContinueTypewriter()
        {
            // Debug.Log("tryign to continue typewriter)");
            // if (sequence.IsActive())
            // {
            //     if (!sequence.IsPlaying())
            //     {
            //         sequence.Play();
            //         Debug.Log("resuming typewriter");
            //     }
            // }

        }

        /// <summary>
        /// Reset UI to an empty narrator-like UI.
        /// No portrait, namecard, CTC, and dialogue text shown.
        /// </summary>
        public override void ClearUI()
        {
            ClearText();
            HideCTC();
            portraitPresenter.HidePortrait();
            speakerNameHolder.SetActive(false);
        }

        /// <summary>
        /// Logic for when the dialogue line is done being displayed
        /// </summary>
        void EndLine()
        {
            ShowCTC();
            IsDisplayingLine = false;
            KillTypewriterRequested = false;
        }

        /// <summary>
        /// We're unwrapping text and the player selects again to make all the text appear instantly.
        /// TECHDEBT: this is kinda buggy!
        /// </summary>
        public override void KillTypewriter()
        {
            KillTypewriterRequested = true;
            Debug.Log("kill typewriter");
            // sequence.Complete();
            // remove any inline functions <_< since sometmies this thing gets called before 
            // sequence to unwrap gets created !!!!!!!
            string pattern = @"<<.+?>>(?=\s?[a-zA-Z]?)";
            dialogueText.text = Regex.Replace(dialogueText.text, pattern, "");
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            // sequence.Kill(true);
            // DOTween.KillAll();
            // while (sequence.IsActive())
            // {
            //     Debug.Log("killing sequence");
            //     sequence.Kill(false);
            // }

            // EndLine();
            // DialogueSystemManager.Instance.RunCancellationToken();

        }

        public override async UniTask DisplayChoices(List<Choice> choices, Story story, CancellationToken ct)
        {
            // close mobile qmenu because it gets in the way
#if PLATFORM_ANDROID
        ingameHUDPresenter.ToggleMenuOpen(false);
#endif
            waitingForPlayerToSelectChoice = true;
            choiceParentCanvasGroup.alpha = 0;
            choiceParentCanvasGroup.interactable = false;

            // forcefully turn off skipping for when ppl make  choice in case they accidentally skip past (?)
            GameManager.Instance.SetSkipping(false);
            Debug.LogFormat("number of choices: {0}", choices.Count);
            for (int i = 0; i < choices.Count; ++i)
            {
                var handler = Instantiate(choicePrefab, choiceParent.transform);
                handler.SetActive(true);
                Choice choice = choices[i];
                handler.transform.GetChild(0).GetComponent<TMP_Text>().text = choice.text;
                handler.GetComponent<Button>().onClick.AddListener(()
                =>
                {
                    story.ChooseChoiceIndex(choice.index);
                    DialogueSystemManager.Instance.AddSelectedChoiceToPersistentHistory(choice.text, choice.pathStringOnChoice);
                    DialogueSystemManager.Instance.AddSelectedChoiceToHistory(choice.text);

                    DialogueSystemManager.Instance.InkContinueStory();
                    SelectChoice();

                });
            }
            // TECHDEBT: hardcoded choice animation
            // choiceParentCanvasGroup.DOFade(1, .5f).SetEase(Ease.Linear);
            await UniTask.WaitUntil(() => choiceParent.GetComponentInChildren<CanvasGroup>().alpha == 1);
            choiceParentCanvasGroup.interactable = true;
            HideCTC();

            // Debug.Log("done displaying choices, waiting for player selection");
            await UniTask.WaitUntil(() => waitingForPlayerToSelectChoice == false, cancellationToken: ct);
        }

        /// <summary>
        /// Logic for when the player selects a choice option.
        /// </summary>
        /// <returns></returns>
        public override async UniTaskVoid SelectChoice()
        {

            choiceParentCanvasGroup.interactable = false;
            var children = new List<GameObject>();
            for (int i = 0; i < choiceParent.transform.childCount; i++)
            {
                children.Add(choiceParent.transform.GetChild(i).gameObject);
            }
            // TECHDEBT: hardcoded choice animation
            // choiceParent.GetComponentInChildren<CanvasGroup>().DOFade(0, .5f).SetEase(Ease.Linear);

            // TECHDEBT hardcoded chocie parent animation
            await UniTask.WaitUntil(() => choiceParent.GetComponentInChildren<CanvasGroup>().alpha == 0);
            foreach (var child in children)
            {
                Destroy(child);
            }
            DialogueSystemManager.Instance.ClearCurrentChoices();
            waitingForPlayerToSelectChoice = false;
        }




        private void OnEnable()
        {
            _playerControls.Enable();
        }
        private void OnDisable()
        {
            _playerControls.Disable();
        }

    }
}
