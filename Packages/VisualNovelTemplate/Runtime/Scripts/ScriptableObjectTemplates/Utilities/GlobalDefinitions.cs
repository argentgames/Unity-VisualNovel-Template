using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GlobalDefinitions : SerializedScriptableObject
{
    public float sceneFadeInDuration = 1.5f;
    public float sceneFadeOutDuration = 1.5f;
    public float delayBeforeShowText = .4f;
    public float delayBeforHideClickUIFX = .4f;
    public float expressionChangeDuration = .35f;
    public float spawnCharacterDuration = .3f;
    public float hideCharacterDuration = .3f;
    public float delayBeforeAutoNextLine = .5f;

    public float defaultBGCameraSize = 16f;
    public float bgNoiseOpacity = .4f;
    public Vector3 defaultBGCameraPosition = new Vector3(0,0,-21.3f);

    public Dictionary<ScreenPosition,Vector2> characterSpawnLocationHelpers = new Dictionary<ScreenPosition, Vector2>();

    // *** GUI WIPES *** //
    public Texture2D defaultTextboxWipeIn;
    public Texture2D defaultTextboxWipeOut;
    // unused, but making for FUTURE
    public Texture2D defaultShotWipe;

    public Texture2D defaultNullTexture;

    void OnEnable()
    {
        
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