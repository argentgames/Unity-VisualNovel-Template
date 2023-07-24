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
using UniRx;
using UnityEngine.Events;

/// <summary>
/// Manages dialogue system and all ink interfacing.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{


    public class DialogueSystemManager : MonoBehaviour
    {
        bool doneInit = false;
        public bool DoneInit => doneInit;

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
        private StringReactiveProperty currentDialogueWindow = new StringReactiveProperty("");
        public StringReactiveProperty CurrentDialogueWindow { get { return currentDialogueWindow; } set { currentDialogueWindow = value; } }
        public HashSet<string> VisibleUIWindows = new HashSet<string>();

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
        public Dialogue? CurrentProcessedDialogue;
        /// <summary>
        /// Any custom functions that we can't use ink functions for.
        /// </summary>
        CustomActionFunctions customActionFunctions;
        public CustomActionFunctions CustomActionFunctions => customActionFunctions;

        /// <summary>
        /// Did the player open the QMenu? If so, we might want to auto open the qmenu when we show the window
        /// </summary>
        bool playerOpenedQmenu = false;
        public bool PlayerOpenedQMenu => playerOpenedQmenu;
        public void SetPlayerOpenedQmenu(bool val)
        {
            playerOpenedQmenu = val;
        }
        /// <summary>
        /// Used when we load a save and need to run the RunContinueGame for the first time
        /// </summary>
        bool isLoadedGame = false;
        public bool IsLoadedGame => isLoadedGame;
        public void SetIsLoadedGame(bool val)
        {
            isLoadedGame = val;
        }

        public UnityEvent OnBeforeRestartGame, OnAfterRestartGame;

        /* Utilities */
        CancellationTokenSource cts;
        CancellationToken ct;
        System.Diagnostics.Stopwatch stopwatch;
        Hash128 hash128 = new Hash128();
        [Sirenix.OdinInspector.Button]
        public void RestartGame()
        {
            OnBeforeRestartGame?.Invoke();

            story = new Story(_story.text);
            story.ChoosePathString(GameManager.Instance.DefaultConfig.startSceneName);
            SetEndGame(false);
            IsProcessingLine = false;


            // SpawnAllUIWindows().Forget();
            HideAllDialogueWindows();
            ResetAllDialogueWindows();
            // TODO, URGENT: need to add a ResetAllUIWindows 
            currentSessionDialogueHistory.Clear();

            SaveLoadManager.Instance.currentSave.inkData = story.state.ToJson();

            OnAfterRestartGame?.Invoke();
        }


        public async UniTask SpawnAllUIWindows()
        {
            // make sure there don't exist windows already before we spawn everything
            foreach (var win in dialogueWindows.Values)
            {
                Destroy(win);
                // win.GetComponentInChildren<DialogueUIManager>().ResetUI();
            }
            dialogueWindows.Clear();

            // Debug.Break();

            // Spawn all the dialogue windows we want to use ingame and deactivate them.
            // Hold a reference to them so we can select the one to use through ink!
            GameObject window;
            foreach (var dialogueWindowMode in dialogueWindowModes)
            {
                // Debug.Break();
                window = Instantiate(dialogueWindowMode.prefab, this.transform);
                var debugName = window.gameObject.name;
                var windowCanvas = window.GetComponentInChildren<Canvas>();
                windowCanvas.sortingOrder = -100;
                dialogueWindows[dialogueWindowMode.internalName] = window;
                // Set our default dialogue ui window 
                if (dialogueWindowMode.internalName == GameManager.Instance.DefaultConfig.defaultDialogueWindow.internalName)
                {
                    dialogueUIManager = window.GetComponentInChildren<DialogueUIManager>();
                }

                try
                {
                    window.GetComponentInChildren<Canvas>().sortingOrder = GameManager.Instance.DefaultConfig.dialogueUISortOrder;
                }
                catch
                {
                    Debug.LogErrorFormat("window {0} doesn't have a canvas?", window.name);
                }

                await UniTask.Yield();
                try
                {
                    // window.GetComponentInChildren<DialogueUIManager>().HideUI(0f);
                    // window.GetComponentInChildren<DialogueUIManager>().ClearUI();
                    try
                    {
                        windowCanvas.sortingOrder = GameManager.Instance.DefaultConfig.dialogueUISortOrder;
                    }
                    catch
                    {
                        Debug.LogErrorFormat("window {0} doesn't have a canvas?", window.name);
                    }
                }
                catch (MissingReferenceException)
                {
                    Debug.LogErrorFormat("window {0} was destroyed asynchronously...", debugName);
                }



            }

            HideAllDialogueWindows();
        }
        public static event Action<Story> OnCreateStory;
        async UniTaskVoid Awake()
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

            // this auto attaches the ink inspector window which is annoying
            // if(OnCreateStory != null) OnCreateStory(story);

            currentSessionDialogueHistory.Clear();

            customActionFunctions = GetComponent<CustomActionFunctions>();

            await UniTask.WaitUntil(() => GameManager.Instance != null);

            await SpawnAllUIWindows();

            doneInit = true;

            // Debug.Break();
            // RunCancellationToken();

        }

        async UniTaskVoid Start()
        {

        }

        public async UniTaskVoid RunStory()
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // await UniTask.WaitWhile(() => !AssetRefLoader.IsDoneLoadingCharacters);
            Debug.LogFormat("do we have a current save: {0}", SaveLoadManager.Instance.currentSave != null);

            await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
            Debug.Log("scene transition manager has finished loading");
            MenuManager.Instance.EnableSettingsUIControls();
            SetDialogueWindow(currentDialogueWindow.Value);

            RunContinueStory().Forget();

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
            if (IsRunningActionFunction)
            {
                IsRunningActionFunction = false;
            }
            customActionFunctions.CancelDelay();
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
            endGame = false;
            story.ChoosePathString(node);
            RunContinueStory().Forget();
        }
        [Button]
        public void JumpToStoryNode(string node)
        {
            story.ChoosePathString(node);
        }
        public bool IsProcessingLine = true;
        public bool IsRunningActionFunction = false;
        public bool IsContinueStoryRunning = false;
        bool firstTimeSeeingChoices = false;
        /// <summary>
        /// LEGACY NEED TO DELETE
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid ContinueStory()
        {

            IsContinueStoryRunning = true;
            InkContinueStory();

            stopwatch = new System.Diagnostics.Stopwatch();

            IsProcessingLine = false;

            while (true && !EndGame)
            {

                if (IsProcessingLine)
                {
                    continue;
                }


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

                // if (story.currentChoices.Count > 0)
                // {
                //     while (story.canContinue)
                //     {
                //         Debug.Log("collection choices");
                //         story.Continue();
                //     }
                // }
                var needToDisplayChoices = NeedToDisplayChoices();
                Debug.LogFormat("Do we need to display choices?: {0}", needToDisplayChoices);
                // if (needToDisplayChoices)
                // {
                //     firstTimeSeeingChoices = !firstTimeSeeingChoices;
                // }


                // }
                // Debug.Break();
                // if (NeedToRunActionFunction())
                // {
                //     stopwatch.Restart();
                //     Debug.Log("actually running an action function now");
                //     IsRunningActionFunction = true;
                //     await RunActionFunction();
                //     IsRunningActionFunction = false;
                //     Debug.Log("time to run action function: " + stopwatch.ElapsedMilliseconds.ToString());
                // }

                // else if (needToDisplayChoices)
                // {
                //     // if (firstTimeSeeingChoices)
                //     // {
                //     //     continue;
                //     // }
                //     dialogueUIManager.EnableCTC();
                //     // Debug.Log("actually displaying choices now");
                //     stopwatch.Restart();
                //     // TECHDEBT: something wrong with choice collection above.
                //     // why does the line before choices section get combined into 
                //     // current choices collection?
                //     // await DisplayLine();
                //     Debug.Log("is this display choices running? line 364");
                //     await DisplayChoices();
                //     // Debug.Log("time to run display choices: " + stopwatch.ElapsedMilliseconds.ToString());

                //     // if we just made a choice, we need to wait for the choicebox to go away so that we don't have
                //     // errors when spam clicking
                //     // await UniTask.WaitWhile(() => dialogueUIManager.ChoicesStillExist());
                // }

                // else
                {
                    // Debug.Log("actually runnnig a regular line now");
                    stopwatch.Restart();
                    dialogueUIManager.EnableCTC();

                    CurrentProcessedDialogue = ProcessDialogue(story.currentText);
                    await RunRegularLine(CurrentProcessedDialogue);
                    Debug.Log("done running regular line");
                    // Debug.Log("time to run regular line: " + stopwatch.ElapsedMilliseconds.ToString());



                }
                IsProcessingLine = false;



            }
        }

        /// <summary>
        /// LEGACY NEED TO DELETE
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid InkContinueStory()
        {
            // Debug.Log("running inkcontinuestory");
            // waitingToContinueStory = false;
            // if (cts.IsCancellationRequested)
            // {
            //     Debug.Log("ctc requested for dsm");
            //     cts = new CancellationTokenSource();
            //     ct = cts.Token;
            // }
            // // if (VideoPlayerManager.Instance.IsVideoPlaying)
            // // {
            // //     Debug.Log("not conitnuing ink story yet; video is playing!");
            // //     return;
            // // }
            // if (EndGame)
            // {
            //     Debug.Log("time to end the game");
            //     IsContinueStoryRunning = false;
            //     return;
            // }
            // if (IsContinueStoryRunning)
            // {
            //     Debug.Log("continue the ink story");
            //     if (story.canContinue)
            //     {
            //         story.Continue();
            //         Debug.Log("run story.Continue()");
            //     }
            //     else
            //     {
            //         Debug.LogError("why can't i continue story? try to display choices?");

            //         if (NeedToDisplayChoices() && !dialogueUIManager.WaitingForPlayerToSelectChoice)
            // {
            //     // Debug.Log("displaying some weird choice stuff");
            //     dialogueUIManager.EnableCTC();
            //     Debug.Log("actually displaying choices now");
            //     stopwatch.Restart();
            //     // TECHDEBT: something wrong with choice collection above.
            //     // why does the line before choices section get combined into 
            //     // current choices collection?
            //     // await DisplayLine();
            //     await DisplayChoices();
            //     // Debug.Log("time to run display choices: " + stopwatch.ElapsedMilliseconds.ToString());

            //     // if we just made a choice, we need to wait for the choicebox to go away so that we don't have
            //     // errors when spam clicking
            //     // await UniTask.WaitWhile(() => dialogueUIManager.ChoicesStillExist());
            // }
            //         else
            //         {
            //             Debug.Log("display line that got lumped together wit hchoices collection?");

            //             CurrentProcessedDialogue = ProcessDialogue(story.currentText);
            //             await DisplayLine((Dialogue)CurrentProcessedDialogue);
            //         }

            //     }

            // }
            // else
            // {
            //     ContinueStory();
            //     Debug.Log("continue the async story");
            // }

        }

        public async UniTaskVoid RunContinueStory()
        {
            Debug.Log("running node: " + story.state.currentPathString);
            // infinite loop to run process the story
            while (true)
            {
                Debug.Log("running node: " + story.state.currentPathString);
                if (story.canContinue && !IsLoadedGame)
                {
                    story.Continue();
                    Debug.Log("run story.Continue()");
                }
                // if we need to end the game, finally quit this function
                if (EndGame)
                {
                    Debug.Log("time to end the game");
                    IsContinueStoryRunning = false;
                    return;
                }

                // turn off skipping if setting is to only skip seen text
                if (!GameManager.Instance.Settings.skipAllText)
                {
                    if (!CurrentTextSeenBefore())
                    {
                        if (GameManager.Instance.IsSkipping)
                        {
                            GameManager.Instance.SetSkipping(false);
                        }

                    }
                }

                GameManager.Instance.PersistentGameData.seenText.Add(
                    CreateHash(story.currentText + "_" + story.state.currentPathString)
                );
                

                // is it an action function and thus we want to automatically evaluate it without any user input?
                if (!IsLoadedGame && NeedToRunActionFunction())
                {
                    Debug.Log("actually running an action function now");
                    IsRunningActionFunction = true;
                    await RunActionFunction();
                    IsRunningActionFunction = false;
                    Debug.Log("done running action function");

                    // force see if there are any choices to run?
                    // without this the game hangs on the while(true) loop because
                    // you get stuck trying to run ink node <null>
                    // this is because of weird bug where choices are collected one line early
                    if (story.currentChoices.Count > 0)
                    {
                        await RunRegularLine(null);
                    }

                }
                // otherwise it's going to be a regular line that may or may not include choices
                else
                {

                    Debug.Log("actually runnnig a regular line now");
                    if (IsLoadedGame)
                    {

                        Debug.LogFormat("do wes need to display current saved line: {0}",
                        dialogueUIManager.DisplayLineBeforeChoiceOnLoad || !NeedToDisplayChoices());

                        if (dialogueUIManager.DisplayLineBeforeChoiceOnLoad || !NeedToDisplayChoices())
                        {
                            CurrentProcessedDialogue = (Dialogue)SaveLoadManager.Instance.currentSave.currentDialogue;
                        }
                        else
                        {
                            CurrentProcessedDialogue = null;
                        }
                        

                        ShowDialogueWindow(currentDialogueWindow.Value);

                        // Debug.Break();

                        Debug.LogFormat("our courrent duimanger is: {0}, isShowingUI value: {1}", dialogueUIManager.name, dialogueUIManager.IsShowingUI);
                        // display ui?
                        if (!dialogueUIManager.IsShowingUI)
                        {
                            Debug.Log("are we stuck waiting to show ui?");

                            await dialogueUIManager.ShowUI();
                            Debug.Log("done showing ui");
                        }
                        else
                        {
                            Debug.Log("FORCE DIALOGUE UI SHOW");
                            await dialogueUIManager.ShowUI();
                        }
                    }
                    else
                    {
                        CurrentProcessedDialogue = ProcessDialogue(story.currentText);
                    }

                    await RunRegularLine(CurrentProcessedDialogue);
                    Debug.Log("done running regular line");
                }

                // finally add the line to our persistent seen text so we know when to stop skipping
                AddCurrentStoryTextToPersistentHistory();
                // only save persistent after every line
                SaveLoadManager.Instance.SavePersistent();
                if (IsLoadedGame)
            {
                Debug.Log("toggling off IsLoadedGame flag");
                SetIsLoadedGame(false);
            }
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
        public void AddCurrentStoryTextToPersistentHistory()
        {
            var line = CreateHash(story.currentText + "_" + story.state.currentPathString);
            GameManager.Instance.PersistentGameData.seenText.Add(line);
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
            if (GameManager.Instance == null)
            {
                return false;
            }
            if (GameManager.Instance.PersistentGameData == null)
            {
                return false;
            }
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
        public async UniTask DisplayLine(Dialogue dialogue)
        {
            Debug.LogFormat("Seen text before: {0}; {1} ", CurrentTextSeenBefore(),
            ((GameManager.Instance.IsSkipping && GameManager.Instance.Settings.skipAllText) ||
            (GameManager.Instance.IsSkipping && (!GameManager.Instance.Settings.skipAllText && DialogueSystemManager.Instance.CurrentTextSeenBefore()))));
            // Debug.Log(story.currentText);
            dialogueUIManager.CurrentTags = story.currentTags;
            // TODO: Turn this into an await for animation to show UI
            // if (!dialogueUIManager.IsShowingUI)
            // {
            //     // Debug.Log("are we stuck waiting to show ui?");

            //     dialogueUIManager.PlayerAllowedToHideUI = true;
            //     await dialogueUIManager.ShowUI();
            //     // Debug.Log("done showing ui");
            // }

            // Debug.Log("please display line now");
            await dialogueUIManager.DisplayLine(dialogue, ct);
            // Debug.Log("done displaing line");
            // Debug.Log("now wait until dialogue is not displaying line still");
            await UniTask.WaitUntil(() => !dialogueUIManager.WaitingForPlayerContinueStory, cancellationToken: this.ct);
            // Debug.Log("finelly we are done with display line function");

        }
        public bool waitingToContinueStory = false;
        public async UniTask RunRegularLine(Dialogue? dialogue)
        {
            waitingToContinueStory = true;

            if (dialogue != null)
            {
                if (((Dialogue)dialogue).text != "")
                {
                    await DisplayLine((Dialogue)dialogue); // ctc will end this by setting isdisplayling line to false

                    // this ugly block was causing lot of delay in performance
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                // await UniTask.Yield();  
                //     await UniTask.Yield();
                }

            }


            if (story.currentChoices.Count > 0)
            {
                if (GameManager.Instance.IsAuto)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine *
                     (1 - GameManager.Instance.Settings.AutoSpeed.Value)), cancellationToken: this.ct);
                    // InkContinueStory();
                }
                await DisplayChoices();
            }

            if (GameManager.Instance.IsAuto)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine *
                      (1 - GameManager.Instance.Settings.AutoSpeed.Value)), cancellationToken: this.ct);
                // InkContinueStory();
            }
            else if (GameManager.Instance.IsSkipping)
            {

                // InkContinueStory();
            }
            else
            {
                await UniTask.WaitWhile(() => waitingToContinueStory, cancellationToken: this.ct);
            }
        }
        public async UniTask RunDisplayChoicesOnlyForSaveLoad()
        {
            dialogueUIManager.ShowUI();
            if (story.currentChoices.Count > 0)
            {
                if (GameManager.Instance.IsAuto)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine *
                     (1 - GameManager.Instance.Settings.AutoSpeed.Value)), cancellationToken: this.ct);
                    // InkContinueStory();
                }
                await DisplayChoices();
            }

            if (GameManager.Instance.IsAuto)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine *
                      (1 - GameManager.Instance.Settings.AutoSpeed.Value)), cancellationToken: this.ct);
                // InkContinueStory();
            }
            else if (GameManager.Instance.IsSkipping)
            {

                // InkContinueStory();
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
                Debug.LogFormat("available ct: {0}", ct.CanBeCanceled);
                await customActionFunctions.ActionFunction(story.currentText, this.ct);
            }
            // InkContinueStory();
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
        [Button]
        public void SetEndGame(bool value)
        {
            endGame = value;
        }
        /// <summary>
        /// Set the current active dialogue window. Can only haev one at a time.
        /// If you set it to null, we want to clear out all VNControlSubscriptions and hide everything
        /// </summary>
        /// <param name="internalName"></param>
        public void SetDialogueWindow(string internalName)
        {
            Debug.Log("setting dialogue window to: " + internalName);
            try
            {
                // unregister vngameplaymap from all windows and then enable it in the appropriate window
                foreach (var win in dialogueWindows.Values)
                {
                    win.GetComponentInChildren<DialogueUIManager>().RemoveVNControlSubscriptions();
                }
                if (internalName == "")
                {
                    foreach (var win in dialogueWindows.Values)
                    {
                        win.GetComponentInChildren<DialogueUIManager>().HideUI(0);
                    }
                    currentDialogueWindow.Value = null;
                }
                else
                {
                    var window = dialogueWindows[internalName];
                    currentDialogueWindow.Value = internalName;
                    dialogueUIManager = window.GetComponentInChildren<DialogueUIManager>();
                    dialogueUIManager.AddVNControlSubscriptions();
                }

            }
            catch
            {
                Debug.LogErrorFormat("dialogue window [{0}] is not registered.", internalName);
            }
        }
        public async UniTask ShowDialogueWindow(string internalName)
        {
            try
            {
                SetDialogueWindow(internalName);
                if (dialogueUIManager != null)
                {
                    float duration = -1f;
                    if (GameManager.Instance.IsSkipping)
                    {
                        duration = 0;
                    }
                    await dialogueUIManager.ShowUI(duration);
                    dialogueUIManager.PlayerAllowedToHideUI = true;

                    VisibleUIWindows.Add(internalName);
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
        public async UniTask HideDialogueWindow(float duration = -1)
        {
            if (VisibleUIWindows.Contains(
                CurrentDialogueWindow.Value
            ))
            {
                VisibleUIWindows.Remove(CurrentDialogueWindow.Value);
            }

            dialogueUIManager.PlayerAllowedToHideUI = false;
            if (duration == -1)
            {
                await dialogueUIManager.HideUI();
            }
            else
            {
                await dialogueUIManager.HideUI(duration);
            }

        }
        public async UniTask HideAllDialogueWindows()
        {
            List<UniTask> windowHiding = new List<UniTask>();
            foreach (var window in dialogueWindows.Values)
            {
                windowHiding.Add(window.transform.GetComponentInChildren<DialogueUIManager>().HideUI(0));
            }
            await UniTask.WhenAll(windowHiding);
        }
        public void ResetAllDialogueWindows()
        {
            foreach (var window in dialogueWindows.Values)
            {
                window.GetComponentInChildren<DialogueUIManager>().ResetUI();
            }
        }
        public async UniTask LoadDialogueWindowStates(string saveIndex)
        {
            List<UniTask> windowLoading = new List<UniTask>();
            foreach (var window in dialogueWindows.Values)
            {
                windowLoading.Add(window.GetComponentInChildren<DialogueUIManager>()
                .Load(saveIndex));
                Debug.Log("loading dialogue window: " + window.name);
            }
            await UniTask.WhenAll(windowLoading);
            VisibleUIWindows.Clear();
            foreach (var win in SaveLoadManager.Instance.currentSave.currentVisibleDialogueUIs)
            {
                VisibleUIWindows.Add(win);
            }
            // Debug.Break();
        }
        public void SaveDialogueWindowStates(string saveIndex)
        {
            SaveLoadManager.Instance.currentSave.currentVisibleDialogueUIs.Clear();
            foreach (var win in VisibleUIWindows)
            {
                SaveLoadManager.Instance.currentSave.currentVisibleDialogueUIs.Add(win);
            }
            foreach (var kv in dialogueWindows)
            {
                var window = kv.Value;
                var duim = window.GetComponentInChildren<DialogueUIManager>();
                
                duim.Save(saveIndex);

            }
           
        }
        public async UniTask ShowNVLWindow(string internalName)
        {
            try
            {
                if (dialogueUIManager == null)
                {
                    var window = dialogueWindows[internalName];
                    dialogueUIManager = window.GetComponentInChildren<DialogueUIManager>();
                    currentDialogueWindow.Value = internalName;
                }
                float duration = -1f;
                if (GameManager.Instance.IsSkipping)
                {
                    duration = 0;
                }
                await dialogueUIManager.ShowNVL(duration);
                dialogueUIManager.PlayerAllowedToHideUI = true;
            }
            catch
            {
                Debug.LogErrorFormat("dialogue window [{0}] is not registered.", internalName);
            }
        }
        public async UniTask HideNVLWindow()
        {
            dialogueUIManager.PlayerAllowedToHideUI = false;
            await dialogueUIManager.HideNVL();
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
            Debug.Log(text);
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