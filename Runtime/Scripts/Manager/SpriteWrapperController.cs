using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using AnimeTask;
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
        [Tooltip("NPC data container to know what images to change out body part images with for expression changes.")]
        NPC_SO npc;
        [SerializeField]
        [Tooltip("Body part object that gets modified for a specific expression body part change.")]
        List<BodyPart> bodyParts = new List<BodyPart>();
        [SerializeField]
        [ReadOnly]
        Dictionary<string, SpriteRenderer> bodyPartsMap = new Dictionary<string, SpriteRenderer>();

        bool animationComplete = false;
        /// <summary>
        /// Gets the current displayed expression of character for saving purposes
        /// </summary>
        /// <value></value>
        public string currentExpression
        {
            get
            {
                var s = "";
                foreach (var sr in transform.GetComponentsInChildren<SpriteRenderer>())
                {
                    s += " " + sr.sprite.name;
                }
                return s;

            }
        }

        void Awake()
        {
            bodyPartsMap.Clear();
            foreach (var part in bodyParts)
            {
                bodyPartsMap[part.prefix] = part.gameObject.GetComponentInChildren<SpriteRenderer>();
            }
            // set our initial/default expression
            SetNewExpression();
        }

        /// <summary>
        /// Sets the new expression that we want to update the character to.
        /// This method DOES NOT MAKE VISIBLE the new expression. It only sets it
        /// in newExp shader parameter! You need to run RunExpressionTransition to
        /// update the visibility of this new expression.
        /// </summary>
        /// <param name="expression">list of expressions in a string format with space delimiting. E.g. 
        /// head_1 eyes_angry_1 mouth_sad2</param>
        void SetNewExpression(string expression = "")
        {
            if (expression == "")
            {
                return;
            }
            var parts = expression.Split(' ');
            foreach (var part in parts)
            {

                var _prefix = part.Split(npc.prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                try
                {

                    var bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    var newExpSprite = npc.GetExpressionImage(part);
                    bodyPartSpriteRenderer.material.SetTexture("NewTex", newExpSprite.texture);
                }
                catch
                {
                    Debug.LogErrorFormat("failed to find part. Part-{0} _prefix-{1}", part, _prefix);
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
                        // TECHDEBT: hardcoding the ease =.=
                        animationTasks.Add(
Easing.Create<InCubic>(start: 0f, end: 1f, duration: transitionDuration)
                    .ToMaterialPropertyFloat(sr, "_TransitionAmount")
                        );
                        
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
            foreach (var part in parts)
            {
                var _prefix = part.Split(npc.prefixDelimiter)[0];
                if (part == "" || _prefix == "")
                {
                    continue;
                }
                try
                {
                    var bodyPartSpriteRenderer = bodyPartsMap[_prefix];
                    var newExpSprite = npc.GetExpressionImage(part);

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
            var tintColor = npc.GetTintColor(tintName);
            foreach (var sr in bodyPartsMap.Values)
            {
                sr.material.SetColor("_Tint", tintColor);
            }
        }


        // public void Save()
        // {
        //     var s = new SpriteSaveData();
        //     s.expressionImageName = currentExpression;
        //     s.position = transform.position;
        //     SaveLoadManager.Instance.currentSave.spriteSaveDatas.Add(npc.internalName,s);
        // }
        public SpriteSaveData Save()
        {
            var s = new SpriteSaveData();
            s.expressionImageName = npc.CurrentExpression;

            // this is dumb but hardcoding 
            // TECHDEBT:
            if (DialogueSystemManager.Instance != null)
            {
                if ((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"])
                {
                    s.position = this.transform.parent.parent.position;
                }
                else
                {
                    s.position = transform.position;
                }
            }
            else
            {
                s.position = transform.position;
            }

            Debug.Log("expression: " + npc.CurrentExpression);
            return s;
        }
        public void Load()
        {

        }
    }
}