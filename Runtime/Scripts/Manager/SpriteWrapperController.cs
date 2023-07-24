using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using AnimeTask;
using ElRaccoone.Tweens;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Threading;

namespace com.argentgames.visualnoveltemplate
{
    public struct BodyPart
    {
        public string prefix;
        public GameObject gameObject;
    }

    public class SpriteWrapperController : SerializedMonoBehaviour
    {
        MaterialPropertyBlockUtilities materialPropertyBlockUtilities;

        [SerializeField]
        [Tooltip(
            "Automatically self register to Image Manager's current characters on screen dict with this name."
        )]
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
        private Dictionary<string, SpriteExpression> expressionsMapForHead =
            new Dictionary<string, SpriteExpression>();

        [BoxGroup("Sprite data")]
        public List<SpriteExpression> expressions = new List<SpriteExpression>();

        [BoxGroup("Sprite data")]
        public Vector3 defaultSpawnPosition;

        [BoxGroup("Sprite data")]
        public Dictionary<ScreenPosition, Vector2> positions =
            new Dictionary<ScreenPosition, Vector2>();

        [BoxGroup("Sprite data")]
        [PropertyTooltip(
            "Color tints applied to sprite image, often used for nighttime or outdoor scenes to make the sprite blend in more with the environment."
        )]
        public List<ColorTint> colorTints = new List<ColorTint>();
        private Dictionary<string, ColorTint> colorTintsMap = new Dictionary<string, ColorTint>();

        public char prefixDelimiter = '_';

        [SerializeField]
        bool doSelfRegister = true;

        SkipTokenSource skipTokenSource = new SkipTokenSource();
        SkipToken skipToken;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;

        /// <summary>
        /// Holds the current expression for each bodyparth that changes so that we can save/load it
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        Dictionary<string, string> currentExpression = new Dictionary<string, string>();

        [SerializeField]
        List<bool> animationsRunning = new List<bool>();

        public List<string> doNotRunSWC = new List<string>();

        public void CreateSkipToken()
        {
            this.skipTokenSource = new SkipTokenSource();
            skipToken = skipTokenSource.Token;
        }

        public void ThrowSkipToken()
        {
            skipTokenSource.Skip();
            CreateSkipToken();

            // TECHDEBT: adding in a force set the transition value directly skip <_< until we get rid of all these animation libraries.
            // SkipAnimation();
            ThrowCancellationToken();
        }

        void CreateCancellationToken()
        {
            this.cts = new CancellationTokenSource();
            this.ct = cts.Token;
        }

        void ThrowCancellationToken()
        {
            cts.Cancel();
            CreateCancellationToken();
        }

        void SkipAnimation()
        {
            var animateIDX = 0;
            foreach (var sr in bodyPartsMap.Values)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                try
                {
                    sr.GetPropertyBlock(propBlock, 0);
                    propBlock.SetFloat("_TransitionAmount", 1);
                    sr.SetPropertyBlock(propBlock, 0);
                    // sr.material.SetFloat("_TransitionAmount", 1);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat(
                        "Could not run expression transition for sr: {0} with exception {1}",
                        sr,
                        e
                    );
                }
                animationsRunning[animateIDX] = false;
                animateIDX += 1;
            }

            foreach (var animate in animates)
            {
                if (animate != null)
                {
                    StopCoroutine(animate);
                }
            }
            animates.Clear();
        }

        bool animationComplete = false;
        public bool AnimationComplete => animationComplete;

        bool initComplete = false;
        public bool InitComplete => initComplete;

        [SerializeField]
        string defaultExpression = "brow_neut eyes_neut mouth_neut";

        async UniTaskVoid Awake()
        {
            materialPropertyBlockUtilities = GetComponent<MaterialPropertyBlockUtilities>();
            // Debug.Break();

            if (doSelfRegister)
            {
                Debug.Log("self registering, waiting for imgmanager not null");
                await UniTask.WaitUntil(() => ImageManager.Instance != null);
                ImageManager.Instance.RegisterCharacter(selfRegisteredName, this.gameObject);
            }
            await UniTask.Yield();
            await UniTask.Yield();

            // set our initial/default expression
            bodyPartsMap.Clear();
            currentExpression.Clear();
            Debug.Log("mapping bodyparts");
            foreach (var part in bodyParts)
            {
                bodyPartsMap[part.prefix] = part.gameObject.GetComponentInChildren<SpriteRenderer>(
                    true
                );
                // init; set our currentExpression dict to hold all the relevant bodyparts
                currentExpression[part.prefix] = "";
            }
            GenerateExpressionsMapForHead();
            await UniTask.Yield();
            await UniTask.Yield();
            Debug.Log("setting new experssion");
            SetNewExpression(defaultExpression);
            await UniTask.Yield();
            Debug.Log("resetting main expression");
            ResetMainExpression(defaultExpression);
            Debug.Log("done initializing sprite with default expression");
            // Debug.Break();

            colorTintsMap.Clear();
            foreach (var tint in colorTints)
            {
                colorTintsMap[tint.internalName] = tint;
            }

            if (transitionInCurve.length == 0)
            {
                Debug.Log("setting trnasition curve manually??");
                transitionInCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            CreateSkipToken();

            for (int idx = 0; idx < bodyParts.Count; idx++)
            {
                animationsRunning.Add(false);
            }

            // set all alpha to 0 so the character isn't shown when spawned
            //    Test_SetAlphaDirectly(0);

            await UniTask.Yield();
            initComplete = true;
        }

        void Start()
        {
            // SetNewExpression();
        }

        public bool MainExpressionIsReset {
            get
            {
                foreach (var mbpu in GetComponentsInChildren<MaterialPropertyBlockUtilities>())
                {
                    // Debug.LogFormat("maintex {0} == newTex {1}, {2}",mbpu.GetTexture("_MainTex").name
                    // ,mbpu.GetTexture("NewTex").name,
                    // mbpu.GetTexture("_MainTex").name == mbpu.GetTexture("NewTex").name);
                    if (mbpu.GetTexture("_MainTex").name != mbpu.GetTexture("NewTex").name)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public void Test_SetAlphaDirectly(float val)
        {
            // Debug.LogFormat("setting alpha directly with val {0}", val);
            foreach (var mpbu in GetComponentsInChildren<MaterialPropertyBlockUtilities>())
            {
                // var mpbu = part.GetComponent<MaterialPropertyBlockUtilities>();
                mpbu.UpdateProperties(new Tuple<string, float>("Alpha", val));
            }
        }

        public void SetMatFloatDirectly(string matRefName, float val)
        {
            foreach (var mpbu in GetComponentsInChildren<MaterialPropertyBlockUtilities>())
            {
                mpbu.UpdateProperties(new Tuple<string, float>(matRefName, val));

                // MaterialPropertyBlock block = new MaterialPropertyBlock();
                // part.GetPropertyBlock(block, 0);
                // block.SetFloat(matRefName, val);
                // part.SetPropertyBlock(block, 0);
                // Debug.LogFormat("set {2} for sr {0} to val {1}", part.name, block.GetFloat(matRefName),matRefName);
            }
        }

        private void OnEnable() { }

        public async UniTask ShowChar(float duration = .35f)
        {
            if (duration != 0)
            {
                Debug.Log("make sure we start showChar from alpha=0");
                Test_SetAlphaDirectly(0);
            }

            gameObject.SetActive(true);
            // StartCoroutine(I_TransitionAlpha(duration));
            Debug.LogFormat("showing char with duration {0}", duration);
            await U_TransitionAlpha(duration, fadein: true);

            animationComplete = true;
            Debug.Log("done show char from swc" + AnimationComplete.ToString());
        }

        public async UniTask HideChar(float duration = .35f)
        {
            // StartCoroutine(I_TransitionAlpha(duration));
            await U_TransitionAlpha(duration, fadein: false);

            animationComplete = true;
            Debug.Log("done show char from swc" + AnimationComplete.ToString());
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
            // Debug.LogFormat(
            //     "Looking for expression image: prefix-{0} noPrefix-{1}",
            //     prefix,
            //     noPrefixExpression
            // );
            var part = expressionsMapForHead[prefix];
            foreach (var exp in part.expressionDatas)
            {
                if (exp.internalName == noPrefixExpression)
                {
                    return exp.sprite;
                }
            }
            Debug.LogWarningFormat("Unable to locate expression {0}", expression);
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
                Debug.LogWarningFormat("Tint color [{0}] does not exist", tintName);
                return Color.white;
            }
        }

        public void GenerateExpressionsMapForHead()
        {
            Debug.Log("generating expressions map for head");
            expressionsMapForHead = new Dictionary<string, SpriteExpression>();
            expressionsMapForHead.Clear();
            foreach (var exp in expressions)
            {
                expressionsMapForHead[exp.prefix] = exp;
            }
        }

        public void ShaderDebug()
        {
            MaterialPropertyBlock block;
            foreach (var part in bodyPartsMap.Values)
            {
                try
                {
                    block = new MaterialPropertyBlock();
                    part.GetPropertyBlock(block);
                    Debug.LogFormat(
                        "bsr {0} has tex {1} and NewTex {2} and trans {3} and alpha {4}",
                        part.name,
                        block.GetTexture("_MainTex").name,
                        block.GetTexture("NewTex").name,
                        block.GetFloat("_TransitionAmount"),
                        block.GetFloat("Alpha")
                    );
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("couldnt debug part {0} with error {1}", part.name, e);
                }
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
            // Debug.LogFormat("setting NewTex expression to {0}", expression);
            if (expression == "")
            {
                return;
            }
            var parts = expression.Split(' ');
            Sprite newExpSprite;
            SpriteRenderer bodyPartSpriteRenderer;
            string _depPart = "";
            MaterialPropertyBlock block = new MaterialPropertyBlock();

            foreach (var part in parts)
            {
                var _prefix = part.Split(prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                if (doNotRunSWC.Contains(part))
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
                                _depPart = string.Format(
                                    "{0}{1}{2}",
                                    depPart,
                                    prefixDelimiter,
                                    StringExtensions.ArrayToString(
                                        splitExpression,
                                        1,
                                        splitExpression.Length
                                    )
                                );
                                // Debug.LogFormat("depPart we are looking for: {0}", _depPart);
                                bodyPartSpriteRenderer = bodyPartsMap[depPart];
                                newExpSprite = GetExpressionImage(_depPart);

                                var mpbu =
                                    bodyPartSpriteRenderer.GetComponent<MaterialPropertyBlockUtilities>();
                                if (mpbu != null)
                                {
                                    mpbu.UpdateProperties(
                                        new Tuple<string, Texture2D>("NewTex", newExpSprite.texture)
                                    );
                                    // Debug.LogFormat("set depPart automatically");
                                }
                                else
                                {
                                    Debug.Log("unable to set depPart automatically");
                                }

                                var customSpriteColor =
                                    bodyPartSpriteRenderer.gameObject.GetComponent<CustomSpriteColorSaveLoad>();
                                if (customSpriteColor != null)
                                {
                                    customSpriteColor.SetCustomColorizationPropertyFlags();
                                    // Debug.Log(
                                    //     "manually ran set custom colorization property flags"
                                    // );
                                }
                            }
                            catch
                            {
                                Debug.LogWarningFormat(
                                    "failed to find depPart. Part-{0} depPartPrefix-{1}",
                                    _depPart,
                                    depPart
                                );
                            }
                        }
                    }
                    // do we need to set dependent parts too?
                    bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    newExpSprite = GetExpressionImage(part);

                    // Debug.LogFormat(
                    //     "setting newTex to: {0} for {1}",
                    //     newExpSprite.texture.name,
                    //     gameObject.name
                    // );

                    bodyPartSpriteRenderer
                        .GetComponent<MaterialPropertyBlockUtilities>()
                        .UpdateProperties(
                            new Tuple<string, Texture2D>("NewTex", newExpSprite.texture)
                        );

                    currentExpression[_prefix] = part;
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat(
                        "failed to find part. Part-{0} _prefix-{1}, exception {2}",
                        part,
                        _prefix,
                        e
                    );
                }
                // Debug.Break();
            }
        }

        [SerializeField]
        AnimationCurve transitionInCurve = AnimationCurve.Linear(0, 0, 1, 1);
        List<Coroutine> animates = new List<Coroutine>();
        public UnityEvent OnAnimationStart,
            OnAnimationComplete;

        IEnumerator I_TransitionMaterial(
            SpriteRenderer sr,
            int animateIDX,
            float transitionDuration = 1f
        )
        {
            float elaspedTime = 0;
            // make sure transition starts from 0
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            sr.GetPropertyBlock(propBlock, 0);
            // Debug.LogFormat(
            //     "current value of _transition amount before transitioning is: {0}",
            //     propBlock.GetFloat("_TransitionAmount")
            // );

            propBlock.SetFloat("_TransitionAmount", 0);
            sr.SetPropertyBlock(propBlock, 0);
            while (
                elaspedTime < transitionDuration
                && propBlock.GetFloat("_TransitionAmount") != 1
                && animationsRunning[animateIDX] != false
            )
            {
                sr.GetPropertyBlock(propBlock, 0);
                elaspedTime += Time.deltaTime;
                float curvePercent = transitionInCurve.Evaluate(elaspedTime / transitionDuration);
                propBlock.SetFloat("_TransitionAmount", curvePercent);

                // Debug.LogFormat("elaspedTime {0} transitionDuration {1} curvePercent: {2}", elaspedTime, transitionDuration, curvePercent);

                sr.SetPropertyBlock(propBlock, 0);

                Debug.LogFormat(
                    "current value of _transition amount: {0} for sr {1}",
                    propBlock.GetFloat("_TransitionAmount"),
                    sr.name
                );
                yield return null;
            }

            animationsRunning[animateIDX] = false;
        }

        IEnumerator I_TransitionAlpha(float transitionDuration = 1f)
        {
            float elaspedTime = 0;
            float curvePercent = 0;
            // make sure transition starts from 0

            Test_SetAlphaDirectly(0);
            // transitionDuration = 10;
            while (elaspedTime < transitionDuration && curvePercent != 1)
            {
                elaspedTime += Time.deltaTime;
                curvePercent = transitionInCurve.Evaluate(elaspedTime / transitionDuration);

                Test_SetAlphaDirectly(curvePercent);

                yield return null;
            }

            animationComplete = true;
            Debug.Log("ANIMATION COMPLETE FOR TRANSITION ALPHA");
        }

        async UniTask U_TransitionAlpha(float transitionDuration = 1f, bool fadein = true)
        {
            float elaspedTime = 0;
            float curvePercent = 0;
            // make sure transition starts from 0
            animationComplete = false;

            if (transitionDuration == 0)
            {
                Debug.Log("forcefully setting alpha directly with transition duration 0");
                if (fadein)
                {
                    Test_SetAlphaDirectly(1);
                }
                else
                {
                    Test_SetAlphaDirectly(0);
                }

                elaspedTime = transitionDuration;
                curvePercent = 1;
            }

            while (elaspedTime < transitionDuration && curvePercent != 1)
            {
                elaspedTime += Time.deltaTime;

                if (transitionDuration == 0)
                {
                    curvePercent = 1;
                }
                else
                {
                    curvePercent = transitionInCurve.Evaluate(elaspedTime / transitionDuration);
                }

                // Debug.LogFormat(
                //     "elapsed time: {0}, curvePercent {1}, transDur {2}, fadeIn {3}",
                //     elaspedTime,
                //     curvePercent,
                //     transitionDuration,
                //     fadein
                // );

                if (fadein)
                {
                    // Debug.Log("fadein");
                    Test_SetAlphaDirectly(curvePercent);
                }
                else
                {
                    // Debug.Log("fadeout");
                    Test_SetAlphaDirectly(1 - curvePercent);
                }

                await UniTask.Yield();
            }

            animationComplete = true;
            Debug.Log("ANIMATION COMPLETE FOR TRANSITION ALPHA");
        }

        async UniTask U_TransitionMaterial(float transitionDuration = 1f)
        {
            float elaspedTime = 0;
            float curvePercent = 0;
            while (elaspedTime < transitionDuration && curvePercent <= 1)
            {
                elaspedTime += Time.deltaTime;
                curvePercent = transitionInCurve.Evaluate(elaspedTime / transitionDuration);

                SetMatFloatDirectly("_TransitionAmount", curvePercent);

                await UniTask.Yield();
            }

            animationComplete = true;
            Debug.Log("ANIMATION COMPLETE FOR TRANSITION ALPHA");
        }

        /// <summary>
        /// Updates the visible expression. In theory we might support other transition types,
        /// but for now we only do image interpolation...............
        /// </summary>
        /// <param name="transition">transition type. Unused for as we only support interpolation!</param>
        /// <returns></returns>
        async UniTask RunExpressionTransition(
            float transitionDuration = -1f,
            string transition = ""
        )
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

            await U_TransitionMaterial(transitionDuration);
            // await UniTask.WhenAll(animationTasks);
            await UniTask.Delay(TimeSpan.FromSeconds(transitionDuration)); // TODO ADD GLOBAL ANIMATION CANCELLATION TOKEN

            // await UniTask.WaitUntil(() => IsTransitionComplete() == true);

            // foreach (var animate in animates)
            // {
            //     if (animate != null)
            //     {
            //         StopCoroutine(animate);
            //     }
            // }
            // animates.Clear();
            animationComplete = true;
        }

        bool IsTransitionComplete()
        {
            Debug.LogFormat("length of animations coroutine list: {0}", animates.Count);

            for (int idx = 0; idx < animationsRunning.Count; idx++)
            {
                if (animationsRunning[idx] == true)
                {
                    Debug.LogFormat("sr {0} not done transitioing", idx);
                    return false;
                }
                else { }
            }

            return true;
        }

        async void ResetMainExpression(string expression)
        {
            // Debug.LogFormat("resetting _MainTex expression to {0}", expression);
            if (expression == "")
            {
                return;
            }

            var parts = expression.Split(' ');
            Sprite newExpSprite;
            SpriteRenderer bodyPartSpriteRenderer;
            string _depPart = "";
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            foreach (var part in parts)
            {
                var _prefix = part.Split(prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                if (doNotRunSWC.Contains(part))
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
                                _depPart = string.Format(
                                    "{0}{1}{2}",
                                    depPart,
                                    prefixDelimiter,
                                    StringExtensions.ArrayToString(
                                        splitExpression,
                                        1,
                                        splitExpression.Length
                                    )
                                );
                                // Debug.LogFormat("depPart we are looking for: {0}", _depPart);
                                bodyPartSpriteRenderer = bodyPartsMap[depPart];
                                newExpSprite = GetExpressionImage(_depPart);
                                // bodyPartSpriteRenderer.sprite = newExpSprite;

                                var mpbu =
                                    bodyPartSpriteRenderer.GetComponent<MaterialPropertyBlockUtilities>();
                                if (mpbu != null)
                                {
                                    mpbu.UpdateProperties(
                                        new Tuple<string, Texture2D>(
                                            "_MainTex",
                                            newExpSprite.texture
                                        )
                                    );
                                    // bodyPartSpriteRenderer.texture = newExpSprite.texture;
                                    // Debug.LogFormat("set depPart automatically");
                                }
                                else
                                {
                                    Debug.Log("unable to set depPart automatically");
                                }

                                var customSpriteColor =
                                    bodyPartSpriteRenderer.gameObject.GetComponent<CustomSpriteColorSaveLoad>();
                                if (customSpriteColor != null)
                                {
                                    customSpriteColor.SetCustomColorizationPropertyFlags();
                                    // Debug.Log(
                                    //     "manually ran set custom colorization property flags"
                                    // );
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarningFormat(
                                    "failed to find depPart. Part-{0} depPartPrefix-{1}, exception {2}",
                                    _depPart,
                                    depPart,
                                    e
                                );
                            }
                        }
                    }

                    bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    newExpSprite = GetExpressionImage(part);

                    bodyPartSpriteRenderer
                        .GetComponent<MaterialPropertyBlockUtilities>()
                        .UpdateProperties(
                            new Tuple<string, Texture2D>("_MainTex", newExpSprite.texture)
                        );
                }
                catch
                {
                    Debug.LogWarningFormat(
                        "Unable to locate parts. Prefix-{0}, part-{1}",
                        _prefix,
                        part
                    );
                }
            }

            // Debug.Log("done resetting main expression");
        }

        public async UniTask ExpressionChange(
            string expression = "",
            float transitionDuration = -1f,
            string transition = ""
        )
        {
            SetNewExpression(expression);
            await RunExpressionTransition(transitionDuration, transition);
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();
            ResetMainExpression(expression);
        }

        public void SetInitialExpression(string expression = "")
        {
            if (expression == "")
            {
                return;
            }

            SetNewExpression(expression);
            ResetMainExpression(expression);

            // make sure the transition is at 1......

            Debug.Log("setting initial expression: " + expression);
            foreach (var sr in bodyPartsMap.Values)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                sr.GetPropertyBlock(block, 0);
                block.SetFloat("_TransitionAmount", 1);
                block.SetFloat("Alpha", 0);
                sr.SetPropertyBlock(block, 0);
            }
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
                s.expressionImageName += string.Format("{0} ", kv.Value);
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
            bodyPartsMap.Clear();
        }
    }
}
