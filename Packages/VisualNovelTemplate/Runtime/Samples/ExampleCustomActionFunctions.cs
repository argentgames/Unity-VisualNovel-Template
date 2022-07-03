using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace com.argentgames.visualnoveltemplate
{
    public class ExampleCustomActionFunctions : CustomActionFunctions
    {

        [SerializeField]
        DialogueUIManager dialogueUIManager;
        public override async UniTask ActionFunction(string text, CancellationToken ct)
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
                case "playVideo":
                    await VideoManager.Instance.PlayVideo(p[1]);
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
                        await UniTask.Delay(TimeSpan.FromSeconds(StringExtensions.ParseFloat(p[1])), cancellationToken: ct);
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: ct);
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
                        await UniTask.Delay(TimeSpan.FromSeconds(timeToDelay), cancellationToken: ct);
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: ct);
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
                        await UniTask.Delay(TimeSpan.FromSeconds(timeToDelay), cancellationToken: ct);
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: ct);
                    }
                    break;
                case "shot":
                    // TECHDEBT: hardcoded to at least clear textbox and namecard before a shot
                    DialogueSystemManager.Instance.DialogueUIManager.ClearUI();
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
                        duration = GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine;
                    }
                    else
                    {
                        duration = StringExtensions.ParseFloat(p[1]);
                    }
                    if (!GameManager.Instance.IsSkipping)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: ct);
                    }
                    break;
                case "clear_p":
                    dialogueUIManager.ClearUI();
                    if (pLength < 2)
                    {
                        duration = GameManager.Instance.DefaultConfig.delayBeforeAutoNextLine;
                    }
                    else
                    {
                        duration = StringExtensions.ParseFloat(p[1]);
                    }

                    if (!GameManager.Instance.IsSkipping)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ct);
                    }
                    else
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: ct);
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
                        GameManager.Instance.PersistentGameData.SetUnlockableState(p[1].TrimStart(null).TrimEnd(null),true);
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
                    DialogueSystemManager.Instance.SetEndGame(true);
                    break;
                case "disablePlayerInput":
                    dialogueUIManager.DisableCTC();
                    DialogueSystemManager.Instance.SetPlayerCanContinue(false);
                    break;
                case "enablePlayerInput":
                    dialogueUIManager.EnableCTC();
                    DialogueSystemManager.Instance.SetPlayerCanContinue(true);
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
                    MenuManager.Instance.DisableSettingsUIControls();
                    break;
                default:
                    Debug.LogErrorFormat("action function {0} doesn't exist", funcName);
                    break;
            }
        }
    }
}
