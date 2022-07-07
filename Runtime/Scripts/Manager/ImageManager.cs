using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
// using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.Serialization;
using System.Threading;
using NaughtyAttributes;
namespace com.argentgames.visualnoveltemplate
{


    public class ImageManager : MonoBehaviour
    {
        public static ImageManager Instance;
        [SerializeField]
        public Camera BGCamera, OnscreenSpriteCamera, PortraitCamera, NewBGCamera;
        [SerializeField]
        RawImage NewBG;
        Material newBGMaterial, newPortraitMaterial, currPortraitMaterial, newSpriteMaterial, currSpriteMaterial;
        [SerializeField]
        GameObject BackgroundHolder; // holds a NEW and CURRENT bgPrefab; all bgs are prefabs that have animations baked into them
        [SerializeField]
        GameObject CharacterLayer;
        [SerializeField]
        PortraitPresenter portraitPresenter;

        Dictionary<string, Shot> cameraShots = new Dictionary<string, Shot>();
        Dictionary<string, Wipe_SO> wipes = new Dictionary<string, Wipe_SO>();

        [SerializeField]
        private Dictionary<string, GameObject> charactersOnScreen = new Dictionary<string, GameObject>();
        Sequence sequence;
        public string CurrentCameraShot;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken ct;

        public bool darkTintOn = false;

        [SerializeField]
        GameObject particleSystemHolder;

        private void Awake()
        {
            Instance = this;
            NewBG.GetComponent<RawImage>().material = Instantiate<Material>(NewBG.GetComponent<RawImage>().material);

            newBGMaterial = NewBG.material;

            var CameraShots = Resources.LoadAll<Shot>("camera shots");
            for (int i = 0; i < CameraShots.Length; i++)
            {
                var shot = CameraShots[i];
                cameraShots.Add(shot.bgName, shot);
            }
            var Wipes = Resources.LoadAll<Wipe_SO>("wipes");
            for (int i = 0; i < Wipes.Length; i++)
            {
                var w = Wipes[i];
                wipes.Add(w.internalName, w);
            }

            sequence = DOTween.Sequence();
            CreateCancellationToken();
        }
        public void CreateCancellationToken()
        {

            this.cts = new CancellationTokenSource();
            this.ct = this.cts.Token;
        }
        public void ThrowCancellationToken()
        {
            cts.Cancel();
        }
        public void SetTint(bool val)
        {
            darkTintOn = val;
        }
        public void SetAllCharactersOnScreenActive()
        {
            foreach (var v in charactersOnScreen.Values)
            {
                v.SetActive(true);
            }
        }
        public void ClearCharactersOnScreen()
        {
            for (int i = 0; i < CharacterLayer.transform.childCount; i++)
            {
                AssetRefLoader.Instance.ReleaseAsset(CharacterLayer.transform.GetChild(i).gameObject);
                // Destroy(CharacterLayer.transform.GetChild(i).gameObject);
            }
            charactersOnScreen.Clear();
        }
        public Dictionary<string, SpriteSaveData> GetAllCharacterOnScreenSaveData()
        {
            var spriteSaveDatas = new Dictionary<string, SpriteSaveData>();
            foreach (var kv in charactersOnScreen)
            {
                if (!kv.Value.activeSelf)
                {
                    continue;
                }
                Debug.Log("saving sprit data for: " + kv.Key);
                // var spriteSaveData = new SpriteSaveData();
                // spriteSaveData.position = kv.Value.transform.position;
                // var s = "";
                // foreach (var sr in kv.Value.GetComponentsInChildren<SpriteRenderer>())
                // {
                //     s += " " + sr.sprite.texture.name;
                // }
                // spriteSaveData.expressionImageName = kv.Value.GetComponent<SpriteWrapperController>().currentExpression;
                spriteSaveDatas.Add(kv.Key, kv.Value.GetComponentInChildren<SpriteWrapperController>().Save());
            }
            return spriteSaveDatas;
        }
        public void SetCameraShot(Camera camera, Vector3? position, Vector3? rotation, float? size)
        {
            if (position != null)
            {
                var p = (Vector3)position;
                if (p.z == 0)
                {
                    p.z = GameManager.Instance.DefaultConfig.defaultBGCameraPosition.z;
                }
                camera.transform.position = p;
            }
            if (rotation != null)
            {
                camera.transform.rotation = Quaternion.Euler((Vector3)rotation);

            }
            if (size != null)
            {
                var _s = (float)size;
                if (_s == 0)
                {
                    _s = GameManager.Instance.DefaultConfig.defaultBGCameraSize;
                }

                camera.orthographicSize = _s;
            }
        }
        public void SetBGCameraShot(Vector3? position, Vector3? rotation, float? size)
        {
            if (position != null)
            {
                var p = (Vector3)position;
                if (p.z == 0)
                {
                    p.z = GameManager.Instance.DefaultConfig.defaultBGCameraPosition.z;
                }
                BGCamera.transform.position = p;
            }
            if (rotation != null)
            {
                BGCamera.transform.rotation = Quaternion.Euler((Vector3)rotation);

            }
            if (size != null)
            {
                var _s = (float)size;
                if (_s == 0)
                {
                    _s = GameManager.Instance.DefaultConfig.defaultBGCameraSize;
                }

                BGCamera.orthographicSize = _s;
            }

        }
        public async UniTask ShowBG(string bgName, string transition = "w9", float duration = 1.4f)
        {
            if (bgName == "")
            {
                Debug.LogError("can't showBG EMPTY STRING");
                return;
            }
            GameObject oldBG = null;
            // get ref to old bg so we can kill it |:
            if (BackgroundHolder.transform.childCount > 0)
            {
                oldBG = BackgroundHolder.transform.GetChild(BackgroundHolder.transform.childCount - 1).gameObject;
            }

            // (optionally enable newBGCam)
            NewBGCamera.gameObject.SetActive(true);

            // spawn BG
            Debug.Log("trying to spawn bg: " + bgName);
            CurrentCameraShot = bgName;
            var shot = cameraShots[bgName];
            var bgAsset = shot.bgPrefab;
            // first spawn the prefab
            var go = await AssetRefLoader.Instance.LoadAsset(bgAsset, BackgroundHolder.transform);
            go.transform.SetSiblingIndex(BackgroundHolder.transform.childCount - 1);


            // set layer to NewBG
            var transforms = go.GetComponentsInChildren<Transform>();

            // set all layer sorts to -= 200 so that it doesn't show up in currCam
            var spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i].sortingOrder < -500)
                {
                    break;
                }
                spriteRenderers[i].sortingOrder -= 1000;
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = LayerMask.NameToLayer("NewBG");
            }

            NewBG.material.SetFloat("TransitionAmount", 0);
            NewBG.material.SetTexture("NewTex", NewBGCamera.activeTexture);

            var transitionWipe = wipes[transition];
            var ease = transitionWipe.ease;
            newBGMaterial.SetFloat("TransitionAmount", 0);


            SetCameraShot(NewBGCamera, shot.position, shot.rotation, shot.size);


            newBGMaterial.SetTexture("Wipe", transitionWipe.wipePrefab);

            sequence = DOTween.Sequence();
            sequence.Pause();
            // if bg shot is black, turn off the noise
            var noiseOpacity = GameManager.Instance.DefaultConfig.bgNoiseOpacity;
            float noiseStartTime = .8f * duration;
            float noiseDuration = .2f * duration;
            bool toggleParticleSystem = true;
#if ANDROID_PLATFORM
        toggleParticleSystem = false;
#endif

            if (bgName == "black")
            {
                Debug.Log("bg name is black, turning off particle system!");
                noiseOpacity = 0;
                toggleParticleSystem = false;
            }
            if (GameManager.Instance.IsSkipping)
            {
                duration = 0.01f;
                noiseStartTime = 0.01f;
                noiseDuration = 0.01f;
            }

            sequence.Join(newBGMaterial.DOFloat(1, "TransitionAmount", duration).SetEase(ease).From(0));
            sequence.Insert(noiseStartTime, newBGMaterial.DOFloat(noiseOpacity, "NoiseOpacity", noiseDuration));

            sequence.InsertCallback(noiseStartTime, () => particleSystemHolder.SetActive(toggleParticleSystem));


            var animationComplete = false;
            go.SetActive(true);
            sequence.Play().OnStart(() =>
            {



            }).OnComplete(() =>
            {
            // Debug.Break();
            animationComplete = true;
                if (oldBG != null)
                {
                    var oldSpriteRenderers = oldBG.GetComponentsInChildren<SpriteRenderer>();
                    for (int i = 0; i < oldSpriteRenderers.Length; i++)
                    {
                        oldSpriteRenderers[i].sortingOrder -= 1000;
                    }
                }

                for (int i = spriteRenderers.Length - 1; i > -1; i--)
                {
                    spriteRenderers[i].sortingOrder += 1000;
                }

            // EXTRA TURN OFF PARTICLE SYSTEM??
            particleSystemHolder.SetActive(toggleParticleSystem);


            });

            if (GameManager.Instance.IsSkipping)
            {
                sequence.Complete();
            }

            await UniTask.WaitUntil(() => animationComplete);
            // set newBG sort order to +200.

            // }

            // // reset alpha in case we ran a regular fade...
            // newBGMaterial.SetFloat("Alpha",1);

            // set currBGCamera shot to same shot as newBGCamera
            SetCameraShot(BGCamera, shot.position, shot.rotation, shot.size);



            // destroy oldBG, so currBGCam can see newBG now.
            // only destroyOld BG if there is a preexisting bg, and also
            // if it isn't the one we just spawned, just in case

            for (int i = 0; i < BackgroundHolder.transform.childCount - 1; i++)
            {
                BackgroundHolder.transform.GetChild(i).gameObject.SetActive(false);
            }


            // if (oldBG != null)
            // {
            //     var oldSpriteRenderers = oldBG.GetComponentsInChildren<SpriteRenderer>();
            //     for (int i = 0; i < oldSpriteRenderers.Length; i++)
            //     {
            //         oldSpriteRenderers[i].sortingOrder -= 1000;
            //     }
            // }

            // reset transitionAmount 
            newBGMaterial.SetFloat("TransitionAmount", 0);
            // change newBG layer to Layer:Default 
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.layer = LayerMask.NameToLayer("Default");
            }

            // (optionally disable newBGCam since not using to free up resources) 
            // NewBGCamera.gameObject.SetActive(false);

#if PLATFORM_ANDROID

        if (BackgroundHolder.transform.childCount > 3)
        {
            for (int i = 0; i < BackgroundHolder.transform.childCount - 2; i++)
            {
                            AssetRefLoader.Instance.ReleaseAsset(BackgroundHolder.transform.GetChild(i).gameObject);

                // Destroy(BackgroundHolder.transform.GetChild(i).gameObject);
            }
        }
#endif

        }
        public void HideBG(string bgName, string transition = "wipe", float duration = .4f)
        {
            BackgroundHolder.gameObject.SetActive(false);
        }
        public async UniTaskVoid MoveCam(string moveType, Vector3 newPosition, float duration = 0f)
        {

            if (!sequence.IsActive())
            {
                sequence = DOTween.Sequence();
            }
            if (GameManager.Instance.IsSkipping)
            { duration = 0.002f; }
            switch (moveType)
            {
                case "position":
                    if (newPosition.z == 0)
                    {
                        newPosition.z = GameManager.Instance.DefaultConfig.defaultBGCameraPosition.z;
                    }
                    sequence.Join(
                        BGCamera.transform.DOLocalMove(newPosition, duration)
                    );

                    break;
                case "rotation":
                    sequence.Join(
                        BGCamera.transform.DOLocalRotate(newPosition, duration)
                    );

                    break;
                case "size":
                    sequence.Join(
                        BGCamera.DOOrthoSize(newPosition.x, duration)
                    );

                    break;
                default:
                    Debug.LogErrorFormat("camera move type {0} doesn't exist", moveType);
                    break;
            }
        }
        public void ShakeCam(float duration = .6f)
        {
            if (GameManager.Instance.Settings.enableScreenShake)
            {
                BGCamera.DOShakePosition(duration, 1.3f, 4);
                OnscreenSpriteCamera.DOShakePosition(duration, 1.3f, 4);
            }

        }
        public void PlayBGTween()
        {
            sequence.Play();
            if (GameManager.Instance.IsSkipping)
            {
                SkipBGTween();
            }
        }
        public void SkipBGTween()
        {
            sequence.Complete();
            ResetBGTween();

        }
        public bool NeedToCompleteTweensEarly = false;
        public void ResetBGTween()
        {
            sequence = DOTween.Sequence();
        }
        // public void PlayBGAnimation(string animationName)
        // {
        //     BackgroundHolder.transform.GetChild(0).GetComponent<ImageAnimations>().PlayAnimation(animationName);
        // }
        public async UniTask SpawnCharFromSave(string charName, SpriteSaveData saveData)
        {
            var npc = (NPC_SO)DialogueSystemManager.Instance.GetNPC(charName);
            GameObject charSprite;
            AssetReference assetToLoad;
            if ((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"])
            {
                if (npc.portraitSprite != null)
                {
                    assetToLoad = npc.portraitSprite;
                }
                else
                {
                    assetToLoad = npc.mainSprite;
                }
            }
            else
            {
                assetToLoad = npc.mainSprite;
            }

            charSprite = await AssetRefLoader.Instance.LoadAsset(assetToLoad, CharacterLayer.transform);
            charSprite.SetActive(false);
            // Debug.Break();
            // TECHDEBT this should never have stuff in it???
            if (!charactersOnScreen.ContainsKey(charName.TrimStart(null).TrimEnd(null)))
            {
                charactersOnScreen.Add(charName.TrimStart(null).TrimEnd(null), charSprite);
            }

            Debug.Log("save data expression: " + saveData.expressionImageName);
            npc.SetExpression(charSprite, saveData.expressionImageName);
            npc.SetNewOldExpression(charSprite, saveData.expressionImageName);
            Debug.LogFormat("position to spawn at {0}", saveData.position);
            ShowChar(charName, saveData.position, duration: 0);
        }
        [Button]
        public async UniTask SpawnChar(string charName, string expression, Vector3? location = null, float? duration = null)
        {
            if (duration == null)
            {
                duration = (float)GameManager.Instance.DefaultConfig.spawnCharacterDuration;
            }
            var npc = (NPC_SO)DialogueSystemManager.Instance.GetNPC(charName);
            GameObject charSprite;
            Debug.LogFormat("SpawnChar parmas: {0}, {1}, {2}", charName, expression, location);
            if (!charactersOnScreen.ContainsKey(charName.TrimStart(null).TrimEnd(null)))
            {
                Debug.Log("need to spawn new char");
                AssetReference assetToLoad = new AssetReference();

                if ((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"])
                {
                    if (npc.portraitSprite != null)
                    {
                        assetToLoad = npc.portraitSprite;
                    }
                    else
                    {
                        assetToLoad = npc.mainSprite;
                    }
                }
                else
                {
                    assetToLoad = npc.mainSprite;
                }
                charSprite = await AssetRefLoader.Instance.LoadAsset(assetToLoad, CharacterLayer.transform);
                charSprite.SetActive(false);
                // Debug.Break();
                charactersOnScreen[charName.TrimStart(null).TrimEnd(null)] = charSprite;
            }
            else
            {
                Debug.Log("char already on screen, reusing!");
                charSprite = charactersOnScreen[charName.TrimStart(null).TrimEnd(null)];
            }

            npc.SetInitialExpression(charSprite, expression);
            // Debug.Log(charactersOnScreen.Count);
            if (!((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"]))
            {
                if (location == null)
                {
                    location = npc.defaultSpawnPosition;
                }
                // move char to location
                charSprite.transform.position = (Vector3)location;
            }


            // OnscreenSpriteCamera.Render();

            // set both new and old exps and temp
            // handler.transform.GetChild(0).GetComponentInChildren<Image>().sprite = npc.Expressions[expression];
            // handler.transform.GetChild(1).GetComponentInChildren<Image>().sprite = npc.Expressions[expression];
            // Debug.Break();

            // TECHDEBT: sidepanel is basically a portrait so we want instant show
            if ((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"])
            {
                duration = 0;
            }
            await ShowChar(charName, location, duration: (float)duration);
            // OnscreenSpriteCamera.Render();

        }
        public async UniTask ShowChar(string charName, Vector3? location, string transition = "dissolve", float duration = 1f)
        {
            // Debug.Log("lol am i ever called");
            var go = charactersOnScreen[charName];
            var animationComplete = false;
            if (location != null)
            {
                go.transform.DOLocalMove((Vector3)location, duration).OnComplete(() =>
                 {
                     animationComplete = true;
                 });
            }
            else
            {
                animationComplete = true;
            }
            if (GameManager.Instance.IsSkipping)
            { duration = 0.002f; }
            else
            {
                duration = GameManager.Instance.DefaultConfig.spawnCharacterDuration;
            }
            if (!go.activeSelf)
            {
                animationComplete = false;
                var spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
                sequence = DOTween.Sequence();

                foreach (var sr in spriteRenderers)
                {
                    if (sr.material.HasProperty("Alpha"))
                    {
                        sequence.Join(sr.material.DOFloat(1, "Alpha", duration).From(0));
                    }
                    else
                    {
                        sequence.Join(sr.DOFade(1, duration).From(0));
                        Debug.Log("where is my spawn fade in basic alpha");
                    }

                    if (sr.material.HasProperty("DoTint"))
                    {
                        if (this.darkTintOn)
                        {
                            Debug.Log("turn tint on");
                            // 1 is true
                            sr.material.SetFloat("DoTint", 1);
                        }
                        else
                        {
                            Debug.Log("turn tint off");
                            sr.material.SetFloat("DoTint", 0);
                        }
                    }


                }
                sequence.Play().OnStart(() =>
                {
                    go.SetActive(true);
                }).OnComplete(() =>
                {
                    animationComplete = true;
                });
            }
            await UniTask.WaitUntil(() => animationComplete);
            // move the gameobject to Location
            // animate showing with transition and duration
            if (!GameManager.Instance.IsSkipping)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText));
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
            }


        }
        [Button]
        public void HideChar(string charName, string transition = "fade", float? duration = null)
        {
            if (!charactersOnScreen.ContainsKey(charName))
            {
                Debug.LogWarningFormat("Character {0} isn't on screen to hide", charName);
                return;
            }
            if (!sequence.IsActive())
            {
                sequence = DOTween.Sequence();
            }
            var go = charactersOnScreen[charName];
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.hideCharacterDuration;
            }
            if (GameManager.Instance.IsSkipping)
            { duration = 0; }
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
            {
                sequence.Join(sr.material.DOFloat(0, "Alpha", (float)duration));
            }
            sequence.AppendCallback(() =>
            {
                go.SetActive(false);
            // charactersOnScreen.Remove(charName);
            if (GameManager.Instance.IsSkipping)
                {
                    SkipBGTween();
                }
            // #if PLATFORM_ANDROID
            // // destroy some old chars that aren't used anymore
            // if (CharacterLayer.transform.childCount > 2)
            // {
            foreach (var character in charactersOnScreen)
                {
                    if (!character.Value.activeSelf)
                    {
                        charactersOnScreen.Remove(character.Key);
                        AssetRefLoader.Instance.ReleaseAsset(character.Value);
                    }
                }
            // }
            // #endif
        });

        }
        [Button]
        public async UniTask HideAllChar(float? duration = null)
        {
            sequence = DOTween.Sequence();
            var charsOnScreen = new List<string>(charactersOnScreen.Keys);
            for (int i = 0; i < charsOnScreen.Count; i++)
            {
                HideChar(charsOnScreen[i], duration: duration);
            }
        }
        [SerializeField]
        NPC_SO currentNPC;
        public GameObject char_;
        public async UniTask<bool> ExpressionChange(string charName, string expression, float? duration = .35f)
        {
            Debug.LogFormat("expchange args for char {0}: {1}", charName, expression);
            // OnscreenSpriteCamera.Render();

            //exp sandwich is currExp > newExp 
            // set new exp, fade out currexp, set currexp=newexp, 
            var npc = (NPC_SO)DialogueSystemManager.Instance.GetNPC(charName);
            currentNPC = npc;
            // if it's a portrait char, then get the portrait one, otherwise get the main big sprite
            // GameObject char_;
            bool needToShowPortrait = false;
            // Debug.Log(charactersOnScreen.Count);
            if (charactersOnScreen.ContainsKey(charName))
            {
                char_ = charactersOnScreen[charName];
            }
            else

            {
                Debug.Log(charactersOnScreen.Count);
                foreach (var k in charactersOnScreen.Keys)
                {
                    Debug.LogFormat("char on screen: {0}", k);
                }
                /// TECHDEBT, commenting out for now...
                /// GetCharGO used to get the current speaking character game object so that we could change its expression and/or make it visible.
                // char_ = portraitPresenter.GetCharGO(npc.npcName);
                char_ = null;
                needToShowPortrait = true;
            }

            if (expression == null || expression.TrimStart(null).TrimEnd(null) == "")
            {
                Debug.Log("don't need to change exp");
                return needToShowPortrait;
            }

            if (expression.TrimStart(null).TrimEnd(null) != "")
            {
                npc.SetExpression(char_, expression);
            }
            Debug.Log("done setting expression, all newTex should have new exp");
            // Debug.Break();
            // TODO: not really a todo, but we arn't supporting other transition types for exp change

            if (duration == null || duration == -1)
            {
                duration = GameManager.Instance.DefaultConfig.expressionChangeDuration;
            }
            if (GameManager.Instance.IsSkipping)
            {
                duration = 0.002f;
            }
            await npc.UpdateExpression(char_, (float)duration);
            Debug.Log("done updating expression, transition should be 1");
            // Debug.Break();
            // after we have transitioned between current => new expression, we need to set the textures so that
            // current == new textures
            npc.SetNewOldExpression(char_, expression);
            Debug.Log("done copy newTex expressions down to mainTex");
            // Debug.Break();
            npc.ResetTransitionAmount(char_);
            Debug.Log("reset transition amount to 0");
            // Debug.Break();

            if (!GameManager.Instance.IsSkipping)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText));
            }
            else
            {
                await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
            }

            // Debug.LogFormat("do i need to show portrait?: {0}", needToShowPortrait);
            // OnscreenSpriteCamera.Render();
            return needToShowPortrait;

        }
        public void ShowPortrait()
        {
            portraitPresenter.ShowPortrait();
        }
        public void HidePortrait()
        {
            portraitPresenter.HidePortrait();
        }

        public async UniTask ShowShot(string shotName)
        {
            Shot shot = cameraShots[shotName];
            BGCamera.transform.position = shot.position;
            BGCamera.transform.rotation = Quaternion.Euler(shot.rotation);
            BGCamera.orthographicSize = shot.size;
            await ShowBG(shot.bgName);
        }
    }
}