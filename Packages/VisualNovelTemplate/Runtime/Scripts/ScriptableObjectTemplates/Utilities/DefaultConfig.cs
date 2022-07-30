using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Default config values such as transition rate between backgrounds, 
/// transition texture, text unwrap speed, etc.
/// This is an abstract class because every game will have different config needs!
/// We have added some common config variables that we use in our framework.
/// Inherit from the class and add your own additional game-specific config variables!
/// NOTE: This config file is DIFFERENT from persistent variables that you might need
/// for in-game things, such as checking whether the player has completed the game once. 
/// If you are looking for persistent variables, use PersistentGameData_SO.
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public abstract class DefaultConfig : SerializedScriptableObject
    {
        [PropertyTooltip("Default dialogue window to use if not specified to activate another window mode. Typicall either ADV or NVL.")]
        public DialogueWindowMode_SO defaultDialogueWindow;
        public float sceneFadeInDuration = 1.5f;
        public float sceneFadeOutDuration = 1.5f;
        public float delayBeforeShowText = .4f;
        public float delayBeforHideClickUIFX = .4f;
        public float expressionChangeDuration = .35f;
        public DG.Tweening.Ease expressionTransitionEase = DG.Tweening.Ease.InCubic;
        public float spawnCharacterDuration = .3f;
        public float hideCharacterDuration = .3f;
        public float delayBeforeAutoNextLine = .5f;
        public float loadSceneStopSoundDuration = 2.5f;
        [InfoBox("For now we only support changing text color for choices added to history. TODO: support style sheet!")]
        [PropertyTooltip("If we are adding selected choices to history, what color should the text be?")]
        public string historyChoiceColor = "#cfda5e";
        [InfoBox("TODO: NOT SUPPORTED RIGHT NOW. Some games like to show the current speaking character on screen as a Midground sprite + in the bottom corner as a portrait sprite. We don't use this design paradigm. However, we might consider supporting it with this flag later...")]
        public bool showOnscreenCharacterAsPortrait = false;

        [PropertySpace(SpaceBefore =15f)]
        [InfoBox("Do we use addressable loading mechanism? If so, for which objects?")]
        [BoxGroup("Addressables")]
        public bool addressableSettings = false;
        public bool addressableSprites = false;
        public bool addressableBackgrounds = false;
        public bool addressableGUI = false;

        public float defaultBGCameraSize = 16f;
        [PropertyTooltip("How much Gaussian noise to apply to the background camera. Originally used in The Hepatica Spring.")]
        public float bgNoiseOpacity = .4f;
        public Vector3 defaultBGCameraPosition = new Vector3(0, 0, -21.3f);

        public Dictionary<ScreenPosition, Vector2> characterSpawnLocationHelpers = new Dictionary<ScreenPosition, Vector2>();

        // *** INGAME SCENE TRANSITION WIPES *** //
        public string defaultTextboxWipeIn;
        public string defaultTextboxWipeOut;
        // unused, but making for FUTURE
        public string defaultShotWipe;

        public Texture2D defaultNullTexture;

    }
}

public enum ScreenPosition
{
    RIGHT,
    CENTER,
    LEFT,
    LEFT_OF_TWO,
    RIGHT_OF_TWO,

}