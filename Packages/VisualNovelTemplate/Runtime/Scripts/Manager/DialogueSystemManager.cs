using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Ink.Runtime;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// Manages dialogue system and all ink interfacing.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{


    public class DialogueSystemManager : MonoBehaviour
    {
        [SerializeField]
        [InfoBox("The master ink file. If you split your script across multiple files, make sure the one attached here is the main/starting/intro point of all your ink scripts!")]
        TextAsset _story;
        Story story;
        public Story Story { get { return story; } set { } }
        public static DialogueSystemManager Instance { get; set; }

        /// <summary>
        /// The dialogueUIManager of the currently active dialogue textbox window mode.
        /// </summary>
        private DialogueUIManager dialogueUIManager;
        public DialogueUIManager DialogueUIManager { get { return dialogueUIManager; } }

        [SerializeField]
        [PropertyTooltip("The different types of dialogue textbox UIs we want to use, such as ADV and NVL.")]
        List<DialogueWindowMode_SO> dialogueWindowModes = new List<DialogueWindowMode_SO>();
        /// <summary>
        /// Used by inkscript to activate a specific dialogue mode
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="GameObject"></typeparam>
        /// <returns></returns>
        Dictionary<string, GameObject> dialogueWindows = new Dictionary<string, GameObject>();

        /// <summary>
        /// The dialogue lines that we have seen in a current play session. When you load a save, the lines seen in that save file are set to this variable.
        /// </summary>
        /// <typeparam name="DialogueHistoryLine"></typeparam>
        /// <returns></returns>
        public List<DialogueHistoryLine> currentSessionDialogueHistory = new List<DialogueHistoryLine>();

        /// <summary>
        /// Used to block player input, e.g. if we don't want them to be able to skip the OP (on first playthrough...)
        /// </summary>
        private bool playerCanContinue = true;
        public bool PlayerCanContinue { get { return playerCanContinue; } }

        /// <summary>
        /// If we need to end the game, we want to ensure Ink no longer runs and such...?
        /// </summary>
        private bool endGame = false;
        public bool EndGame { get { return endGame; } }
        /// <summary>
        /// The Dialogue that we have already parsed for any characters, sprites, and extracted out the actual dialogue text line.
        /// TODO: turn this into a list of dialogue so we can process inline waits? or have some external function processors for that...
        /// </summary>
        public Dialogue CurrentProcessedDialogue;
        /// <summary>
        /// Any custom functions that we can't use ink functions for.
        /// </summary>
        CustomActionFunctions customActionFunctions;


        /* Utilities */
        CancellationTokenSource cts;
        CancellationToken ct;
        System.Diagnostics.Stopwatch stopwatch;
        Hash128 hash128 = new Hash128();
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            cts = new CancellationTokenSource();
            ct = cts.Token;

            story = new Story(_story.text);
            currentSessionDialogueHistory.Clear();

            customActionFunctions = GetComponent<CustomActionFunctions>();

            // Spawn all the dialogue windows we want to use ingame and deactivate them.
            // Hold a reference to them so we can select the one to use through ink!
            GameObject window;
            foreach (var dialogueWindowMode in dialogueWindowModes)
            {
                window = Instantiate(dialogueWindowMode.prefab, this.transform);
                dialogueWindows[dialogueWindowMode.internalName] = window;
                // Set our default dialogue ui window 
                if (dialogueWindowMode.internalName == GameManager.Instance.DefaultConfig.defaultDialogueWindow.internalName)
                {
                dialogueUIManager = window.GetComponentInChildren<DialogueUIManager>();
                }
                window.GetComponentInChildren<DialogueUIManager>().HideUI();
            }


            // RunCancellationToken();

        }

        async UniTaskVoid Start()
        {

        }

        // URGENT: Need to update this because DSM is now persistent across all scenes, not just ingame!!!
        public async UniTaskVoid RunStory()
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // await UniTask.WaitWhile(() => !AssetRefLoader.IsDoneLoadingCharacters);
            Debug.LogFormat("do we have a current save: {0}", SaveLoadManager.Instance.currentSave != null);

            await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
            MenuManager.Instance.EnableSettingsUIControls();

            if (SaveLoadManager.Instance.currentSave != null)
            {
                if (DialogueSystemManager.Instance.NeedToDisplayChoices())
                {

                    DialogueSystemManager.Instance.DisplayChoices().Forget();

                    await dialogueUIManager.ShowUI();
                    await dialogueUIManager.DisplayLine(ct);
                    dialogueUIManager.HideCTC();
                    SceneTransitionManager.Instance.FadeIn(GameManager.Instance.DefaultConfig.sceneFadeInDuration);

                }
                else if (DialogueSystemManager.Instance.NeedToRunActionFunction())
                {
                    await DialogueSystemManager.Instance.RunActionFunction();
                    await SceneTransitionManager.Instance.FadeIn(GameManager.Instance.DefaultConfig.sceneFadeInDuration);


                }
                else
                {
                    await SceneTransitionManager.Instance.FadeIn(GameManager.Instance.DefaultConfig.sceneFadeInDuration);
                    DialogueSystemManager.Instance.RunRegularLine().Forget();
                }
                Debug.Log("fading in ds from a save");
            }
            else
            {
                Debug.Log("fading in from ds no save");

                await SceneTransitionManager.Instance.FadeIn(0f);
                ContinueStory().Forget();
            }
        }


        /// <summary>
        /// An attempt to cancel/stop running inkscript/diualogue lines... only works sometimes???
        /// TECHDEBT
        /// </summary>
        [Button]
        public void RunCancellationToken()
        {
            cts.Cancel();
            Debug.LogFormat("is cts cancellation requested {0}", cts.IsCancellationRequested);
            cts.Dispose();
            cts = new CancellationTokenSource();
            ct = cts.Token;
            IsContinueStoryRunning = false;
            Debug.LogFormat("is cts cancellation requested after creating new cts {0}", cts.IsCancellationRequested);
        }

        /// <summary>
        /// TECHDEBT: Hacky hard coded way of giving a custom choice history style
        /// Probably should replace with an actual stylesheet.
        /// </summary>
        /// <param name="choice"></param>
        public void AddSelectedChoiceToHistory(string choice)
        {
            choice = string.Format("<color={0}>{1}</color>", GameManager.Instance.DefaultConfig.historyChoiceColor, choice);
            var log = new DialogueHistoryLine();
            log.speaker = "";
            log.line = choice;
            GameManager.Instance.PersistentGameData.chosenChoices.Add(log.line);
            currentSessionDialogueHistory.Add(log);

        }

        /// <summary>
        /// After the player has seen a dialogue line, save it to history in-memory. 
        /// THIS DOES NOT DISPLAY THESE LINES IN A HISTORY LOG AUTOMATICALLY.
        /// </summary>
        /// <param name="log"></param>
        public void AddDialogueToHistory(DialogueHistoryLine log)
        {
            // remove any inline << >> commands
            string pattern = @"<<.+?>>(?=\s?[a-zA-Z]?)";
            log.line = Regex.Replace(log.line, pattern, "");
            GameManager.Instance.PersistentGameData.seenText.Add(log.line);
            currentSessionDialogueHistory.Add(log);
        }

        /// <summary>
        /// We might want to give a custom choice history style.
        /// NOT IMPLEMENTED!!! SWAP TO DEFAULTCONFIG!!!
        /// </summary>
        /// <param name="choice"></param>
        /// <returns></returns>
        public DialogueHistoryLine FormatChoiceForHistory(string choice)
        {
            return new DialogueHistoryLine("<style='choiceHistory'>" + choice + "</style>");
        }

        /// <summary>
        /// We might want to do some fancy custom formatting for display in the history log?
        /// Not implemented!!!
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public DialogueHistoryLine FormatDialogueTextForHistory(string log)
        {
            return new DialogueHistoryLine(log);
        }
        [Button]
        public void RunStoryNode(string node)
        {
            story.ChoosePathString(node);
            ContinueStory().Forget();
        }
        public bool IsProcessingLine = true;
        public bool IsRunningActionFunction = false;
        public bool IsContinueStoryRunning = false;
        public async UniTaskVoid ContinueStory()
        {

            IsContinueStoryRunning = true;
            InkContinueStory();

            stopwatch = new System.Diagnostics.Stopwatch();

            while (true && !EndGame)
            {




                // Debug.Log("loop stuck before first delay?");
                // HACK: prevent skip from going too fast which can break everything
                // Debug.Log(ct.IsCancellationRequested);
                // Debug.LogFormat("need to display choices: {0}", NeedToDisplayChoices());

                // if (dialogueUIManager.KillTypewriter())
                // {
                //     return;
                // }

                // Debug.Log("st no longer loading, pls run ds continue story");
                IsProcessingLine = true;
                // if (story.canContinue)
                // {

                if (story.currentChoices.Count > 0)
                {
                    while (story.canContinue)
                    {
                        Debug.Log("collection choices");
                        story.Continue();
                    }
                }

                Debug.LogFormat("Do we need to display choices?: {0}", NeedToDisplayChoices());

                // }
                // Debug.Break();
                if (NeedToRunActionFunction())
                {
                    stopwatch.Restart();
                    // Debug.Log("actually running an action function now");
                    IsRunningActionFunction = true;
                    await RunActionFunction();
                    IsRunningActionFunction = false;
                    // Debug.Log("time to run action function: " + stopwatch.ElapsedMilliseconds.ToString());
                }
                else if (NeedToDisplayChoices())
                {
                    dialogueUIManager.EnableCTC();
                    // Debug.Log("actually displaying choices now");
                    stopwatch.Restart();
                    // TECHDEBT: something wrong with choice collection above.
                    // why does the line before choices section get combined into 
                    // current choices collection?
                    // await DisplayLine();
                    await DisplayChoices();
                    // Debug.Log("time to run display choices: " + stopwatch.ElapsedMilliseconds.ToString());

                    // if we just made a choice, we need to wait for the choicebox to go away so that we don't have
                    // errors when spam clicking
                    // await UniTask.WaitWhile(() => dialogueUIManager.ChoicesStillExist());
                }
                else
                {
                    // Debug.Log("actually runnnig a regular line now");
                    stopwatch.Restart();
                    dialogueUIManager.EnableCTC();
                    await RunRegularLine();
                    // Debug.Log("time to run regular line: " + stopwatch.ElapsedMilliseconds.ToString());



                }
                IsProcessingLine = false;



            }
        }

        public async UniTaskVoid InkContinueStory()
        {
            waitingToContinueStory = false;
            if (cts.IsCancellationRequested)
            {
                cts = new CancellationTokenSource();
                ct = cts.Token;
            }
            if (VideoPlayerManager.Instance.IsVideoPlaying)
            {
                return;
            }
            if (EndGame)
            {
                IsContinueStoryRunning = false;
                return;
            }
            if (IsContinueStoryRunning)
            {
                Debug.Log("continue the ink story");
                if (story.canContinue)
                {
                    story.Continue();
                    Debug.Log("run story.Continue()");
                }
                else
                {
                    Debug.LogError("why can't i continue story? try to display choices?");

                    if (NeedToDisplayChoices())
                    {
                        Debug.Log("displaying some weird choice stuff");
                        dialogueUIManager.EnableCTC();
                        Debug.Log("actually displaying choices now");
                        stopwatch.Restart();
                        // TECHDEBT: something wrong with choice collection above.
                        // why does the line before choices section get combined into 
                        // current choices collection?
                        // await DisplayLine();
                        await DisplayChoices();
                        Debug.Log("time to run display choices: " + stopwatch.ElapsedMilliseconds.ToString());

                        // if we just made a choice, we need to wait for the choicebox to go away so that we don't have
                        // errors when spam clicking
                        // await UniTask.WaitWhile(() => dialogueUIManager.ChoicesStillExist());
                    }

                }

            }
            else
            {
                ContinueStory();
                Debug.Log("continue the async story");
            }

        }

        public void ClearCurrentChoices()
        {
            story.currentChoices.Clear();
        }
        public bool NeedToDisplayChoices()
        {
            if (story.currentChoices.Count > 0)
            {
                return true;
            }
            return false;
        }
        public async UniTask DisplayChoices()
        {
            Debug.Log("about to display choices");
            await dialogueUIManager.DisplayChoices(story.currentChoices, story, ct);
        }
        public void AddSelectedChoiceToPersistentHistory(string choice, string pathString)
        {
            var line = choice + "_" + pathString;
            GameManager.Instance.PersistentGameData.chosenChoices.Add(line);
        }
        public bool PreviouslySelectedChoice(string choice, string pathString)
        {
            var line = choice + "_" + pathString;
            if (GameManager.Instance.PersistentGameData.chosenChoices.Contains(line))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CurrentTextSeenBefore()
        {
            if (GameManager.Instance.PersistentGameData.seenText.Contains(
                CreateHash(story.currentText + "_" + story.state.currentPathString)
            ))
            {
                return true;
            }
            return false;
        }
        public string CreateHash(string s)
        {
#if DEVELOPMENT_BUILD
        return s;
#endif
            hash128 = new Hash128();
            hash128.Append(s);
            return hash128.ToString();
        }
        public async UniTask DisplayLine()
        {
            Debug.LogFormat("Seen text before: {0}; {1} ", CurrentTextSeenBefore(),
            ((GameManager.Instance.IsSkipping && GameManager.Instance.Settings.skipAllText) ||
            (GameManager.Instance.IsSkipping && (!GameManager.Instance.Settings.skipAllText && DialogueSystemManager.Instance.CurrentTextSeenBefore()))));
            // Debug.Log(story.currentText);
            dialogueUIManager.CurrentTags = story.currentTags;
            // TODO: Turn this into an await for animation to show UI
            if (!dialogueUIManager.IsShowingUI)
            {
                Debug.Log("are we stuck waiting to show ui?");
                await dialogueUIManager.ShowUI();
                Debug.Log("done showing ui");
            }

            Debug.Log("please display line now");
            await dialogueUIManager.DisplayLine(ct);
            Debug.Log("done displaing line");
            if (!CurrentTextSeenBefore())
            {
                GameManager.Instance.PersistentGameData.seenText.Add(CreateHash(story.currentText + "_" + story.state.currentPathString));
            }
            Debug.Log("now wait until dialogue is not displaying line still");
            await UniTask.WaitUntil(() => !dialogueUIManager.IsDisplayingLine, cancellationToken: this.ct);
            Debug.Log("finelly we are done with display line function");

        }
        public bool waitingToContinueStory = false;
        public async UniTask RunRegularLine()
        {
            waitingToContinueStory = true;
            CurrentProcessedDialogue = ProcessDialogue(story.currentText);
            await DisplayLine();
            await UniTask.Yield();

            if (GameManager.Instance.IsAuto)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine *
                 (1 - GameManager.Instance.Settings.AutoSpeed.Value + .04)), cancellationToken: this.ct);
                InkContinueStory();
            }
            else if (GameManager.Instance.IsSkipping)
            {

                InkContinueStory();
            }
            else
            {
                await UniTask.WaitWhile(() => waitingToContinueStory, cancellationToken: this.ct);
            }
        }

        public async UniTask RunActionFunction()
        {
            Debug.LogFormat("now running actionfunction: {0}", story.currentText);
            if (customActionFunctions == null)
            {
                Debug.LogError("Custom action functions are not defined!!");
            }
            else
            {
                await customActionFunctions.ActionFunction(story.currentText, this.ct);
            }
            InkContinueStory();
        }

        /// <summary>
        /// If an inkLine starts with `>> `, then we want to run a custom action function!
        /// The game will wait until the action function is done running before it automatically
        /// moves to the next line.
        /// </summary>
        /// <returns></returns>
        public bool NeedToRunActionFunction()
        {
            if (story.currentText.StartsWith(">> "))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Are we in the process of displaying a line, such as with a typewriter?
        /// </summary>
        /// <value></value>
        public bool IsDisplayingLine { get { return dialogueUIManager.IsDisplayingLine; } }

        public void ClearText()
        {
            dialogueUIManager.ClearText();
        }

        public NPC_SO GetNPC(string npcName)
        {
            return GameManager.Instance.GetNPC(npcName);
        }

        public void SetPlayerCanContinue(bool value)
        {
            playerCanContinue = value;
        }
        public void SetEndGame(bool value)
        {
            endGame = value;
        }
        public void ShowDialogueWindow(string internalName)
        {
            try
            {
                var window = dialogueWindows[internalName];
                dialogueUIManager = window.GetComponentInChildren<DialogueUIManager>();
                if (dialogueUIManager != null)
                {
                    dialogueUIManager.ShowUI();
                }
                else
                {
                    Debug.LogWarningFormat("unable to locate dialogueui manager for window {0}", internalName);
                }

            }
            catch
            {
                Debug.LogErrorFormat("dialogue window [{0}] is not registered.", internalName);
            }
        }
        public void HideDialogueWindow()
        {
            dialogueUIManager.HideUI();
        }



        /// <summary>
        /// Parse an ink line into displayable text and any commands.
        /// Our writing style is: <speaker name> <expressions> <transition duration>: <dialogue text>
        /// Perhaps in the future we will support transition types, if you want to do something other 
        /// than a dissolve/fade between expressions...
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Dialogue ProcessDialogue(string text)
        {
            Dialogue dialogue = new Dialogue();
            var splitText = text.Split(null);
            int colonIDX = -1;
            for (int i = 0; i < splitText.Length; i++)
            {
                if (splitText[i].EndsWith(":"))
                {
                    colonIDX = i;
                    break;
                }
            }
            Debug.Log("location of colon: " + colonIDX.ToString());
            // Debug.Break();
            if (colonIDX > -1) // TECHDEBT: hard coding max number of params for colon split <_<
            {
                dialogue.speaker = splitText[0].TrimEnd(':');

                if (colonIDX > 0)
                {
                    string exps = "";
                    for (int i = 1; i < colonIDX + 1; i++)
                    {
                        // Debug.Break();
                        if (i == colonIDX)
                        {
                            bool res;
                            float dur;
                            res = float.TryParse(splitText[i].TrimEnd(':'), out dur);
                            if (!res)
                            {
                                exps += splitText[i].TrimEnd(':') + " ";
                                dialogue.duration = -1;
                            }
                            else
                            {
                                dialogue.duration = dur;
                            }

                        }
                        else
                        {
                            exps += splitText[i].TrimEnd(':') + " ";
                        }

                    }
                    dialogue.expression = exps;
                }
                else
                {
                    dialogue.expression = "";
                }

                string s = "";
                for (int i = colonIDX + 1; i < splitText.Length; i++)
                {
                    s += splitText[i] + " ";
                }
                s = s.TrimStart(null).TrimEnd(null);
                dialogue.text = s;
            }
            else
            {
                dialogue.speaker = "narrator";
                dialogue.text = text;
            }
            dialogue.npc = DialogueSystemManager.Instance.GetNPC(dialogue.speaker);
            return dialogue;
        }




    }

}