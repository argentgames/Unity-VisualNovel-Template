using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;
// using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

using Sirenix.Serialization;
using System.Threading;
using NaughtyAttributes;
using AnimeTask;
/// <summary>
/// Controls character and background images! Attached to a large prefab. 
/// 
/// HOW THIS WORKS: We spawn 
/// 
/// BACKGROUNDS
/// 
/// ProjectedBGObject: The final flat background image visible to the player. Has a shader
/// that allows transitioning from preTransition ==> postTransition image states.
/// CurrentBGCamera: Outputs to the preTransition renderTexture
/// NewBGCamera: Outputs to the postTransition renderTexture. "origin" at (0,1000,0)
/// 
/// 1. Spawn a new background under NewBGContainer (which has some weird origin like (0,1000,0) 
/// so that it's inviisble to the CurrentBGCamera)
/// 2. Run transition from preTransition ==> postTransition. ProjectedBGObject now shows NewBG.
/// 3. IN A SINGLE FRAME: parent new newBG to CurrentBGContainer, reset the transform position to (0,0,0) + newBGAnimatinoTransform,
/// delete currentBG. Reset transition to preTransition state.
/// 4. DOUBLE CHECK THIS WORKS WITH ANIMATIONS/BG PANNING?? (our cameras are STATIC. any image panning adjusts the image transform, NOT the camera transform)
/// 
/// 
/// 
/// </summary>
namespace com.argentgames.visualnoveltemplate
{
    public class ImageManager : MonoBehaviour
    {
        public static ImageManager Instance { get; set; }
        [BoxGroup("Image containers")]
        [SerializeField]
        GameObject NewBackgroundContainer;
        [BoxGroup("Image containers")]
        [InfoBox("Parents that hold spawned characters and overlays.")]
        [SerializeField]
        GameObject BackgroundContainer;
        [BoxGroup("Image containers")]
        [SerializeField]
        GameObject MidgroundCharacterContainer;
        [BoxGroup("Image containers")]
        [SerializeField]
        GameObject ForegroundCharacterContainer;
        [BoxGroup("Image containers")]
        [SerializeField]
        GameObject OverlayContainer;

        [BoxGroup("Cameras")]
        [InfoBox("Cameras that view SpriteRenderers and then project them to render textures.")]
        [SerializeField]
        public Camera CurrentBGCamera, MidgroundCharactersCamera, ForegroundCharactersCamera, NewBGCamera;

        [SerializeField]
        Material newBGMaterial;
        [SerializeField]
        GameObject BackgroundProjectedImage;
        Renderer backgroundProjectedImageRenderer;


        [InfoBox("Controller for a side portrait that lives on the ForegroundCharacter layer. Only one portrait is allowed on screen at a time!")]
        [SerializeField]
        PortraitPresenter portraitPresenter;



        /// <summary>
        /// Mapping of shots for where to place camera and which background prefab to spawn.
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="Shot"></typeparam>
        /// <returns></returns>
        Dictionary<string, Shot> cameraShots = new Dictionary<string, Shot>();
        [SerializeField]
        List<Shot> shots = new List<Shot>();


        /// <summary>
        /// Keep reference of which characters are currently on screen (including portrait character)
        /// so we can manipulate them without respawning them.
        /// </summary>
        /// <typeparam name="string">internal character name reference</typeparam>
        /// <typeparam name="GameObject"></typeparam>
        /// <returns></returns>
        [SerializeField]
        private Dictionary<string, GameObject> charactersOnScreen = new Dictionary<string, GameObject>();

        /// <summary>
        /// Keep track of which characters have what tints currently active. Mainly a save/load thing.
        /// If a character has activeTint="", then no tint is being applied.
        /// </summary>
        /// <typeparam name="string">character name</typeparam>
        /// <typeparam name="string">active tint name</typeparam>
        /// <returns></returns>
        private Dictionary<string, string> activeCharacterTints = new Dictionary<string, string>();

        // Sequence sequence;
        List<UniTask> animationTasks = new List<UniTask>();

        /// <summary>
        /// Keep track of the current Shot on screen for Save/Load purposes
        /// </summary>
        /// <value></value>
        public string CurrentCameraShot { get { return currentCameraShot; } }
        private string currentCameraShot;

        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken ct;
        SkipTokenSource skipTokenSource = new SkipTokenSource();
        SkipToken skipToken;


        [SerializeField]
        GameObject particleSystemHolder;

        Vector3 newBGContainerPosition = new Vector3(0, 1000, 0);

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            newBGContainerPosition = NewBackgroundContainer.transform.position;


            foreach (var shot in shots)
            {
                Debug.Log(shot);
                cameraShots[shot.bgName] = shot;
            }

            animationTasks.Clear();
            backgroundProjectedImageRenderer = BackgroundProjectedImage.GetComponent<Renderer>();
            CreateCancellationToken();
        }
        public void CreateSkipToken()
        {
            this.skipTokenSource = new SkipTokenSource();
            skipToken = skipTokenSource.Token;
        }
        public void ThrowSkipToken()
        {
            skipTokenSource.Skip();
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
        public void SetAllCharactersOnScreenActive()
        {
            foreach (var v in charactersOnScreen.Values)
            {
                v.SetActive(true);
            }
        }
        public void ClearCharactersOnScreen()
        {
            for (int i = 0; i < MidgroundCharacterContainer.transform.childCount; i++)
            {
                AssetRefLoader.Instance.ReleaseAsset(MidgroundCharacterContainer.transform.GetChild(i).gameObject);
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
                Debug.LogFormat("which camera {0}, what position {1}", camera.name, p);
                camera.transform.localPosition = p;
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
        public void SetCurrentBGCameraShot(Vector3? position, Vector3? rotation, float? size)
        {
            SetCameraShot(CurrentBGCamera, position, rotation, size);
        }
        public void SetNewBGCameraShot(Vector3? position, Vector3? rotation, float? size)
        {
            Vector3 pos = newBGContainerPosition;
            if (position != null)
            {
                pos = newBGContainerPosition + (Vector3)position;
            }
            Debug.LogFormat("set new bg camera shot to positon {0}", pos);
            SetCameraShot(NewBGCamera, pos, rotation, size);
        }

        /// <summary>
        /// Show a new background with a given transition such as a dissolve or wipe.
        /// </summary>
        /// <param name="bgName"></param>
        /// <param name="transition"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        [Button]
        public async UniTask ShowBG(string bgName, string transition = "w9", float duration = 1.4f)
        {
            if (bgName == "")
            {
                Debug.LogError("can't showBG EMPTY STRING");
                return;
            }
            GameObject oldBG = null;
            // get ref to old bg so we can kill it |:
            if (BackgroundContainer.transform.childCount > 0)
            {
                oldBG = BackgroundContainer.transform.GetChild(BackgroundContainer.transform.childCount - 1).gameObject;
            }
            else if (OverlayContainer.transform.childCount > 0)
            {
                oldBG = OverlayContainer.transform.GetChild(OverlayContainer.transform.childCount - 1).gameObject;
            }

            // (optionally enable newBGCam)
            NewBGCamera.gameObject.SetActive(true);

            // spawn BG
            Debug.Log("trying to spawn bg: " + bgName);
            currentCameraShot = bgName;

            // foreach (var shotName in cameraShots.Keys)
            // {
            //     Debug.LogFormat("shot name available: {0}", shotName);
            // }

            var shot = cameraShots[bgName];
            GameObject newBGGO;
            if (shot.UseAddressables)
            {
                var bgAsset = shot.bgAssetReference;
                // first spawn the prefab
                newBGGO = await AssetRefLoader.Instance.LoadAsset(bgAsset, NewBackgroundContainer.transform);
            }
            else
            {
                newBGGO = GameObject.Instantiate(shot.bgPrefab, NewBackgroundContainer.transform);
            }

            // newBGGO.transform.SetSiblingIndex(BackgroundContainer.transform.childCount - 1);

            // set all spriteRenderer sorting order to -= 1000 so that it doesn't show up in currCam
            // var spriteRenderers = newBGGO.GetComponentsInChildren<SpriteRenderer>();
            // for (int i = 0; i < spriteRenderers.Length; i++)
            // {
            //     if (spriteRenderers[i].sortingOrder < -500)
            //     {
            //         break;
            //     }
            //     spriteRenderers[i].sortingOrder -= 1000;
            // }

            Wipe_SO transitionWipe;
            // TODO: figure out how to specify Ease with AnimeTask
            // Ease ease;
            if (transition != "dissolve")
            {
                transitionWipe = GameManager.Instance.GetWipe(transition);
                //  ease = transitionWipe.ease;

                newBGMaterial.SetTexture("Wipe", transitionWipe.wipePrefab);
            }
            else
            {
                // ease = GameManager.Instance.DefaultConfig.expressionTransitionEase;
            }

            newBGMaterial.SetFloat("TransitionAmount", 0);
            newBGMaterial.SetTexture("NewTex", NewBGCamera.activeTexture);


            SetNewBGCameraShot(shot.position, shot.rotation, shot.size);

            animationTasks.Clear();

            // if bg shot is black, turn off the noise and particle system
            var noiseOpacity = GameManager.Instance.DefaultConfig.bgNoiseOpacity;
            // TECHDEBT: random hardcoded noise time/duration multiplication
            float noiseStartTime = .8f * duration;
            float noiseDuration = .2f * duration;
            bool toggleParticleSystem = true;
            // #if ANDROID_PLATFORM
            //         toggleParticleSystem = false;
            // #endif

            // TECHDEBT: maybe later you want to transition to black without turning off particle system/noise...
            // should switch to requiring explicitly using the overlay camera?
            if (bgName == "black")
            {
                Debug.Log("bg name is black, turning off particle system!");
                noiseOpacity = 0;
                toggleParticleSystem = false;
            }

            // add in some small transition duration when skipping so things to don't bug out
            if (GameManager.Instance.IsSkipping)
            {
                duration = 0.01f;
                noiseStartTime = 0.01f;
                noiseDuration = 0.01f;
            }

            // set up the animation sequencer
            // create a sequence to simultaneously play the transition wipe and turn on/off noise

            if (transition == "dissolve")
            {
                animationTasks.Add(
                    //TECHDEBT: hardcoded Ease
                    Easing.Create<InCubic>(start: 1f, end: 0f, duration: duration)
                    .ToMaterialPropertyFloat(backgroundProjectedImageRenderer, "Alpha", skipToken: skipTokenSource.Token)
                );
                // sequence.Join(newBGMaterial.DOFloat(0, "Alpha", duration).SetEase(ease).From(1));
            }

            animationTasks.Add(
                    Easing.Create<InCubic>(start: 0f, end: 1f, duration: duration)
                    .ToMaterialPropertyFloat(backgroundProjectedImageRenderer, "TransitionAmount", skipToken: skipTokenSource.Token)
            );
            // sequence.Join(newBGMaterial.DOFloat(1, "TransitionAmount", duration).SetEase(ease).From(0));
            // TECHDEBT: add noise animation delay later
            // sequence.Insert(noiseStartTime, newBGMaterial.DOFloat(noiseOpacity, "NoiseOpacity", noiseDuration));
            // sequence.InsertCallback(noiseStartTime, () => particleSystemHolder.SetActive(toggleParticleSystem));


            newBGGO.SetActive(true);

            // sequence.Play().OnComplete(() =>
            // {
            //     // Debug.Break();
            //     animationComplete = true;
            //     // if (oldBG != null)
            //     // {
            //     //     var oldSpriteRenderers = oldBG.GetComponentsInChildren<SpriteRenderer>();
            //     //     for (int i = 0; i < oldSpriteRenderers.Length; i++)
            //     //     {
            //     //         oldSpriteRenderers[i].sortingOrder -= 1000;
            //     //     }
            //     // }

            //     // for (int i = spriteRenderers.Length - 1; i > -1; i--)
            //     // {
            //     //     spriteRenderers[i].sortingOrder += 1000;
            //     // }

            //     // EXTRA TURN OFF PARTICLE SYSTEM??
            //     particleSystemHolder.SetActive(toggleParticleSystem);


            // });

            // if we are skipping, automatically run the entire sequence
            // idk if this actually works or only runs through the first tween??
            if (GameManager.Instance.IsSkipping)
            {
                ThrowSkipToken();
            }

            await UniTask.WhenAll(animationTasks);

            // set currBGCamera shot to same shot as newBGCamera
            SetCurrentBGCameraShot(shot.position, shot.rotation, shot.size);
            // move newBG down to currBGContainer
            newBGGO.transform.position -= newBGContainerPosition;
            newBGGO.transform.SetParent(BackgroundContainer.transform);

            // Debug.Break();

            // destroy oldBG, so currBGCam can see newBG now.
            // only destroyOld BG if there is a preexisting bg, and also
            // if it isn't the one we just spawned, just in case
            if (oldBG != null)
            {
                oldBG.SetActive(false);
            }


            // reset transitionAmount 
            newBGMaterial.SetFloat("TransitionAmount", 0);
            if (transition == "dissolve")
            {
                newBGMaterial.SetFloat("Alpha", 1);
            }

            // (optionally disable newBGCam since not using to free up resources) 
            // NewBGCamera.gameObject.SetActive(false);

            // if (oldBG != null)
            // {
            //     if (shot.UseAddressables)
            //     {
            //         AssetRefLoader.Instance.ReleaseAsset(oldBG.gameObject);
            //     }
            //     else
            //     {
            //         Destroy(oldBG);
            //     }
            // }







        }

        public void HideBG(string bgName, string transition = "dissolve", float duration = .4f)
        {
            ShowBG("black", duration: duration);
        }
        public async UniTaskVoid MoveCam(string moveType, Vector3 newPosition, float duration = 0f)
        {

            animationTasks.Clear();

            if (GameManager.Instance.IsSkipping)
            { duration = 0.002f; }
            switch (moveType)
            {
                case "position":
                    if (newPosition.z == 0)
                    {
                        newPosition.z = GameManager.Instance.DefaultConfig.defaultBGCameraPosition.z;
                    }

                    animationTasks.Add(
                        Easing.Create<InQuart>(to: newPosition, duration: duration).ToLocalPosition(CurrentBGCamera.transform)
                    );


                    break;
                case "rotation":
                    animationTasks.Add(
                        Easing.Create<InQuart>(to: Quaternion.Euler(newPosition.x, newPosition.y, newPosition.z), duration: duration)
                        .ToLocalRotation(CurrentBGCamera.transform)
                    );

                    break;
                case "size":

                    animationTasks.Add(
                        Easing.Create<InQuart>(CurrentBGCamera.orthographicSize, newPosition.x, duration: duration)
                        .ToAction<float>(x => CurrentBGCamera.orthographicSize = x)
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
                // CurrentBGCamera.DOShakePosition(duration, 1.3f, 4);
                // MidgroundCharactersCamera.DOShakePosition(duration, 1.3f, 4);
            }

        }
        async UniTaskVoid FireBGTween()
        {
            await UniTask.WhenAll(animationTasks);
        }
        public void PlayBGTween()
        {
            FireBGTween().Forget();

            if (GameManager.Instance.IsSkipping)
            {
                ThrowSkipToken();
            }
        }
        public void SkipBGTween()
        {
            ThrowSkipToken();
            ResetBGTween();

        }
        public bool NeedToCompleteTweensEarly = false;
        public void ResetBGTween()
        {
            animationTasks.Clear();
        }
        // public void PlayBGAnimation(string animationName)
        // {
        //     BackgroundHolder.transform.GetChild(0).GetComponent<ImageAnimations>().PlayAnimation(animationName);
        // }
        public async UniTask SpawnCharFromSave(string charName, SpriteSaveData saveData)
        {
            var npc = (NPC_SO)DialogueSystemManager.Instance.GetNPC(charName);
            GameObject charSprite;
            Debug.Log("need to spawn new char");
            var layerToSpawn = MidgroundCharacterContainer;
            switch (npc.spawnLayer)
            {
                case ImageLayer.Foreground:
                    layerToSpawn = ForegroundCharacterContainer;
                    break;
                case ImageLayer.Midground:
                    layerToSpawn = MidgroundCharacterContainer;
                    break;
            }

            if (npc.UseAddressables)
            {
                AssetReference assetToLoad = new AssetReference();


                assetToLoad = npc.charGameObjectAssetRef;


                charSprite = await AssetRefLoader.Instance.LoadAsset(npc.charGameObjectAssetRef, layerToSpawn.transform);

            }
            else
            {
                charSprite = GameObject.Instantiate(npc.charGameObject, layerToSpawn.transform);
            }
            charSprite.SetActive(false);
            // Debug.Break();
            // TECHDEBT this should never have stuff in it???
            if (!charactersOnScreen.ContainsKey(charName.TrimStart(null).TrimEnd(null)))
            {
                charactersOnScreen.Add(charName.TrimStart(null).TrimEnd(null), charSprite);
            }

            var charSpriteWrapperController = charSprite.GetComponentInChildren<SpriteWrapperController>();

            Debug.Log("save data expression: " + saveData.expressionImageName);
            charSpriteWrapperController.ExpressionChange(saveData.expressionImageName, 0);
            Debug.LogFormat("position to spawn at {0}", saveData.position);
            ShowChar(charName, saveData.position, duration: 0);
        }
        [Button]
        /// <summary>
        /// Character does not exist on screen. Spawn it with provided expression.
        /// TODO: support different spawn transitions (e.g. sliding in, not only fading in)
        /// </summary>
        /// <param name="charName">The character to spawn</param>
        /// <param name="expression">The initial expression</param>
        /// <param name="location">Location of character spawn point</param>
        /// <param name="transition">Transition type, e.g. dissolve, slideFromRight</param>
        /// <param name="duration">transition duration</param>
        /// <returns></returns>
        public async UniTask SpawnChar(string charName, string expression, Vector3? location = null,
        string transition = "dissolve", float? duration = null)
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
                var layerToSpawn = MidgroundCharacterContainer;
                switch (npc.spawnLayer)
                {
                    case ImageLayer.Foreground:
                        layerToSpawn = ForegroundCharacterContainer;
                        break;
                    case ImageLayer.Midground:
                        layerToSpawn = MidgroundCharacterContainer;
                        break;
                }
                Debug.LogFormat("layer to spawn: {0}", layerToSpawn);

                if (npc.UseAddressables)
                {
                    AssetReference assetToLoad = new AssetReference();


                    assetToLoad = npc.charGameObjectAssetRef;


                    charSprite = await AssetRefLoader.Instance.LoadAsset(npc.charGameObjectAssetRef, layerToSpawn.transform);

                }
                else
                {
                    charSprite = GameObject.Instantiate(npc.charGameObject, layerToSpawn.transform);
                }
                charSprite.SetActive(false);
                // Debug.Break();
                charactersOnScreen[charName.TrimStart(null).TrimEnd(null)] = charSprite;
            }
            else
            {
                Debug.Log("char already on screen, reusing!");
                charSprite = charactersOnScreen[charName.TrimStart(null).TrimEnd(null)];
            }
            // Debug.Break();
            var spriteWrapperController = charSprite.GetComponentInChildren<SpriteWrapperController>();
            spriteWrapperController.ExpressionChange(expression, 0).Forget();

            if (location == null)
            {
                location = npc.defaultSpawnPosition;
            }
            // move char to location
            charSprite.transform.localPosition = (Vector3)location;

            // Debug.Break();
            // Debug.LogFormat("location: {0}",location);

            // OnscreenSpriteCamera.Render();

            // set both new and old exps and temp
            // handler.transform.GetChild(0).GetComponentInChildren<Image>().sprite = npc.Expressions[expression];
            // handler.transform.GetChild(1).GetComponentInChildren<Image>().sprite = npc.Expressions[expression];
            // Debug.Break();

            // TECHDEBT: sidepanel is basically a portrait so we want instant show
            // if ((bool)DialogueSystemManager.Instance.Story.variablesState["sidepanel"])
            // {
            //     duration = 0;
            // }
            await ShowChar(charName, location, transition: transition, duration: (float)duration);
            // OnscreenSpriteCamera.Render();

        }
        public async UniTask ShowChar(string charName, Vector3? location, string transition = "dissolve", float duration = 1f)
        {
            // TECHDEBT: need to be able to show a character with an arbitrary transition!
            // Debug.Log("lol am i ever called");
            var go = charactersOnScreen[charName];
            var animationComplete = false;
            if (location != null)
            {
                await Easing.Create<InQuart>(to: (Vector3)location, duration: duration).ToLocalPosition(go);
                animationComplete = true;
            }
            else
            {
                animationComplete = true;
            }
            // TECHDEBT
            // if (GameManager.Instance.IsSkipping)
            // { duration = 0.002f; }
            // else
            // {
            duration = GameManager.Instance.DefaultConfig.spawnCharacterDuration;
            // }
            if (!go.activeSelf)
            {
                animationComplete = false;
                var spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
                animationTasks.Clear();

                foreach (var sr in spriteRenderers)
                {
                    animationTasks.Add(
                    Easing.Create<InCubic>(start: 0f, end: 1f, duration: duration)
                    .ToMaterialPropertyFloat(sr, "Alpha")
                    );


                    // if (sr.material.HasProperty("DoTint"))
                    // {
                    //     if (this.darkTintOn)
                    //     {
                    //         Debug.Log("turn tint on");
                    //         // 1 is true
                    //         sr.material.SetFloat("DoTint", 1);
                    //     }
                    //     else
                    //     {
                    //         Debug.Log("turn tint off");
                    //         sr.material.SetFloat("DoTint", 0);
                    //     }
                    // }


                }

                await UniTask.WhenAll(animationTasks);
                animationComplete = true;
            }
            await UniTask.WaitUntil(() => animationComplete);
            // move the gameobject to Location
            // animate showing with transition and duration
            // TECHDEBT
            // if (!GameManager.Instance.IsSkipping)
            // {
            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText));
            // }
            // else
            // {
            //     await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
            // }


        }
        [Button]
        public async UniTask FireHideChar(string charName, string transition = "fade", float? duration = null)
        {
            if (!charactersOnScreen.ContainsKey(charName))
            {
                Debug.LogWarningFormat("Character {0} isn't on screen to hide", charName);
                return;
            }
            animationTasks.Clear();
            var go = charactersOnScreen[charName];
            if (duration == null)
            {
                duration = GameManager.Instance.DefaultConfig.hideCharacterDuration;
            }
            if (GameManager.Instance.IsSkipping)
            { duration = 0; }
            // TECHDEBT:  this skip Token is not going to work as expected.
            foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>())
            {
                animationTasks.Add(
                    Easing.Create<InCubic>(start: 1f, end: 0f, duration: (float)duration)
                    .ToMaterialPropertyFloat(sr, "Alpha",skipToken:skipToken)
                );
            }
            if (GameManager.Instance.IsSkipping)
            {
                ThrowSkipToken();
            }
            // sequence.AppendCallback(() =>
            // {
            //     go.SetActive(false);
            //     // charactersOnScreen.Remove(charName);
            //     if (GameManager.Instance.IsSkipping)
            //     {
            //         SkipBGTween();
            //     }
            //     // #if PLATFORM_ANDROID
            //     // // destroy some old chars that aren't used anymore
            //     // if (CharacterLayer.transform.childCount > 2)
            //     // {
            //     foreach (var character in charactersOnScreen)
            //     {
            //         if (!character.Value.activeSelf)
            //         {
            //             charactersOnScreen.Remove(character.Key);
            //             AssetRefLoader.Instance.ReleaseAsset(character.Value);
            //         }
            //     }
            //     // }
            //     // #endif
            // });
            await UniTask.WhenAll(animationTasks);
            go.SetActive(false);
            foreach (var character in charactersOnScreen)
                {
                    if (!character.Value.activeSelf)
                    {
                        charactersOnScreen.Remove(character.Key);
                        AssetRefLoader.Instance.ReleaseAsset(character.Value);
                    }
                }

        }
        // TODO: figure out how we want to do the awaiting for hide char...........
        public void HideChar(string charName, string transition = "fade", float? duration = null)
        {
            FireHideChar(charName,transition,duration).Forget();
        }
        [Button]
        public async UniTask HideAllChar(float? duration = null)
        {
            animationTasks.Clear();
            var charsOnScreen = new List<string>(charactersOnScreen.Keys);
            for (int i = 0; i < charsOnScreen.Count; i++)
            {
                FireHideChar(charsOnScreen[i], duration: duration).Forget();
            }

        }
        private GameObject char_;
        /// <summary>
        /// Change the expression of a specific character!
        /// Returns UniTask<bool> because we want to check whether we needed to show the portrait.
        /// </summary>
        /// <param name="charName">Character to change the expression of</param>
        /// <param name="expression">The new expression</param>
        /// <param name="duration">The transition duration</param>
        /// <returns></returns>
        public async UniTask<bool> ExpressionChange(string charName, string expression, float? duration = .35f)
        {
            Debug.LogFormat("expchange args for char {0}: {1}", charName, expression);
            // OnscreenSpriteCamera.Render();

            //exp sandwich is currExp > newExp 
            // set new exp, fade out currexp, set currexp=newexp, 
            var npc = (NPC_SO)DialogueSystemManager.Instance.GetNPC(charName);

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
                await char_.GetComponentInChildren<SpriteWrapperController>().ExpressionChange(expression);
            }

            if (!GameManager.Instance.IsSkipping)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText));
            }
            else
            {
                // await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
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
            CurrentBGCamera.transform.position = shot.position;
            CurrentBGCamera.transform.rotation = Quaternion.Euler(shot.rotation);
            CurrentBGCamera.orthographicSize = shot.size;
            await ShowBG(shot.bgName);
        }
    }
}