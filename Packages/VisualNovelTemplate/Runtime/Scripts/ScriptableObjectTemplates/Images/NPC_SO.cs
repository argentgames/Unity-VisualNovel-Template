using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;

using UnityEditor;

using UnityEngine.AddressableAssets;
namespace com.argentgames.visualnoveltemplate
{
    public enum ImageLayer
    {
        NewBackground,
        CurrentBackground,
        Midground,
        Foreground,
        Overlay
    }

    public abstract class NPC_SO : SerializedScriptableObject
    {
        [SerializeField]
        [PropertyTooltip("the name of the npc used in scripts for ease of writing, e.g. lowercased 'markus' instead of 'MARKUS'.")]
        public string internalName;
        [PropertyTooltip("The character name shown on screen to the player. May be changed during runtime gameplay.")]
        public string DisplayName
        {
            get {
                if (inkVariableName != "" && inkVariableName != null)
                {
                    try
                    {
                        return (string)DialogueSystemManager.Instance.Story.variablesState[inkVariableName];
                    }
                    catch
                    {
                        Debug.LogErrorFormat("Unable to locate {0} as the name of {1}",inkVariableName,this.internalName);
                        return defaultDisplayName;
                    }
                }
                else
                {
                    return defaultDisplayName;
                }
            }
        }
        [SerializeField]
        [Tooltip("The display name to reset to at the launch of game in case DisplayName is changed during runtime.")]
        private string defaultDisplayName;
        [SerializeField]
        [Tooltip("If we update the DisplayName through an ink variable, then return that saved value.")]
        private string inkVariableName;
        public string InkVariableName => inkVariableName;
        [PropertyTooltip("The color of the character's displayed name in-game.")]
        public Color NameColor = new Color(1,1,1,1);
        [PropertyTooltip("The color of a character's spoken lines displayed in-game.")]
        public Color TextColor = new Color(1,1,1,1);


        [PropertySpace(SpaceBefore = 20)]
        [InfoBox("Set this to true to reveal fields for adding in image data to an NPC")]
        public bool HasSpriteImages = false;
        [InfoBox("Use addressable system to instantiate our characters or just spawn game objects with Instantiate?")]
        public bool UseAddressables = false;

        [SerializeField]
        [InfoBox("Sprite image. You can only have one sprite type for an npc!")]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [PropertyTooltip("Large midground sprite shown behind the textbox")]
        [HideIf("@this.UseAddressables")]
        [DisableIf("@this.portraitSpritePrefab != null || sidePanelSpritePrefab != null")]
        GameObject mainSpritePrefab = null;
        [SerializeField]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [HideIf("@this.UseAddressables")]
        [DisableIf("@this.mainSpritePrefab != null || portraitSpritePrefab != null")]
        [PropertyTooltip("Side panel sprites are shown above the textbox but do not have a fixed location. You can move them around such as being to the right of the textbox.")]

        GameObject sidePanelSpritePrefab = null;
        [SerializeField]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [HideIf("@this.UseAddressables")]
        [PropertyTooltip("Portrait sprite is shown in a single fixed location and above the textbox.")]
        [DisableIf("@this.mainSpritePrefab != null || sidePanelSpritePrefab != null")]
        GameObject portraitSpritePrefab = null;
        [SerializeField]
        [InfoBox("Sprite image. You can only have one sprite type for an npc!")]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [DisableIf("@this.sidePanelSprite.RuntimeKeyIsValid() || this.portraitSprite.RuntimeKeyIsValid()")]
        [PropertyTooltip("Large midground sprite shown behind the textbox")]
        [HideIf("@this.UseAddressables == false")]
        AssetReference mainSprite = null;
        [SerializeField]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [HideIf("@this.UseAddressables == false")]
        [DisableIf("@this.mainSprite.RuntimeKeyIsValid() || this.portraitSprite.RuntimeKeyIsValid()")]
        [PropertyTooltip("Side panel sprites are shown above the textbox but do not have a fixed location. You can move them around such as being to the right of the textbox.")]
        AssetReference sidePanelSprite = null;
        [SerializeField]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [HideIf("@this.UseAddressables == false")]
        [DisableIf("@this.sidePanelSprite.RuntimeKeyIsValid() || this.mainSprite.RuntimeKeyIsValid()")]
        [PropertyTooltip("Portrait sprite is shown in a single fixed location and above the textbox.")]
        AssetReference portraitSprite = null;



        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [PropertyTooltip("Pose/expression of sprite spawned without any parameters, e.g. head_neutral, eyes_neutral")]
        public string defaultExpression = "";

        

        public AssetReference charGameObjectAssetRef
        {
            get
            {
                if (portraitSprite != null)
                {
                    return portraitSprite;
                }
                else if (mainSprite != null)
                {
                    return mainSprite;
                }
                else
                {
                    return sidePanelSprite;
                }
            }
        }
        public GameObject charGameObject
        {
            get
            {
                if (portraitSpritePrefab != null)
                {
                    return portraitSpritePrefab;
                }
                else if (sidePanelSpritePrefab != null)
                {
                    return sidePanelSpritePrefab;
                }
                else
                {
                    return mainSpritePrefab;
                }
            }
        }
        public ImageLayer spawnLayer
        {
            get
            {
                if (portraitSprite.RuntimeKeyIsValid() || portraitSpritePrefab != null)
                {
                    Debug.Log("portraitSprite !_ null");
                    return ImageLayer.Foreground;
                }
                else if (sidePanelSprite.RuntimeKeyIsValid() || sidePanelSpritePrefab != null)
                {
                    Debug.Log("sidepanelsprite !_ null");
                    return ImageLayer.Foreground;
                }
                else
                {
                    Debug.Log("mainSprite !_ null");
                    return ImageLayer.Midground;
                }
            }
        }
        void OnEnable()
        {
            
            

        }

       

    }

    public struct ColorTint
    {
        public Color color;
        public string internalName;
    }

    public struct SpriteExpression
    {
        public string prefix;
        public List<SpriteExpressionData> expressionDatas;
        
        /// <summary>
        /// if the sprite changes, then also change every dependent part using the same name
        /// </summary>
        public List<string> dependentParts;

    }
    public struct SpriteExpressionData
    {
        public string internalName;
        public Sprite sprite;
    }
}