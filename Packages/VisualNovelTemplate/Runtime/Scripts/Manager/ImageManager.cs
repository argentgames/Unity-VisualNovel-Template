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
        GameObject NewBackgroundContainer1, NewBackgroundContainer2;
        [BoxGroup("Image containers")]
        [InfoBox("Parents that hold spawned characters and overlays.")]
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
        public Camera CurrentBGCamera, MidgroundCharactersCamera, ForegroundCharactersCamera, NewBGCamera1, NewBGCamera2;

        private MaterialPropertyBlock _propBlock;
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
        [Tooltip("We automatically load all shots in Resources/shotsPath, but you can also manually add shots here.")]
        List<Shot> shots = new List<Shot>();
        [SerializeField]
        string shotsPath = "Shots";


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

            newBGContainerPosition = NewBackgroundContainer1.transform.position;

            var stuff = Resources.LoadAll<Shot>(shotsPath);
            foreach (var shot in stuff)
            {
                // Debug.Log(shot);
                cameraShots[shot.bgName] = shot;
            }
            foreach (var shot in shots)
            {
                // Debug.Log(shot);
                cameraShots[shot.bgName] = shot;
            }
            animationTasks.Clear();
            _propBlock = new MaterialPropertyBlock();
            backgroundProjectedImageRenderer = BackgroundProjectedImage.GetComponentInChildren<Renderer>();
            backgroundProjectedImageRenderer.GetPropertyBlock(_propBlock);
            Debug.LogFormat("propblock {0}",_propBlock.GetFloat("TransitionAmount"));
            _propBlock.SetFloat("TransitionAmount", 1);
            Debug.LogFormat("propblock after set float to 1 {0}",_propBlock.GetFloat("TransitionAmount"));
            backgroundProjectedImageRenderer.SetPropertyBlock(_propBlock);
            backgroundProjectedImageRenderer.GetPropertyBlock(_propBlock);
            Debug.LogFormat("propblock after assign mbp to renderer {0}",_propBlock.GetFloat("TransitionAmount"));
            CreateCancellationToken();
            CreateSkipToken();
        }
        public void CreateSkipToken()
        {
            this.skipTokenSource = new SkipTokenSource();
            skipToken = skipTokenSource.Token;
        }
        public void ThrowSkipToken()
        {
            Debug.Log("throwing skip token from IM");
            skipTokenSource.Skip();
            CreateSkipToken();

            foreach (var character in charactersOnScreen.Values)
            {
                var swc = character.GetComponentInChildren<SpriteWrapperController>();
                if (swc != null)
                {
                    swc.ThrowSkipToken();
                }
            }
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
        public void UnregisterCharacter(string charName)
        {
            if (charactersOnScreen.ContainsKey(charName))
            {
                charactersOnScreen.Remove(charName);
            }
        }
        public void SetAllCharactersOnScreenActive()
        {
            foreach (var v in charactersOnScreen.Values)
            {
                v.SetActive(true);
            }
        }
        public void RegisterCharacter(string charName, GameObject go)
        {
            Debug.Log("registering character " + charName);
            charactersOnScreen[charName] = go;
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
                // Debug.LogFormat("which camera {0}, what position {1}", camera.name, p);
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
            SetCameraShot(NewBGCamera1, pos, rotation, size);
        }

        int currentNewBGSet = 0;

        /// <summary>
        /// Show a new background with a given transition such as a dissolve or wipe.
        /// TECHDEBT: in future we should be able to supply an arbitrary transition!!! not just wipe/dissolve!!!
        /// </summary>
        /// <param name="bgName"></param>
        /// <param name="transition"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        // [Button]
        public async UniTask ShowBG(string bgName, string transition = "w9", float? duration = null)
        {
            if (bgName == "")
            {
                Debug.LogError("can't showBG EMPTY STRING");
                return;
            }

            if (duration == null)
            {
                duration = (float) GameManager.Instance.DefaultConfig.bgTransitionDuration;
            }

            // CreateSkipToken();

            // destroy the previous bg that was spawned, but it may be under newbg1 or newbg2.
            // do we even still need currentbg........ or the first time we spawn anything we just
            // spawn it to both bgs?

            // spawn BG
            Debug.Log("trying to spawn bg: " + bgName);
            currentCameraShot = bgName;

            // foreach (var shotName in cameraShots.Keys)
            // {
            //     Debug.LogFormat("shot name available: {0}", shotName);
            // }

            var shot = cameraShots[bgName];
            GameObject newBGGO;
            // which newbgcontainer do we want to spawn under?
            GameObject newBGContainer;
            RenderTexture newBGRT;
            if (currentNewBGSet == 2)
            {
                Debug.Log("using newbg1");
                newBGContainer = NewBackgroundContainer1;
                newBGRT = NewBGCamera1.activeTexture;
                currentNewBGSet = 1;
            }
            else
            {
                Debug.Log("using newbg2");
                newBGContainer = NewBackgroundContainer2;
                newBGRT = NewBGCamera2.activeTexture;
                currentNewBGSet = 2;
            }

            // instantiate the new background
            if (shot.UseAddressables)
            {
                var bgAsset = shot.bgAssetReference;
                // first spawn the prefab
                newBGGO = await AssetRefLoader.Instance.LoadAsset(bgAsset, newBGContainer.transform);
            }
            else
            {
                newBGGO = GameObject.Instantiate(shot.bgPrefab, newBGContainer.transform);
            }

            // Debug.Break();


            Wipe_SO transitionWipe;

            backgroundProjectedImageRenderer.GetPropertyBlock(_propBlock);

            // TODO: figure out how to specify Ease with AnimeTask
            // Ease ease;
            if (transition != "fade")
            {
                transitionWipe = GameManager.Instance.GetWipe(transition);
                //  ease = transitionWipe.ease;

                _propBlock.SetTexture("Wipe", transitionWipe.wipePrefab);
            }
            else
            {
                // ease = GameManager.Instance.DefaultConfig.expressionTransitionEase;
            }

            _propBlock.SetFloat("TransitionAmount", 0);
            _propBlock.SetTexture("NewTex", newBGRT);


            SetNewBGCameraShot(shot.position, shot.rotation, shot.size);

            animationTasks.Clear();

            // if bg shot is black, turn off the noise and particle system
            var noiseOpacity = GameManager.Instance.DefaultConfig.bgNoiseOpacity;
            // TECHDEBT: random hardcoded noise time/duration multiplication
            float noiseStartTime = .8f * (float)duration;
            float noiseDuration = .2f * (float)duration;
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
                duration = 0.001f;
                noiseStartTime = 0.001f;
                noiseDuration = 0.001f;
            }

            // set up the animation sequencer
            // create a sequence to simultaneously play the transition wipe and turn on/off noise

            // Debug.Log("time to add animations to animationTask");

            newBGGO.SetActive(true);

            if (transition == "fade")
            {
// 1 is true
                _propBlock.SetFloat("_DoAlpha", 1);
                
            backgroundProjectedImageRenderer.SetPropertyBlock(_propBlock);
                await Easing.Create<InCubic>(start: 0f, end: 1f, duration: (float)duration)
                .ToMaterialPropertyFloat(backgroundProjectedImageRenderer, "TransitionAmount", skipToken: GameManager.Instance.SkipToken);

            }
            else
            {
                // 1 is true
                _propBlock.SetFloat("_DoAlpha", 0);
                Debug.Log("now doing a wipe animation");

            backgroundProjectedImageRenderer.SetPropertyBlock(_propBlock);
                await Easing.Create<InCubic>(start: 0f, end: 1f, duration: (float)duration)
                .ToMaterialPropertyFloat(backgroundProjectedImageRenderer, "TransitionAmount", skipToken: GameManager.Instance.SkipToken)
                                ;
                Debug.Log("done doing wipe   animation");
            }


            // sequence.Join(newBGMaterial.DOFloat(1, "TransitionAmount", duration).SetEase(ease).From(0));
            // TECHDEBT: add noise animation delay later
            // sequence.Insert(noiseStartTime, newBGMaterial.DOFloat(noiseOpacity, "NoiseOpacity", noiseDuration));
            // sequence.InsertCallback(noiseStartTime, () => particleSystemHolder.SetActive(toggleParticleSystem));


            //     // EXTRA TURN OFF PARTICLE SYSTEM??
            //     particleSystemHolder.SetActive(toggleParticleSystem);


            // });

            // if we are skipping, automatically run the entire sequence
            // idk if this actually works or only runs through the first tween??
            // if (GameManager.Instance.IsSkipping)
            // {
            //     ThrowSkipToken();
            // }

            // Debug.Log("done running show bg animation tasks");

            // clean up bg mess

            // set currBGCamera shot to same shot as newBGCamera
            SetCurrentBGCameraShot(shot.position, shot.rotation, shot.size);

            if (currentNewBGSet == 1)
            {
                Debug.Log("destroying newbg2 objects");
                for (int idx = 0; idx < NewBackgroundContainer2.transform.childCount; idx++)
                {
                    Destroy(NewBackgroundContainer2.transform.GetChild(idx).gameObject);
                }
            }
            else
            {
                Debug.Log("destroying newbg1 objects");
                for (int idx = 0; idx < NewBackgroundContainer1.transform.childCount; idx++)
                {
                    Destroy(NewBackgroundContainer1.transform.GetChild(idx).gameObject);
                }
            }


            // reset transitionAmount 
            backgroundProjectedImageRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat("TransitionAmount", 0);
            _propBlock.SetTexture("_MainTex", newBGRT);
            // 1 is true
                _propBlock.SetFloat("_DoAlpha", 0);

            backgroundProjectedImageRenderer.SetPropertyBlock(_propBlock);



            Debug.Log("done running showbg");



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
                GameManager.Instance.ThrowSkipToken();
            }
        }
        public void SkipBGTween()
        {
            GameManager.Instance.ThrowSkipToken();
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
        // [Button]
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
            if (expression == "")
            {
                expression = npc.defaultExpression;
            }
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
            await UniTask.WaitUntil(() => spriteWrapperController.InitComplete);
            spriteWrapperController.ExpressionChange(expression, 0).Forget();

            if (location == null)
            {
                location = spriteWrapperController.defaultSpawnPosition;
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
                await Easing.Create<InQuart>(to: (Vector3)location, duration: duration)
                    .ToLocalPosition(go,skipToken:GameManager.Instance.SkipToken);
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
                    .ToMaterialPropertyFloat(sr, "Alpha",skipToken:GameManager.Instance.SkipToken)
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
                go.SetActive(true);
                await UniTask.WhenAll(animationTasks);
                animationComplete = true;
            }
            await UniTask.WaitUntil(() => animationComplete);
            // move the gameobject to Location
            // animate showing with transition and duration
            // TECHDEBT
            // if (!GameManager.Instance.IsSkipping)
            // {
            await UniTask.Delay(TimeSpan.FromSeconds(GameManager.Instance.DefaultConfig.delayBeforeShowText),cancellationToken:ct);
            // }
            // else
            // {
            //     await UniTask.Delay(TimeSpan.FromSeconds(.002f), cancellationToken: this.ct);
            // }


        }
        // [Button]
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
                    .ToMaterialPropertyFloat(sr, "Alpha",skipToken:GameManager.Instance.SkipToken)
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
            charactersOnScreen.Remove(charName);

            // TODO: add in asset ref loader removal
            // AssetRefLoader.Instance.ReleaseAsset(go);
            Destroy(go);



        }
        // TODO: figure out how we want to do the awaiting for hide char...........
        public void HideChar(string charName, string transition = "fade", float? duration = null)
        {
            FireHideChar(charName, transition, duration).Forget();
        }
        // [Button]
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