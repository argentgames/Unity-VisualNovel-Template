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
using UnityEngine.UI.Extensions;
using DG.Tweening;
using System.Threading;
using System.Text.RegularExpressions;
public class DialogueUIManager : MonoBehaviour
{
    [SerializeField]
    AssetReference choicePrefab;
    [SerializeField]
    Image textboxBG, speakerNameBG;

    [SerializeField]
    TMP_Text dialogueText, speakerText;
    [SerializeField]
    GameObject choiceParent, ctcImage, UIHolder, speakerNameHolder;
    [SerializeField]
    Button clickToContinue;
    ImageManager imageManager;
    public PortraitPresenter portraitPresenter;
    [SerializeField]
    float dialogueTextLeftMarginForPortraitMode = 402.2328f;
    float portraitNameX = -391f, namecardX = -808f;
    public List<string> currentTags = new List<string>();
    List<Tween> dialogueUnwrapTweens = new List<Tween>();

    // Start is called before the first frame update
    Material textboxMaterial, speakerNameMaterial;

    [SerializeField]
    List<CanvasGroup> uiElementsToToggle = new List<CanvasGroup>();
    Sequence sequence, dialogueBoxSequence;
    CanvasGroup choiceParentCanvasGroup;
    PlayerControls _playerControls;
    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
 
    public IngameHUDPresenter ingameHUDPresenter;

    void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.UI.HideIngameUI.performed += ctx =>
       {
           ToggleUI();
           GameManager.Instance.SetAuto(false);
           GameManager.Instance.SetSkipping(false);
       };
       _playerControls.UI.Click.performed += ctx =>
       {Debug.Log("player click so toggle UI");
           if (!UIHolder.activeSelf)
           {
               if (GameObject.Find("Popup") == null && GameObject.Find("Ad") == null && DialogueSystem.Instance.playerCanContinue){

                ToggleUI();
               }
           }
       };
    }
    void Start()
    {

        clickToContinue.OnClickAsObservable()
    .Subscribe(_ =>
        {
            CTCLogic();

        }).AddTo(this);

        imageManager = GameObject.FindObjectOfType<ImageManager>();
        textboxMaterial = textboxBG.material;
        speakerNameMaterial = speakerNameBG.material;
        sequence = DOTween.Sequence();
        dialogueBoxSequence = DOTween.Sequence();
        HideUI(0);
        choiceParentCanvasGroup = choiceParent.GetComponentInChildren<CanvasGroup>();
    }

    async UniTaskVoid CTCLogic()
    {
        Debug.Log("clicked");
        // await UniTask.Yield();
        // stopwatch.Stop();
        var wasSkipping = GameManager.Instance.IsSkipping;
            GameManager.Instance.SetSkipping(false);
        
      
            GameManager.Instance.SetAuto(false);
        
        if (IsDisplayingLine)
        {
            Debug.Log("is displaying line, killing typewriter!");
            // DialogueSystem.Instance.RunCancellationToken();
            KillTypewriter();
        }

        // else if (stopwatch.Elapsed.Milliseconds < 500)
        // {
        //     stopwatch.Restart();
        //     return;
        // } 
        // else if (DialogueSystem.Instance.IsProcessingLine)
        // {
        //     return;
        // }
        else if (DialogueSystem.Instance.IsRunningActionFunction)
        {
            Debug.Log("running action function, do nothing except turn off skipping");
            //     Debug.Log("trying to skip over any bg tween stuffs |:");
            //     ImageManager.Instance.NeedToCompleteTweensEarly = true;
            //     await UniTask.Delay(TimeSpan.FromSeconds(.1f));
            //     DialogueSystem.Instance.ContinueStory();
            // HideCTC();
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
                Debug.Log("continue story from ctc");
            DialogueSystem.Instance.InkContinueStory();
            HideCTC();
            DisableCTC();
            ClearText();
            }
            
        }
    }

    public void DisableCTC()
    {
        Debug.Log("disable ctc");
        clickToContinue.gameObject.SetActive(false);
    }
    public void EnableCTC()
    {
        clickToContinue.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }
    private void OnDisable()
    {
        _playerControls.Disable();
    }

    public void HideCTC()
    {
        ctcImage.SetActive(false);
    }
    public void ShowCTC()
    {
        ctcImage.SetActive(true);
        EnableCTC();
    }
    public async UniTask HideUI(float duration = .3f)
    {
        if (UIHolder.activeSelf)
        {
            if (GameManager.Instance.IsSkipping)
            {
                duration = 0.002f;
            }
            if (!dialogueBoxSequence.IsActive())
            {
                dialogueBoxSequence = DOTween.Sequence();
            }
            foreach (var cg in uiElementsToToggle)
            {
                dialogueBoxSequence.Join(cg.DOFade(0, duration));
            }
            

            dialogueBoxSequence.AppendCallback(() => UIHolder.SetActive(false));
            dialogueBoxSequence.Play();


            portraitPresenter.HidePortrait();
        }

    }
    public void ToggleMobileUI()
    {

    }
    bool portraitCurrentlyShowing = false;
    public void ToggleUI()
    {Debug.Log("calling toggle ui");
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
    public void ShowUIWithoutClearing(float duration = .3f)
    {
        UIHolder.SetActive(true);
        dialogueBoxSequence = DOTween.Sequence();
        foreach (var cg in uiElementsToToggle)
        {
            dialogueBoxSequence.Join(cg.DOFade(1, duration));
        }

        dialogueBoxSequence.Play();

    }
    public async UniTask ShowUI(float duration = .3f)
    {
        dialogueText.text = "";
        HideCTC();
        speakerText.text = "";
        imageManager.HidePortrait();
        speakerNameHolder.SetActive(false);
        UIHolder.SetActive(true);
        if (dialogueBoxSequence.IsActive())
        {
            dialogueBoxSequence.Complete();
        }

        dialogueBoxSequence = DOTween.Sequence();

        if (GameManager.Instance.IsSkipping)
        {
            duration = 0.002f;
        }
        foreach (var cg in uiElementsToToggle)
        {
            dialogueBoxSequence.Join(cg.DOFade(1, duration));
        }

        dialogueBoxSequence.Play();


    }

    public bool IsShowingUI
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
            dialogue.text = string.Format("{0}{1}{2}", GameManager.Instance.GenericTexts.Texts[GenericText.START_QUOTE_CHAR],
            s, GameManager.Instance.GenericTexts.Texts[GenericText.END_QUOTE_CHAR]);
        }
        else
        {
            dialogue.speaker = "narrator";
            dialogue.text = text;
        }
        dialogue.npc = DialogueSystem.Instance.GetNPC(dialogue.speaker);
        return dialogue;
    }


    // CancellationTokenSource ct = new CancellationTokenSource();
    public bool IsDisplayingLine = false;
    public async UniTask DisplayLine(string text, CancellationToken ct)
    {
        ClearText();
        IsDisplayingLine = true;
        bool needToShowSpeakerName = false;
        HideCTC();
        // Debug.Log(text);
        var dialogue = ProcessDialogue(text);


        await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.GlobalDefinitions.delayBeforeShowText), cancellationToken: this.GetCancellationTokenOnDestroy());

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
        if (dialogue.npc is SpriteNPC_SO)
        {
            needToShowPortrait = await imageManager.ExpressionChange(dialogue.npc.internalName, dialogue.expression, dialogue.duration);
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

        if (needToShowPortrait && !(bool)DialogueSystem.Instance.Story.variablesState["cgmode"])
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
        DialogueSystem.Instance.AddDialogueToHistory(historyLog);

        if ((bool)DialogueSystem.Instance.Story.variablesState["cgmode"])
        {
            needToShowPortrait = false;
        }

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
        (GameManager.Instance.IsSkipping && (DialogueSystem.Instance.CurrentTextSeenBefore())) ||
        KillTypewriterRequested)
        {
            Debug.LogFormat("WHY ARE YOU SKIPING: isSkipping {0}, skipAllText {1}, seenText {2}",
            GameManager.Instance.IsSkipping, GameManager.Instance.Settings.skipAllText, DialogueSystem.Instance.CurrentTextSeenBefore());
            if (needToShowPortrait)
            {
                portraitPresenter.ShowChar(dialogue.npc.npcName);
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
            // DialogueSystem.Instance.InkContinueStory();
        }
        else
        {
            GameManager.Instance.SetSkipping(false);
            // Set TMP maxVisibleCharacters to 0
            dialogueText.maxVisibleCharacters = 0;

            // if (portraitPresenter.IsShowingPortrait)
            // {
            if (needToShowPortrait && !(bool)DialogueSystem.Instance.Story.variablesState["cgmode"])
            {
                portraitPresenter.ShowChar(dialogue.npc.npcName);
            }
            // }

            if (needToShowSpeakerName)
            {
                // TODO: add npc specific color?
                speakerText.text = dialogue.npc.DisplayName;
                speakerText.color = dialogue.npc.NameColor;
                speakerText.gameObject.transform.parent.gameObject.SetActive(true);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.GlobalDefinitions.delayBeforeShowText));

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
            sequence = DOTween.Sequence();
            sequence.Pause();
            Tween t;
            if (textSplitWaits.Count == 0)
            {
                t = DOTween.To(() => dialogueText.maxVisibleCharacters,
                               x => dialogueText.maxVisibleCharacters = x,
                               dialogueText.text.Length, text.Length /
                               GameManager.Instance.Settings.TextSpeed.Value)
                       .SetEase(Ease.Linear).OnUpdate(() =>
           {
           });
                dialogueUnwrapTweens.Add(t);
                sequence.Append(t);
            }
            else
            {
                Debug.Log("there are some inline Waits");
                // TODO: the split reveal for WAIT is incorrect character counting.
                // so after the first WAIT, it won't show up as expected :^
                // example: v: Don't bother <<wait=2>> bringing <<wait=2>>those <<wait=2>>cushions <<wait=5>>over, Maja.
                int runningCharacterCount = textSplits[0].Length;
                for (int i = 0; i < textSplitWaits.Count; i++)
                {

                    if (i != 0)
                    {
                        t = DOTween.To(() => dialogueText.maxVisibleCharacters,
                       x => dialogueText.maxVisibleCharacters = x,
                       runningCharacterCount,
                       textSplits[i].Length / GameManager.Instance.Settings.TextSpeed.Value)
                       .SetEase(Ease.Linear).SetDelay(textSplitWaits[i]);
                        dialogueUnwrapTweens.Add(t);
                    }
                    else
                    {
                        t = DOTween.To(() => dialogueText.maxVisibleCharacters,
                       x => dialogueText.maxVisibleCharacters = x,
                       runningCharacterCount,
                       textSplits[i].Length / GameManager.Instance.Settings.TextSpeed.Value)
                       .SetEase(Ease.Linear);
                        dialogueUnwrapTweens.Add(t);
                    }

                    sequence.Append(t);

                    // sequence.AppendInterval(textSplitWaits[i]);
                    runningCharacterCount += textSplits[i].Length;

                }

                sequence.Append(DOTween.To(() => dialogueText.maxVisibleCharacters,
                   x => dialogueText.maxVisibleCharacters = x,
                   text.Length,
                   textSplits[textSplits.Count - 1].Length / GameManager.Instance.Settings.TextSpeed.Value)
                   .SetEase(Ease.Linear));

            }
            Debug.LogFormat("num text split: {0}, num wait {1}", textSplits.Count, textSplitWaits.Count);

        }

        // Debug.Break();
        Debug.Log("now running sequence");
        while (!KillTypewriterRequested)
        {
            if (sequence.IsActive())
            {
                sequence.Play();
            }
            else
            {
                break;
            }

            await UniTask.WaitUntil(() => !sequence.IsActive());
            EndLine();
            return;
            // break;
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
    public void PauseTypewriter()
    {
        if (sequence.IsActive())
        {
            if (sequence.IsPlaying())
            {
                sequence.Pause();
            }
        }


    }
    public void ContinueTypewriter()
    {
        Debug.Log("tryign to continue typewriter)");
        if (sequence.IsActive())
        {
            if (!sequence.IsPlaying())
            {
                sequence.Play();
                Debug.Log("resuming typewriter");
            }
        }

    }
    public void ClearText()
    {
        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
    }
    public void ClearUI()
    {
        ClearText();
        HideCTC();
        portraitPresenter.HidePortrait();
        speakerNameHolder.SetActive(false);
    }
    void EndLine()
    {
        // DOTween.KillAll(false);

        // foreach (var t in dialogueUnwrapTweens)
        // {
        //     if (t.IsActive())
        //     {
        //         t.Kill(false);
        //     }
        // }
        // dialogueUnwrapTweens.Clear();

        Debug.Log("CALL END LINE");
        ShowCTC();
        // clickToContinue.Select();
        IsDisplayingLine = false;
        KillTypewriterRequested = false;
    }
    bool KillTypewriterRequested = false;
    public void KillTypewriter()
    {
        KillTypewriterRequested = true;
        Debug.Log("kill typewriter");
        // sequence.Complete();
        // remove any inline functions <_< since sometmies this thing gets called before 
            // remove any inline functions <_< since sometmies this thing gets called before 
        // remove any inline functions <_< since sometmies this thing gets called before 
        // sequence to unwrap gets created !!!!!!!
        string pattern = @"<<.+?>>(?=\s?[a-zA-Z]?)";
        dialogueText.text = Regex.Replace(dialogueText.text, pattern, "");
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        // sequence.Kill(true);
        // DOTween.KillAll();
        while (sequence.IsActive())
        {
            Debug.Log("killing sequence");
            sequence.Kill(false);
        }

        // EndLine();
        // DialogueSystem.Instance.RunCancellationToken();

    }
    public bool waitingForPlayerToSelectChoice = false;
    public async UniTask DisplayChoices(List<Choice> choices, Story story, CancellationToken ct)
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
            var handler = await AssetRefLoader.Instance.LoadAsset(choicePrefab, choiceParent.transform); // await choicePrefab.InstantiateAsync(choiceParent.transform);
            handler.SetActive(true);
            Choice choice = choices[i];
            handler.transform.GetChild(0).GetComponent<TMP_Text>().text = choice.text;
            handler.GetComponent<Button>().onClick.AddListener(()
            =>
            {
                story.ChooseChoiceIndex(choice.index);
                DialogueSystem.Instance.AddSelectedChoiceToPersistentHistory(choice.text, choice.pathStringOnChoice);
                DialogueSystem.Instance.AddSelectedChoiceToHistory(choice.text);

                DialogueSystem.Instance.InkContinueStory();
                SelectChoice();

            });
        }
        // TECHDEBT: hardcoded choice animation
        choiceParentCanvasGroup.DOFade(1, .5f).SetEase(Ease.Linear);
        await UniTask.WaitUntil(() => choiceParent.GetComponentInChildren<CanvasGroup>().alpha == 1);
        choiceParentCanvasGroup.interactable = true;
        HideCTC();
        Debug.Log("done displaying choices, waiting for player selection");
        await UniTask.WaitUntil(() => waitingForPlayerToSelectChoice == false, cancellationToken: ct);
    }
    public async UniTaskVoid SelectChoice()
    {

        choiceParentCanvasGroup.interactable = false;
        var children = new List<GameObject>();
        for (int i = 0; i < choiceParent.transform.childCount; i++)
        {
            children.Add(choiceParent.transform.GetChild(i).gameObject);
        }
        // TECHDEBT: hardcoded choice animation
        choiceParent.GetComponentInChildren<CanvasGroup>().DOFade(0, .5f).SetEase(Ease.Linear);

        await UniTask.WaitUntil(() => choiceParent.GetComponentInChildren<CanvasGroup>().alpha == 0);
        Debug.Log("how many chocies to kill: " + children.Count.ToString());
        foreach (var child in children)
        {
            AssetRefLoader.Instance.ReleaseAsset(child);
            // Destroy(child);
        }
        DialogueSystem.Instance.ClearCurrentChoices();
        waitingForPlayerToSelectChoice = false;
    }




}

public struct Dialogue
{
    public string expression;
    public string speaker;
    public string text;
    public NPC_SO npc;
    public float duration;

    public Dialogue(string expression, string speaker, string text, NPC_SO npc, float duration)
    {
        this.expression = expression;
        this.speaker = speaker;
        this.text = text;
        this.duration = duration;
        this.npc = npc;
    }
}