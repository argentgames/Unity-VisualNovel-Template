using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using DG.Tweening;
public abstract class SpriteNPC_SO : NPC_SO
{
    public AssetReference mainSprite, sideSprite;
    public Dictionary<string, string> expressionsMapForHead = new Dictionary<string, string>();
    public Dictionary<string, Sprite> expressions = new Dictionary<string, Sprite>();


    public abstract void SetExpressionTextures(GameObject go, string exp, string textureName);
    public abstract void SetMainTextureExpression(GameObject go, string exp);
    public Vector3 defaultSpawnPosition;
    public Dictionary<ScreenPosition, Vector2> positions = new Dictionary<ScreenPosition, Vector2>();
    public string defaultExp = "";
    public virtual string CurrentExpression { get; set; }
    public GameObject bodyPartsHolder;
    Sequence sequence;
    string tag = "sprite expression holder";
    public Color darkTintColor = new Color(89,81,81,0);

    public void GenerateExpressionsMapForHead()
    {
        expressionsMapForHead.Clear();
        foreach (KeyValuePair<string, Sprite> keyValuePair in expressions)
        {
            expressionsMapForHead.Add(keyValuePair.Value.name, keyValuePair.Key);
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
    public virtual void SetExpression(GameObject go, string exp)
    {
        bodyPartsHolder = FindWithTag(go.transform).gameObject;
        SetExpressionTextures(bodyPartsHolder, exp, "NewTex");

    }


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
    public virtual void SetInitialExpression(GameObject go, string exp)
    {

        // string defaultExp =  string.Format("{0} {1} {2} {3}", defaultArm, defaultExpression, defaultGlasses, defaultSweat);

        // since we go through exps and update them in order, lets set all the defaults
        // and then override the necessary ones at the end
        if (exp != null)
        {
            exp = defaultExp + " " + exp;
        }

        bodyPartsHolder = FindWithTag(go.transform).gameObject;

        // set the base layer expression
        SetMainTextureExpression(bodyPartsHolder, exp);
        // set the new layer expression. now we have a single current expression "current" == "new" xpression
        SetExpressionTextures(bodyPartsHolder, exp, "NewTex");

        // reset transition amount so that we are showing the mainTex sprite
        foreach (var sr in bodyPartsHolder.GetComponentsInChildren<SpriteRenderer>())
        {
            
            sr.material.SetFloat("_TransitionAmount", 0);
            sr.material.SetColor("Tint",darkTintColor);
        }

    }

    public virtual async UniTask UpdateExpression(GameObject go, float duration = 0f)
    {
        var animationComplete = false;
        var spriteWrapper = FindWithTag(go.transform);

        var spriteRenderers = spriteWrapper.GetComponentsInChildren<SpriteRenderer>();

        sequence = DOTween.Sequence();
        Debug.Log(spriteRenderers[0].sprite.name);
        foreach (var sr in spriteRenderers)
        {
            if (sr.gameObject.tag == "no_transition")
            {
                continue;
            }

            if (sr.material.GetTexture("NewTex") != null)
            {
                if (sr.material.GetTexture("NewTex").name != sr.sprite.texture.name)
            {
                sequence.Join(sr.material.DOFloat(1, "_TransitionAmount", duration).SetEase(Ease.InOutCubic).From(0));
            }
            else
            {
                Debug.LogFormat("newTex {0} is same as mainTex {1}", sr.material.GetTexture("NewTex").name, sr.sprite.name);
            }
            }
            

        }

        sequence.Play().OnComplete(() =>
        {
            animationComplete = true;
        }
        ).OnStart(() => Debug.Log("starting sequ")).OnUpdate(() =>
        {
            if (spriteRenderers[0].material.GetFloat("_TransitionAmount") >= .5)
            {

            }
        }

            );//.OnUpdate(() => ImageManager.Instance.OnscreenSpriteCamera.Render());
        if (GameManager.Instance.IsSkipping)
        {
            sequence.Complete();
        }

        await UniTask.WaitUntil(() => animationComplete);
        await UniTask.Yield();

        foreach (var sr in spriteRenderers)
        {
            if (sr.gameObject.tag == "no_transition")
            {
                continue;
            }
            if (sr != null)
            {
                // sr.material.SetFloat("_TransitionAmount", 0);

            }
        }

    }

    public void ResetTransitionAmount(GameObject go)
    {
        var spriteRenderers = go.transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            if (sr.gameObject.tag == "no_transition")
            {
                continue;
            }
            if (sr != null)
            {
                sr.material.SetFloat("_TransitionAmount", 0);

            }
        }
    }

}
