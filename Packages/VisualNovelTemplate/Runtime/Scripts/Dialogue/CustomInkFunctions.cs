using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Ink.Runtime;
using Cysharp.Threading.Tasks;

namespace com.argentgames.visualnoveltemplate
{


public class CustomInkFunctions : MonoBehaviour
{
    Story _inkStory;
    AudioManager audioManager;
    ImageManager imageManager;
    public bool registeredFunctions = false;
    async UniTaskVoid Awake()
    {
        await UniTask.WaitWhile(() => DialogueSystemManager.Instance == null);
        _inkStory = GetComponent<DialogueSystemManager>().Story;
        await UniTask.WaitWhile(() => AudioManager.Instance == null);
        audioManager = AudioManager.Instance;
        await UniTask.WaitWhile(() => ImageManager.Instance == null);
        imageManager = ImageManager.Instance;
        RegisterFunctions();
        registeredFunctions = true;
    }

    void RegisterFunctions()
    {
        Debug.Log("registering functions");
        _inkStory.BindExternalFunction("sfx", (string name) =>
        {
            audioManager.PlaySFX(name);
        });
        _inkStory.BindExternalFunction("music", (string name) =>
        {
            audioManager.PlayMusic(name);
        });
        _inkStory.BindExternalFunction("amb", (string name, int channel) =>
        {
            audioManager.PlayAmbient(name, channel);
        });
        _inkStory.BindExternalFunction("shot", (string bgName, string transition, float duration) =>
        {
            imageManager.ShowBG(bgName, transition, duration);
        });
        _inkStory.BindExternalFunction("moveCam", (string moveType, string newValues, float duration) =>
        {
            imageManager.MoveCam(moveType, StringExtensions.ParseVector2(newValues), duration);
        });
        _inkStory.BindExternalFunction("setCam", (string position, string rotation, string size) =>
        {
            Vector3? p, r;
            float? s;
            if (position == "")
            {
                p = null;
            }
            else
            {
                p = StringExtensions.ParseVector3(position);
            }
            if (rotation == "")
            {
                r = null;
            }
            else

            {
                r = StringExtensions.ParseVector3(rotation);
            }
            if (size == "")
            {
                s = null;
            }
            else
            {
                s = StringExtensions.ParseFloat(size);
            }
            imageManager.SetBGCameraShot(p, r, s);
        });
        _inkStory.BindExternalFunction("hideBG", (string bgName, string transition, float duration) =>
        {
            imageManager.HideBG(bgName, transition, duration);
        });
        _inkStory.BindExternalFunction("spawnChar", async (string charName, string expression, string position) =>
        {
            Vector3? pos = null;
            if (position == "null")
            {
                pos = null;
            }
            else
            {
                pos = StringExtensions.ParseVector3(position);
            }
            await imageManager.SpawnChar(charName, expression, pos);
        });
        _inkStory.BindExternalFunction("showChar", async (string charName, Vector3? location, string transition, float duration) =>
        {
            await imageManager.ShowChar(charName, location, transition, duration);
        });

        _inkStory.BindExternalFunction("hideChar", (string charName, string transition, float duration) =>
        {
            imageManager.HideChar(charName, transition, duration);
        });

        _inkStory.BindExternalFunction("playBG", () =>
        {
            imageManager.PlayBGTween();
        });
        _inkStory.BindExternalFunction("shakeCam", (float duration) =>
        {
            imageManager.ShakeCam(duration);
        });
        _inkStory.BindExternalFunction("loadScene", (string scene, string fadeOutDur, string fadeInDur) =>
        {
            float? fadeOut,fadeIn;
            if (fadeOutDur == "")
            {
                fadeOut = null;
            }
            else
            {
                fadeOut = StringExtensions.ParseFloat(fadeOutDur);
            }
            if (fadeInDur == "")
            {
                fadeIn = null;
            }
            else
            {
                fadeIn = StringExtensions.ParseFloat(fadeInDur);
            }
            SceneTransitionManager.Instance.LoadScene(scene,fadeOut,fadeIn);
        });
        
    }

}
}