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
public class DialogueHistoryLine
{
    public string speaker = null;
    public string line = "";
    public DialogueHistoryLine() { }
    public DialogueHistoryLine(string line)
    {
        this.line = line;
    }
    public DialogueHistoryLine(string speaker, string line)
    {
        this.speaker = speaker;
        this.line = line;
    }


}
public class DialogueSystem : MonoBehaviour
{
    [SerializeField]
    TextAsset _story;
    [SerializeField]
    List<TextAsset> inkStoryFiles = new List<TextAsset>();
    Story story;
    public Story Story { get { return story; } set { } }
    public static DialogueSystem Instance { get; set; }
    public DialogueUIManager dialogueUIManager;

    public List<DialogueHistoryLine> sessionDialogueHistory = new List<DialogueHistoryLine>();
    private List<DialogueHistoryLine> dialogueHistory = new List<DialogueHistoryLine>();
    public List<DialogueHistoryLine> DialogueHistory { get { return dialogueHistory; } set { this.dialogueHistory = value; } }
    public string currentDialogue = "";
    Hash128 hash128 = new Hash128();
    public bool playerCanContinue = true;

    CancellationTokenSource cts;
    CancellationToken ct;
    System.Diagnostics.Stopwatch stopwatch;
    bool endGame = false;
    [SerializeField]
    void Awake()
    {
        cts = new CancellationTokenSource();
        ct = cts.Token;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // var compiler = new Ink.Compiler(inkStoryFiles, new Compiler.Options
        // {
        //     countAllVisits = true,
        //     fileHandler = new UnityInkFileHandler(Path.GetDirectoryName(inkAbsoluteFilePath))
        // });
        // Ink.Runtime.Story story = compiler.Compile();


        story = new Story(_story.text);
        dialogueUIManager = GetComponent<DialogueUIManager>();
        sessionDialogueHistory.Clear();

        dialogueUIManager.DisableCTC();
        playerCanContinue = false;

        // RunCancellationToken();

        // MC_NPC_SO mc = (MC_NPC_SO) GameManager.Instance.NamedCharacterDatabase[NPC_NAME.MC];
        // story.variablesState["mc_name"] = mc.DisplayName;
        // Debug.LogFormat("mc name si now: {0}", (string)story.variablesState["mc_name"]);

    }
    async UniTaskVoid Start()
    {
        // RunCancellationToken();
        var brain = GameObject.FindObjectOfType<IngameSceneBrain>();
        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        // Debug.LogError("waiting for brain to set up scene");
        await UniTask.WaitWhile(() => !brain.IsDoneSettingScene);
        // await UniTask.WaitWhile(() => !AssetRefLoader.IsDoneLoadingCharacters);
        // Debug.LogError("done waiting for brain: " + stopwatch.ElapsedMilliseconds.ToString());
        Debug.LogFormat("do we have a current save: {0}", SaveLoadManager.Instance.currentSave != null);

        await UniTask.WaitUntil(() => !SceneTransitionManager.Instance.IsLoading);
        SettingsManager.Instance.EnableSettingsUIControls();
        
        if (SaveLoadManager.Instance.currentSave != null)
        {
            if (DialogueSystem.Instance.NeedToDisplayChoices())
            {

                DialogueSystem.Instance.DisplayChoices().Forget();

                await dialogueUIManager.ShowUI();
                await dialogueUIManager.DisplayLine(DialogueSystem.Instance.Story.currentText, ct);
                dialogueUIManager.HideCTC();
                SceneTransitionManager.Instance.FadeIn(2f);

            }
            else if (DialogueSystem.Instance.NeedToRunActionFunction())
            {
                await DialogueSystem.Instance.RunActionFunction();
                await SceneTransitionManager.Instance.FadeIn(2f);


            }
            else
            {
                await SceneTransitionManager.Instance.FadeIn(2f);
                DialogueSystem.Instance.RunRegularLine().Forget();
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

    [Button]
    public void RunCancellationToken()
    {
        cts.Cancel();
        Debug.LogFormat("is cts cancellation requested {0}", cts.IsCancellationRequested);
        Debug.Log("cancel pls... ds...");
        cts.Dispose();
        cts = new CancellationTokenSource();
        ct = cts.Token;
        IsContinueStoryRunning = false;
        Debug.LogFormat("is cts cancellation requested after creating new cts {0}", cts.IsCancellationRequested);
    }

    public void AddSelectedChoiceToHistory(string choice)
    {
        choice = "<color=#cfda5e>" + choice + "</color>";
        var log = new DialogueHistoryLine();
        log.speaker = "";
        log.line = choice;
        dialogueHistory.Add(log);
        sessionDialogueHistory.Add(log);

    }
    public void AddDialogueToHistory(DialogueHistoryLine log)
    {
        // remove any inline << >> commands
        string pattern = @"<<.+?>>(?=\s?[a-zA-Z]?)";
        log.line = Regex.Replace(log.line, pattern, "");
        dialogueHistory.Add(log);
        sessionDialogueHistory.Add(log);
    }
    public DialogueHistoryLine FormatChoiceForHistory(string choice)
    {
        return new DialogueHistoryLine("<style='choiceHistory'>" + choice + "</style>");
    }
    public DialogueHistoryLine FormatDialogueTextForHistory(string log)
    {
        // dialogueUIManager.
        return new DialogueHistoryLine(log);
    }

    public bool IsProcessingLine = true;
    public bool IsRunningActionFunction = false;
    public bool IsContinueStoryRunning = false;
    public async UniTaskVoid ContinueStory()
    {

        IsContinueStoryRunning = true;
        InkContinueStory();

        while (true && !endGame)
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

            // }
            // Debug.Break();
            if (NeedToRunActionFunction())
            {
                stopwatch.Restart();
                Debug.Log("actually running an action function now");
                IsRunningActionFunction = true;
                await RunActionFunction();
                IsRunningActionFunction = false;
                Debug.Log("time to run action function: " + stopwatch.ElapsedMilliseconds.ToString());
            }
            else if (NeedToDisplayChoices())
            {
                dialogueUIManager.EnableCTC();
                Debug.Log("actually displaying choices now");
                Debug.Log("need to display choicesssss");
                stopwatch.Restart();
                // TECHDEBT: something wrong with choice collection above.
                // why does the line before choices section get combined into 
                // current choices collection?
                await DisplayLine();
                await DisplayChoices();
                Debug.Log("time to run display choices: " + stopwatch.ElapsedMilliseconds.ToString());

                // if we just made a choice, we need to wait for the choicebox to go away so that we don't have
                // errors when spam clicking
                // await UniTask.WaitWhile(() => dialogueUIManager.ChoicesStillExist());
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.07f), cancellationToken: ct);
                Debug.Log("actually runnnig a regular line now");
                stopwatch.Restart();
                dialogueUIManager.EnableCTC();
                await RunRegularLine();
                Debug.Log("time to run regular line: " + stopwatch.ElapsedMilliseconds.ToString());



            }
            IsProcessingLine = false;



        }

        // if (NeedToDisplayChoices() && !dialogueUIManager.waitingForPlayerToSelectChoice)
        //     {
        //         // TECHDEBT: something wrong with choice collection above.
        //         // why does the line before choices section get combined into 
        //         // current choices collection?
        //         await DisplayLine();
        //         await DisplayChoices();
        //     }
    }

    public async UniTaskVoid InkContinueStory()
    {
        waitingToContinueStory = false;
        if (cts.IsCancellationRequested)
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }
        if (VideoManager.Instance.IsVideoPlaying)
        {
            return;
        }
        if (endGame)
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
            }
            else
            {
                Debug.LogError("why can't i continue story?");
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
        GameManager.Instance.Settings.chosenChoices.Add(choice + "_" + pathString);
    }
    public bool PreviouslySelectedChoice(string choice)
    {
        if (GameManager.Instance.Settings.chosenChoices.Contains(choice))
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
        if (GameManager.Instance.Settings.SeenText.Contains(
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
        (GameManager.Instance.IsSkipping && (!GameManager.Instance.Settings.skipAllText && DialogueSystem.Instance.CurrentTextSeenBefore()))));
        // Debug.Log(story.currentText);
        dialogueUIManager.currentTags = story.currentTags;
        // TODO: Turn this into an await for animation to show UI
        if (!dialogueUIManager.IsShowingUI)
        {
            await dialogueUIManager.ShowUI();
        }
        this.currentDialogue = story.currentText;

        await dialogueUIManager.DisplayLine(story.currentText, ct);
        if (!CurrentTextSeenBefore())
        {
            GameManager.Instance.Settings.SeenText.Add(CreateHash(story.currentText + "_" + story.state.currentPathString));
        }
        await UniTask.WaitUntil(() => !dialogueUIManager.IsDisplayingLine, cancellationToken: this.ct);

    }
    public bool waitingToContinueStory = false;
    public async UniTask RunRegularLine()
    {
        waitingToContinueStory = true;
        await DisplayLine();
        await UniTask.Yield();

        if (GameManager.Instance.IsAuto)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.GlobalDefinitions.delayBeforeAutoNextLine *
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
        await ActionFunction(story.currentText);
        // await UniTask.Yield();
        InkContinueStory();
    }
    public bool NeedToRunActionFunction()
    {
        if (story.currentText.StartsWith(">> "))
        {
            return true;
        }
        return false;
    }
    public bool IsDisplayingLine { get { return dialogueUIManager.IsDisplayingLine; } }

    public void ClearText()
    {
        dialogueUIManager.ClearText();
    }

    public NPC_SO GetNPC(string npcName)
    {
        return GameManager.Instance.GetNPC(npcName);
    }
    public async UniTask ActionFunction(string text)
    {
        text = text.TrimStart('>').TrimStart(null).TrimEnd(null);
        var p = text.Split(null);

        var funcName = p[0];
        var pLength = p.Length;

        // by default all our functions are blocking. check if final parameter is true
        bool blocking = true;
        var blockingParse = false;
        bool tryParseResult = false;
        blockingParse = bool.TryParse(p[p.Length - 1], out blocking);
        if (!blockingParse)
        {
            blocking = true;
        }

        bool blockingParamPresent = false;
        bool locationParamPresent = false;
        bool durationParamPresent = false;

        string transition = "";
        string charName = "";
        int numParams = 0;
        int channel = 0;
        float timeToDelay = .4f; // TECHDEBT: turn into global var

        // TECHDEBT: trn into globl var
        float duration = .35f;
        switch (funcName)
        {
            case "playOP":
                await VideoManager.Instance.PlayVideo();
                break;
            case "toggleTint":
                ImageManager.Instance.SetTint(bool.Parse(p[1]));

                break;
            case "stop":
                // stop music duration?
                // stop ambient duration? channel?
                if (pLength == 4) //[0]stop [1]ambient [2]duration [3]channel (4)
                {
                    duration = StringExtensions.ParseFloat(p[2]);
                    channel = int.Parse(p[3]);
                }
                else if (pLength == 3) //[0]stop [1]ambient/music [2]duration (3)
                {
                    duration = StringExtensions.ParseFloat(p[2]);
                }
                if (p[1] == "music")
                {
                    AudioManager.Instance.StopMusic(duration);
                }
                else if (p[1] == "ambient")
                {
                    AudioManager.Instance.StopAmbient(fadeout: duration, channel: channel);
                }
                break;
            case "sfx":
                AudioManager.Instance.PlaySFX(p[1]);
                break;
            case "music":
                if (pLength == 3)
                {
                    duration = StringExtensions.ParseFloat(p[2]);
                }
                else
                {
                    duration = 0;
                }
                AudioManager.Instance.PlayMusic(p[1], duration);
                break;
            case "ambient":
                channel = 0;
                if (pLength == 3) //[0] ambient [1]soundname [2]dur (3)
                {

                    if (int.TryParse(p[2], out channel))
                    {
                        duration = 0;
                    }
                    else
                    {
                        duration = StringExtensions.ParseFloat(p[2]);
                        channel = 0;
                    }

                }
                else if (pLength == 4) //[0]ambient [1]soundname [2]dur [3]channel (4)
                {
                    channel = int.Parse(p[3]);
                    duration = StringExtensions.ParseFloat(p[2]);
                }
                else //[0] ambient [1] soundname (2)
                {
                    duration = 0;
                }
                AudioManager.Instance.PlayAmbient(p[1], channel: channel, fadein: duration);
                break;
            case "hideAllChar":
                float? dur = null;
                if (p.Length == 3)
                {
                    dur = StringExtensions.ParseFloat(p[2]);
                }
                ImageManager.Instance.HideAllChar(dur);
                break;
            case "hideChar":
                // TODO: turn into await
                charName = p[1].TrimStart(null).TrimEnd(null);

                // TODO: we don't support transitions. need for NEXT GAME
                dur = null;
                if (p.Length == 3)
                {
                    if (!bool.TryParse(p[2], out tryParseResult))
                    {
                        dur = StringExtensions.ParseFloat(p[2]);
                    }

                }
                // if (p.Length > 2)
                // {
                //     if (p.Length == 4)
                //     {
                //         transition = p[2];
                //         duration = StringExtensions.ParseFloat(p[3]);
                //     }
                //     else
                //     {
                //         if (!float.TryParse(p[2], out duration))
                //         {
                //             transition = p[2];
                //         }
                //     }
                // }
                ImageManager.Instance.HideChar(charName, duration: duration);
                break;
            case "showChar":
                numParams = p.Length;
                blockingParamPresent = false;
                durationParamPresent = false;

                charName = p[1].TrimStart(null).TrimEnd(null); ;
                var newPosition = StringExtensions.ParseVector3(p[2]);

                if (numParams == 6)
                {
                    blocking = bool.Parse(p[5]);
                    duration = StringExtensions.ParseFloat(p[4]);
                    transition = p[3];
                }
                else if (numParams == 5)
                {
                    if (!bool.TryParse(p[4], out blocking))
                    {
                        blocking = true;
                        duration = StringExtensions.ParseFloat(p[4]);
                        transition = p[3];
                    }
                    else
                    {
                        if (!float.TryParse(p[3], out duration))
                        {
                            duration = .35f;
                            transition = p[3];
                        }
                        else
                        {

                        }

                    }

                }
                else if (numParams == 4)
                {
                    if (float.TryParse(p[3], out duration))
                    {

                    }
                    else if (bool.TryParse(p[3], out blocking))
                    {
                        duration = .35f;
                    }
                    else
                    {
                        transition = p[3];
                        blocking = true;
                    }
                }
                if (blocking)
                {
                    await ImageManager.Instance.ShowChar(charName, newPosition, transition, duration);
                }
                else
                {
                    ImageManager.Instance.ShowChar(charName, newPosition, transition, duration);
                }
                break;
            case "spawnChar":
                // syntax: spawnChar <charName> <any number of expressions> dur:<duration?> loc:<location?> <blocking?>
                // dialogueUIManager.ClearUI();
                // is the last parameter for blocking?
                numParams = p.Length;

                blockingParamPresent = false;
                locationParamPresent = false;
                durationParamPresent = false;
                Vector3? loc = null;
                if (bool.TryParse(p[numParams - 1], out blocking))
                {
                    blockingParamPresent = true;
                }
                else
                {
                    blocking = true;
                }

                // extract out dur and loc if they exist
                int durIDX, locIDX;
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i].Contains("dur:"))
                    {
                        durIDX = i;
                        duration = StringExtensions.ParseFloat(p[i].Split(':')[1]);
                        durationParamPresent = true;
                    }
                    else if (p[i].Contains("loc:"))
                    {
                        locIDX = i;
                        loc = StringExtensions.ParseVector3(p[i].Split(':')[1]);
                        locationParamPresent = true;
                    }
                }

                string exp = "";
                int numExpElements = numParams - 2;
                if (blockingParamPresent)
                {
                    numExpElements -= 1;
                }
                if (locationParamPresent)
                {
                    numExpElements -= 1;
                }
                if (durationParamPresent)
                {
                    numExpElements -= 1;
                }


                // if (p.Length > 3)
                // {
                //     if (numExpElements == 0)
                //     {
                //         numExpElements = 1;
                //     }
                //     exp = String.Join(" ", new ArraySegment<string>(p, 2, numExpElements));

                // }
                // else if (p.Length == 3)
                // {
                //     exp = p[2];
                // }

                exp = String.Join(" ", new ArraySegment<string>(p, 2, numExpElements));





                Debug.LogFormat("{0},{1},{2}, {3}", p[1], exp, loc, blocking);


                if (!locationParamPresent)
                {
                    loc = null;
                }
                // TODO: add duration for spawn so its not always a .35f dissolve for action spot
                if (blocking)
                {
                    await ImageManager.Instance.SpawnChar(p[1], exp, loc);
                }
                else
                {
                    ImageManager.Instance.SpawnChar(p[1], exp, loc);
                }

                break;
            case "ec":
                dialogueUIManager.ClearUI();
                // dur = null;
                // if (p.Length == 4)
                // {
                //     bool res = bool.TryParse(p[3], out blocking);
                //     if (!res)
                //     {
                //         blocking = true;
                //     }
                // }
                // try
                // {
                //     dur = StringExtensions.ParseFloat(p[3]);
                // }
                // catch
                // {
                //     Debug.Log("no duration given for expression change, going with defaults");
                //     dur = GameManager.Instance.GlobalDefinitions.expressionChangeDuration;
                // }

                // if (blocking)
                // {
                //     await ImageManager.Instance.ExpressionChange(p[1], String.Join(" ", new ArraySegment<string>(p, 2, p.Length - 2)), dur);
                // }
                // else
                // {
                    ImageManager.Instance.ExpressionChange(p[1], String.Join(" ", new ArraySegment<string>(p, 2, p.Length - 2)));
                // }

                break;
            case "pause":
                dialogueUIManager.ClearUI();
                if (!GameManager.Instance.IsSkipping)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(StringExtensions.ParseFloat(p[1])), cancellationToken: this.ct);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
                }
                break;
            case "wh":
                // TODO: make await for animation
                timeToDelay = .4f;
                if (p.Length == 2)
                {
                    timeToDelay = StringExtensions.ParseFloat(p[1]);
                }
                else if (p.Length == 3)
                {
                    duration = StringExtensions.ParseFloat(p[2]);
                }
                await dialogueUIManager.HideUI(duration);

                if (!GameManager.Instance.IsSkipping)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(timeToDelay), cancellationToken: this.ct);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
                }
                break;
            case "ws":
                // TODO make await for animation
                dialogueUIManager.ClearUI();
                timeToDelay = .4f;
                if (p.Length == 2)
                {
                    timeToDelay = StringExtensions.ParseFloat(p[1]);
                }
                else if (p.Length == 3)
                {
                    duration = StringExtensions.ParseFloat(p[2]);
                }
                await dialogueUIManager.ShowUI(duration);
                if (!GameManager.Instance.IsSkipping)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(timeToDelay), cancellationToken: this.ct);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
                }
                break;
            case "shot":
                // TECHDEBT: hardcoded to at least clear textbox and namecard before a shot
                DialogueSystem.Instance.dialogueUIManager.ClearUI();
                Debug.Log("running shot action?");
                if (blocking)
                {
                    await ImageManager.Instance.ShowBG(p[1], p[2], StringExtensions.ParseFloat(p[3]));
                }
                else
                {
                    ImageManager.Instance.ShowBG(p[1], p[2], StringExtensions.ParseFloat(p[3]));
                }

                break;
            case "clear":
                dialogueUIManager.ClearUI();
                if (pLength < 2)
                {
                    duration = GameManager.Instance.GlobalDefinitions.delayBeforeAutoNextLine;
                }
                else
                {
                    duration = StringExtensions.ParseFloat(p[1]);
                }
                if (!GameManager.Instance.IsSkipping)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: this.ct);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
                }
                break;
            case "clear_p":
                dialogueUIManager.ClearUI();
                if (pLength < 2)
                {
                    duration = GameManager.Instance.GlobalDefinitions.delayBeforeAutoNextLine;
                }
                else
                {
                    duration = StringExtensions.ParseFloat(p[1]);
                }

                if (!GameManager.Instance.IsSkipping)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: this.ct);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
                }

                break;
            case "shakeCam":
                if (p.Length > 1)
                {
                    ImageManager.Instance.ShakeCam(StringExtensions.ParseFloat(p[1]));
                }
                else
                {
                    ImageManager.Instance.ShakeCam();
                }
                // TODO: change to blocking

                break;
            case "moveCam":
                ImageManager.Instance.MoveCam(p[1], StringExtensions.ParseVector3(p[2]), StringExtensions.ParseFloat(p[3]));
                break;
            case "playBG":
                // TODO: add blocking/await
                ImageManager.Instance.PlayBGTween();
                break;
            case "unlockCG":
                try
                {
                    GameManager.Instance.Settings.cgDict[p[1].TrimStart(null).TrimEnd(null)] = true;
                }
                catch
                {
                    Debug.LogFormat("Cant unlock cg {0}", p[1]);
                }

                break;
            case "loadScene":
                Debug.Log("try to load scene?");
                SceneTransitionManager.Instance.LoadScene(p[1], StringExtensions.ParseFloat(p[2]),
                StringExtensions.ParseFloat(p[3]));
                endGame = true;
                break;
            case "disablePlayerInput":
                dialogueUIManager.DisableCTC();
                playerCanContinue = false;
                break;
            case "enablePlayerInput":
                dialogueUIManager.EnableCTC();
                playerCanContinue = true;
                break;
            case "showAd":
                #if PLATFORM_ANDROID
                playerCanContinue = false;
                AdManager.Instance.ShowAdPopupToContinue();
                await UniTask.WaitWhile(() => AdManager.Instance.IsTryingToRunAd);
                playerCanContinue = true;
                #endif
                break;
            case "loadAd":
                #if PLATFORM_ANDROID
                AdManager.Instance.RequestInterstitial();
                #endif
                break;
            case "endGame":
                GameManager.Instance.SetSkipping(false);
                GameManager.Instance.SetAuto(false);
                SettingsManager.Instance.DisableSettingsUIControls();
                break;
            default:
                Debug.LogErrorFormat("action function {0} doesn't exist", funcName);
                break;
        }
    }


}
