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
        public string DisplayName;
        [PropertyTooltip("The color of the character's displayed name in-game.")]
        public Color NameColor = new Color(255, 255, 255, 255);
        [PropertyTooltip("The color of a character's spoken lines displayed in-game.")]
        public Color TextColor = new Color(255, 255, 255, 255);


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

        [SerializeField]
        [ReadOnly]
        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        // TECHDEBT: not sure what this is for. Probably auto setting an exp when there's a pose change??
        private Dictionary<string, List<SpriteExpressionData>> expressionsMapForHead = new Dictionary<string, List<SpriteExpressionData>>();

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public List<SpriteExpression> expressions = new List<SpriteExpression>();

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public virtual void SetExpressionTextures(GameObject go, string exp, string textureName) { }

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public virtual void SetMainTextureExpression(GameObject go, string exp) { }

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public Vector3 defaultSpawnPosition;

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public Dictionary<ScreenPosition, Vector2> positions = new Dictionary<ScreenPosition, Vector2>();

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [PropertyTooltip("Pose/expression of sprite spawned without any parameters, e.g. head_neutral, eyes_neutral")]
        public string defaultExpression = "";

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        public virtual string CurrentExpression { get; set; }

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        private GameObject bodyPartsHolder;

        [ShowIf("HasSpriteImages")]
        [BoxGroup("Sprite data")]
        [PropertyTooltip("Color tints applied to sprite image, often used for nighttime or outdoor scenes to make the sprite blend in more with the environment.")]
        public List<ColorTint> colorTints = new List<ColorTint>();
        private Dictionary<string, ColorTint> colorTintsMap = new Dictionary<string, ColorTint>();

        string tag = "sprite expression holder";
        public char prefixDelimiter = '_';

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
            colorTintsMap.Clear();
            foreach (var tint in colorTints)
            {
                colorTintsMap[tint.internalName] = tint;
            }
            

        }
        public Sprite GetExpressionImage(string expression)
        {
            // get prefix from expression.
            var splitExpression = expression.Split(prefixDelimiter);
            var prefix = splitExpression[0];
            var segment = new ArraySegment<string>(splitExpression, 1, splitExpression.Length -1);
            var noPrefixExpression = string.Join("_",segment);
            Debug.LogFormat("Looking for expression image: prefix-{0} noPrefix-{1}",prefix,noPrefixExpression);
            var part = expressionsMapForHead[prefix];
            foreach (var exp in part)
            {
                if (exp.internalName == noPrefixExpression)
                {
                    return exp.sprite;
                }

            }
            Debug.LogErrorFormat("Unable to locate expression {0}", expression);
            return null;
        }
        public Color GetTintColor(string tintName)
        {
            try
            {
                return colorTintsMap[tintName].color;
            }
            catch
            {
                Debug.LogErrorFormat("Tint color [{0}] does not exist", tintName);
                return Color.white;
            }
        }
        public void GenerateExpressionsMapForHead()
        {
            expressionsMapForHead.Clear();
            foreach (var exp in expressions)
            {
                expressionsMapForHead[exp.prefix] = exp.expressionDatas;
            }
        }

        Transform FindWithTag(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag(this.tag)) return t;
            }
            return null;
        }

        // GO structure: Exp > Arm > Sweat (accessories)> Glasses 
        // public virtual void SetExpression(GameObject go, string exp)
        // {
        //     bodyPartsHolder = FindWithTag(go.transform).gameObject;
        //     SetExpressionTextures(bodyPartsHolder, exp, "NewTex");

        // }


        public virtual void SetNewOldExpression(GameObject go, string exp)
        {
            bodyPartsHolder = FindWithTag(go.transform).gameObject;
            SetMainTextureExpression(bodyPartsHolder, exp);
        }

        /// <summary>
        /// The GO has just been spawned but is disabled. Set the initial expression.
        /// </summary>
        /// <param name="go">Character GO which has structure CharName > BodyPartsHolder > list of body parts</param>
        /// <param name="exp">Expression list</param>
        // public virtual void SetInitialExpression(GameObject go, string exp)
        // {

        //     // string defaultExp =  string.Format("{0} {1} {2} {3}", defaultArm, defaultExpression, defaultGlasses, defaultSweat);

        //     // since we go through exps and update them in order, lets set all the defaults
        //     // and then override the necessary ones at the end
        //     if (exp != null)
        //     {
        //         exp = defaultExpression + " " + exp;
        //     }

        //     bodyPartsHolder = FindWithTag(go.transform).gameObject;

        //     // set the base layer expression
        //     SetMainTextureExpression(bodyPartsHolder, exp);
        //     // set the new layer expression. now we have a single current expression "current" == "new" xpression
        //     SetExpressionTextures(bodyPartsHolder, exp, "NewTex");

        //     // reset transition amount so that we are showing the mainTex sprite
        //     foreach (var sr in bodyPartsHolder.GetComponentsInChildren<SpriteRenderer>())
        //     {

        //         sr.material.SetFloat("_TransitionAmount", 0);
        //     }

        // }

        // public virtual async UniTask UpdateExpression(GameObject go, float duration = 0f)
        // {
        //     var animationComplete = false;
        //     var spriteWrapper = FindWithTag(go.transform);

        //     var spriteRenderers = spriteWrapper.GetComponentsInChildren<SpriteRenderer>();

        //     sequence = DOTween.Sequence();
        //     Debug.Log(spriteRenderers[0].sprite.name);
        //     foreach (var sr in spriteRenderers)
        //     {
        //         if (sr.gameObject.tag == "no_transition")
        //         {
        //             continue;
        //         }

        //         if (sr.material.GetTexture("NewTex") != null)
        //         {
        //             if (sr.material.GetTexture("NewTex").name != sr.sprite.texture.name)
        //             {
        //                 sequence.Join(sr.material.DOFloat(1, "_TransitionAmount", duration).SetEase(Ease.InOutCubic).From(0));
        //             }
        //             else
        //             {
        //                 Debug.LogFormat("newTex {0} is same as mainTex {1}", sr.material.GetTexture("NewTex").name, sr.sprite.name);
        //             }
        //         }


        //     }

        //     sequence.Play().OnComplete(() =>
        //     {
        //         animationComplete = true;
        //     }
        //     ).OnStart(() => Debug.Log("starting sequ")).OnUpdate(() =>
        //     {
        //         if (spriteRenderers[0].material.GetFloat("_TransitionAmount") >= .5)
        //         {

        //         }
        //     }

        //         );//.OnUpdate(() => ImageManager.Instance.OnscreenSpriteCamera.Render());
        //     if (GameManager.Instance.IsSkipping)
        //     {
        //         sequence.Complete();
        //     }

        //     await UniTask.WaitUntil(() => animationComplete);
        //     await UniTask.Yield();

        //     foreach (var sr in spriteRenderers)
        //     {
        //         if (sr.gameObject.tag == "no_transition")
        //         {
        //             continue;
        //         }
        //         if (sr != null)
        //         {
        //             // sr.material.SetFloat("_TransitionAmount", 0);

        //         }
        //     }

        // }

        // public void ResetTransitionAmount(GameObject go)
        // {
        //     var spriteRenderers = go.transform.GetComponentsInChildren<SpriteRenderer>();
        //     foreach (var sr in spriteRenderers)
        //     {
        //         if (sr.gameObject.tag == "no_transition")
        //         {
        //             continue;
        //         }
        //         if (sr != null)
        //         {
        //             sr.material.SetFloat("_TransitionAmount", 0);

        //         }
        //     }
        // }

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

    }
    public struct SpriteExpressionData
    {
        public string internalName;
        public Sprite sprite;
    }
}