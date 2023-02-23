using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using AnimeTask;
using ElRaccoone.Tweens;
namespace com.argentgames.visualnoveltemplate
{
    public struct BodyPart
    {
        public string prefix;
        public GameObject gameObject;
    }

    public class SpriteWrapperController : SerializedMonoBehaviour
    {
        [SerializeField]
        [Tooltip("Automatically self register to Image Manager's current characters on screen dict with this name.")]
        string selfRegisteredName;
        [SerializeField]
        [Tooltip("Body part object that gets modified for a specific expression body part change.")]
        List<BodyPart> bodyParts = new List<BodyPart>();
        [SerializeField]
        [ReadOnly]
        Dictionary<string, SpriteRenderer> bodyPartsMap = new Dictionary<string, SpriteRenderer>();

        [SerializeField]
        [ReadOnly]
        [BoxGroup("Sprite data")]
        // TECHDEBT: not sure what this is for. Probably auto setting an exp when there's a pose change??
        private Dictionary<string, SpriteExpression> expressionsMapForHead = new Dictionary<string, SpriteExpression>();

        [BoxGroup("Sprite data")]
        public List<SpriteExpression> expressions = new List<SpriteExpression>();

        [BoxGroup("Sprite data")]
        public Vector3 defaultSpawnPosition;

        [BoxGroup("Sprite data")]
        public Dictionary<ScreenPosition, Vector2> positions = new Dictionary<ScreenPosition, Vector2>();

        [BoxGroup("Sprite data")]
        [PropertyTooltip("Color tints applied to sprite image, often used for nighttime or outdoor scenes to make the sprite blend in more with the environment.")]
        public List<ColorTint> colorTints = new List<ColorTint>();
        private Dictionary<string, ColorTint> colorTintsMap = new Dictionary<string, ColorTint>();

        public char prefixDelimiter = '_';
        [SerializeField]
        bool doSelfRegister = true;

        SkipTokenSource skipTokenSource = new SkipTokenSource();
        SkipToken skipToken;

        /// <summary>
        /// Holds the current expression for each bodyparth that changes so that we can save/load it
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        Dictionary<string,string> currentExpression = new Dictionary<string, string>();

        public void CreateSkipToken()
        {
            this.skipTokenSource = new SkipTokenSource();
            skipToken = skipTokenSource.Token;
        }
        public void ThrowSkipToken()
        {
            skipTokenSource.Skip();
            CreateSkipToken();
        }

        bool animationComplete = false;

        bool initComplete = false;
        public bool InitComplete => initComplete;
        async UniTaskVoid Awake()
        {
            bodyPartsMap.Clear();
            currentExpression.Clear();
            // Debug.Break();
        
            if (doSelfRegister)
            {
                await UniTask.WaitUntil(() => ImageManager.Instance != null);
               ImageManager.Instance.RegisterCharacter(selfRegisteredName, this.gameObject); 
            }
            // set our initial/default expression

            foreach (var part in bodyParts)
            {
                bodyPartsMap[part.prefix] = part.gameObject.GetComponentInChildren<SpriteRenderer>();
                // init; set our currentExpression dict to hold all the relevant bodyparts
                currentExpression[part.prefix] = "";
            }
            colorTintsMap.Clear();
            foreach (var tint in colorTints)
            {
                colorTintsMap[tint.internalName] = tint;
            }


            GenerateExpressionsMapForHead();
            
            CreateSkipToken();
            
            initComplete = true;
            
        }

        void Start()
        {
            // SetNewExpression();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression">prefix_expression</param>
        /// <returns></returns>
        public Sprite GetExpressionImage(string expression)
        {
            // get prefix from expression.
            var splitExpression = expression.Split(prefixDelimiter);
            var prefix = splitExpression[0];
            var segment = new ArraySegment<string>(splitExpression, 1, splitExpression.Length - 1);
            var noPrefixExpression = string.Join("_", segment);
            Debug.LogFormat("Looking for expression image: prefix-{0} noPrefix-{1}", prefix, noPrefixExpression);
            var part = expressionsMapForHead[prefix];
            foreach (var exp in part.expressionDatas)
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
            expressionsMapForHead = new Dictionary<string, SpriteExpression>();
            expressionsMapForHead.Clear();
            foreach (var exp in expressions)
            {
                expressionsMapForHead[exp.prefix] = exp;
            }
        }

        /// <summary>
        /// Sets the new expression that we want to update the character to.
        /// This method DOES NOT MAKE VISIBLE the new expression. It only sets it
        /// in newExp shader parameter! You need to run RunExpressionTransition to
        /// update the visibility of this new expression.
        /// </summary>
        /// <param name="expression">list of expressions in a string format with space delimiting. E.g. 
        /// head_1 eyes_angry_1 mouth_sad2</param>
        [Button]
        void SetNewExpression(string expression = "")
        {
            if (expression == "")
            {
                return;
            }
            var parts = expression.Split(' ');
            Sprite newExpSprite;
            SpriteRenderer bodyPartSpriteRenderer;
            string _depPart = "";
            foreach (var part in parts)
            {

                var _prefix = part.Split(prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                try
                {
                    var spriteExpression = expressionsMapForHead[_prefix];
                    if (spriteExpression.dependentParts != null)
                    {
                        foreach (var depPart in spriteExpression.dependentParts)
                        {
                            try
                            {
                                var splitExpression = part.Split(prefixDelimiter);
                                _depPart = string.Format("{0}{1}{2}", depPart, prefixDelimiter, StringExtensions.ArrayToString(splitExpression, 1, splitExpression.Length));
                                Debug.LogFormat("depPart we are looking for: {0}", _depPart);
                                bodyPartSpriteRenderer = bodyPartsMap[depPart];
                                newExpSprite = GetExpressionImage(_depPart);
                                bodyPartSpriteRenderer.material.SetTexture("NewTex", newExpSprite.texture);
                            }
                            catch
                            {
                                Debug.LogErrorFormat("failed to find depPart. Part-{0} depPartPrefix-{1}", _depPart, depPart);
                            }

                        }
                    }
                    // do we need to set dependent parts too?
                    bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    newExpSprite = GetExpressionImage(part);
                    bodyPartSpriteRenderer.material.SetTexture("NewTex", newExpSprite.texture);

                    currentExpression[_prefix] = part;
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("failed to find part. Part-{0} _prefix-{1}, exception {2}", 
                        part, _prefix,e);
                }

            }
        }
        /// <summary>
        /// Updates the visible expression. In theory we might support other transition types,
        /// but for now we only do image interpolation...............
        /// </summary>
        /// <param name="transition">transition type. Unused for as we only support interpolation!</param>
        /// <returns></returns>
        async UniTask RunExpressionTransition(float transitionDuration = -1f, string transition = "")
        {
            // if no transition duration is given, use the default expression change duration
            if (transitionDuration == -1f)
            {
                transitionDuration = GameManager.Instance.DefaultConfig.expressionChangeDuration;
            }

            if (GameManager.Instance.IsSkipping)
            {
                transitionDuration = 0;
            }

            List<UniTask> animationTasks = new List<UniTask>();
            animationComplete = false;
            foreach (var sr in bodyPartsMap.Values)
            {
                if (sr.material.GetTexture("NewTex") != null)
                {
                    if (sr.material.GetTexture("NewTex").name != sr.sprite.texture.name)
                    {
                        Debug.Log("running animation for SR: " + sr.gameObject.name +
                         " with oldTex "+ sr.sprite.texture.name + " and newTex " + sr.material.GetTexture("NewTex").name);
                        
                        
                        sr.TweenValueFloat (1f, transitionDuration, (v) => {
                            sr.material.SetFloat("_TransitionAmount",v);
                            Debug.Log("setting material value");
                        }).SetFrom(0) ;
                        
                        // TECHDEBT: hardcoding the ease =.=
//                         animationTasks.Add(
// Easing.Create<Linear>(start: 0f, end: 1f, duration: transitionDuration)
//                     .ToMaterialPropertyFloat(sr, "_TransitionAmount", skipToken: skipToken)
//                         );

                        // sequence.Join(sr.material.DOFloat(1, "_TransitionAmount", transitionDuration)
                        // .SetEase(GameManager.Instance.DefaultConfig.expressionTransitionEase)
                        // .From(0));
                    }
                    else
                    {
                        Debug.LogFormat("newTex {0} is same as mainTex {1}", sr.material.GetTexture("NewTex").name, sr.sprite.name);
                    }
                }
            }


            await UniTask.WhenAll(animationTasks);




        }
        void ResetMainExpression(string expression)
        {

            if (expression == "")
            {
                return;
            }

            var parts = expression.Split(' ');
            Sprite newExpSprite;
            SpriteRenderer bodyPartSpriteRenderer;
            string _depPart = "";
            foreach (var part in parts)
            {
                var _prefix = part.Split(prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                try
                {
                    var spriteExpression = expressionsMapForHead[_prefix];
                    if (spriteExpression.dependentParts != null)
                    {
                        foreach (var depPart in spriteExpression.dependentParts)
                        {
                            try
                            {
                                var splitExpression = part.Split(prefixDelimiter);
                                _depPart = string.Format("{0}{1}{2}", depPart, prefixDelimiter, StringExtensions.ArrayToString(splitExpression, 1, splitExpression.Length));
                                Debug.LogFormat("depPart we are looking for: {0}", _depPart);
                                bodyPartSpriteRenderer = bodyPartsMap[depPart];
                                newExpSprite = GetExpressionImage(_depPart);
                                bodyPartSpriteRenderer.sprite = newExpSprite;
                                bodyPartSpriteRenderer.material.SetFloat("_TransitionAmount", 0);
                            }
                            catch
                            {
                                Debug.LogErrorFormat("failed to find depPart. Part-{0} depPartPrefix-{1}", _depPart, depPart);
                            }

                        }
                    }

                    bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    newExpSprite = GetExpressionImage(part);

                    bodyPartSpriteRenderer.sprite = newExpSprite;
                    bodyPartSpriteRenderer.material.SetFloat("_TransitionAmount", 0);
                }
                catch
                {
                    Debug.LogErrorFormat("Unable to locate parts. Prefix-{0}, part-{1}", _prefix, part);
                }

            }
        }
        public async UniTask ExpressionChange(string expression = "", float transitionDuration = -1f, string transition = "")
        {
            SetNewExpression(expression);
            await RunExpressionTransition(transitionDuration, transition);
            ResetMainExpression(expression);
        }
        public void ApplyTint(string tintName)
        {
            var tintColor = GetTintColor(tintName);
            foreach (var sr in bodyPartsMap.Values)
            {
                sr.material.SetColor("_Tint", tintColor);
            }
        }

        public SpriteSaveData Save()
        {
            var s = new SpriteSaveData();

            // idk if there's any reason to not save a dict but for now lets just use the save string...
            s.expressionImageName = "";
            foreach (var kv in currentExpression)
            {
                s.expressionImageName += string.Format("{0} ",kv.Value);
            }

            
            s.position = transform.localPosition;
            
            // TODO: add in tint color

            // Debug.Log("expression: " + CurrentExpression)
            return s;
        }
        public void Load(SpriteSaveData saveData)
        {
            transform.position = saveData.position;
            SetNewExpression(saveData.expressionImageName);
        }
        
        void OnDestroy()
        {
            if (doSelfRegister)
            {
            ImageManager.Instance.UnregisterCharacter(selfRegisteredName);
 
            }
        }
    }
}