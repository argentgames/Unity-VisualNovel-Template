using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using NaughtyAttributes;

using AnimeTask;
namespace com.argentgames.visualnoveltemplate
{


    /// <summary>
    /// The big brain in the game~!
    /// Holds all the config and defaults.
    /// Controls game state, e.g. PAUSE, AUTODialogue, SKIPDialogue
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; set; }

        [Tooltip("Default config values such as transition rate between backgrounds, transition texture, text unwrap speed, etc.")]
        [SerializeField]
        [Required]
        private DefaultConfig defaultConfig;
        public DefaultConfig DefaultConfig { get { return defaultConfig; } }

        [SerializeField]
        [Tooltip("Persistent game settings across all game sessions such as volume and screen resolution.")]
        [Required]
        private Settings_SO settings;
        public Settings_SO Settings { get { return settings; } }

        [SerializeField]
        [Tooltip("Persistent game data such as all texts seen, routes currently unlocked, and cgs current unlocked.")]
        [Required]
        private PersistentGameData_SO persistentGameData;
        public PersistentGameData_SO PersistentGameData { get { return persistentGameData; } }

        [SerializeField]
        [Tooltip("Texts that are reused throughout the game, such as quote characters and menu texts.")]
        private GenericTexts_SO genericTexts;
        public GenericTexts_SO GenericTexts { get { return genericTexts; } }

        [SerializeField]
        [Tooltip("All characters that speak in the game.")]
        [Required]
        private NPCBank_SO characterDatabase;

        /// <summary>
        /// Mapping for transition wipes, such as screen dissolve or ink blot.
        /// </summary>
        /// <typeparam name="string">name by which we refer to the wipe elsewhere</typeparam>
        /// <typeparam name="Wipe_SO"></typeparam>
        /// <returns></returns>
        Dictionary<string, Wipe_SO> wipes = new Dictionary<string, Wipe_SO>();



        // public Camera bgCamera, spriteCamera;

        public BoolReactiveProperty isAuto = new BoolReactiveProperty(false);
        public bool IsAuto { get { return isAuto.Value; } }
        public BoolReactiveProperty isSkipping = new BoolReactiveProperty(false);
        public bool IsSkipping { get { return isSkipping.Value; } }
        public bool isGamePaused = false;
        public Texture2D currentScreenshot;

        /// <summary>
        /// Globally used skip token. Mainly used for skipping animations and transitions.
        /// </summary>
        /// <returns></returns>
        SkipTokenSource skipTokenSource = new SkipTokenSource();
        SkipToken skipToken;
        public SkipToken SkipToken => skipToken;
        /// <summary>
        /// This assumes there is one qmenu in the game, usually only found in the Ingame scene.
        /// Does not have to be used!!! You can have individual qmenus attached to dialogue windows.
        /// </summary>
        public QMenu globalQmenu;
        public void CreateSkipToken()
        {
            this.skipTokenSource = new SkipTokenSource();
            skipToken = skipTokenSource.Token;
        }
        public void ThrowSkipToken()
        {
            Debug.Log("throwing gm skip token");
            skipTokenSource.Skip();
            CreateSkipToken();
        }

        public void SetSkipping(bool val)
        {
            // Debug.Log("someone set skipping to: " + val.ToString());
            isSkipping.Value = val;
        }
        public void SetAuto(bool val)
        {
            // Debug.Log("someone set auto to: " + val.ToString());
            isAuto.Value = val;

        }
        async UniTaskVoid Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }

            var Wipes = Resources.LoadAll<Wipe_SO>("wipes");
            for (int i = 0; i < Wipes.Length; i++)
            {
                var w = Wipes[i];
                wipes.Add(w.internalName, w);
            }


            await UniTask.WaitUntil(() => SaveLoadManager.Instance != null);
            // load settings
            SaveLoadManager.Instance.LoadSettings();

#if PLATFORM_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

            this.currentScreenshot = defaultConfig.defaultNullTexture;

        }

        public Wipe_SO GetWipe(string wipeName)
        {
            try
            {
                return wipes[wipeName];
            }
            catch
            {
                Debug.LogErrorFormat("Unable to find wipe [{0}]", wipeName);
                return null;
            }
        }

        public NPC_SO GetNPC(string npcName)
        {
            return characterDatabase.GetNPC(npcName);

        }


        // TECHDEBT: clear out the currentScreenshot so it's not always held in memory the entire game
        public async UniTask TakeScreenshot()
        {
// byte[] bytes = tex.EncodeToPNG();
// Object.Destroy(tex);

            try
            {
//                 string sspath = "";
// #if PLATFORM_ANDROID
//             sspath = "current.PNG";
// #else
//                 SaveLoadManager.Instance.CreateSavePath("current.PNG");
// #endif

//                 var actualSavedFP = SaveLoadManager.Instance.CreateSavePath("current.PNG");

//                 Debug.Log("trying to save screenshot to " + actualSavedFP);
//                 // if (File.Exists(actualSavedFP))
//                 // {
//                 //     File.Delete(actualSavedFP);
//                 //     Console.WriteLine("The file exists.");
//                 // }
//                 ScreenCapture.CaptureScreenshot(actualSavedFP);
//                 await UniTask.WaitUntil(() => File.Exists(actualSavedFP));
                //Read
// #if PLATFORM_ANDROID
//             actualSavedFP = "file://" + actualSavedFP;
// #endif
                // var bytes = (await UnityWebRequest.Get(actualSavedFP).SendWebRequest()).downloadHandler.data;
                // // byte[] bytes = File.ReadAllBytes(sspath);
                // //Convert image to texture
                // Texture2D loadTexture = new Texture2D(2, 2);
                // loadTexture.LoadImage(bytes);
                // // resize texture because we only need a tiny screenshot 
                // currentScreenshot = loadTexture;


                // int width = Screen.currentResolution.width;
                // int height = Screen.currentResolution.height;
                // var scaleFactorWidth = width / (float)419;
                // var scalefactorHeight = height / (float)213;
                // var scaleFactor = scaleFactorWidth > scalefactorHeight ? scaleFactorWidth : scalefactorHeight;
                // currentScreenshot = Resize(currentScreenshot, 419, 213);


await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                currentScreenshot = new Texture2D(Screen.width, Screen.height,TextureFormat.RGB24, true);

await UniTask.WaitForEndOfFrame(this);

currentScreenshot.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);
currentScreenshot.LoadRawTextureData(currentScreenshot.GetRawTextureData());
currentScreenshot.Apply();
currentScreenshot = Resize(currentScreenshot, 419, 213);
// byte[] bytes = tex.EncodeToPNG();
// Object.Destroy(tex);

            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("couldn't save screenshot, using default null textureee, exception {0}", e);
                currentScreenshot = DefaultConfig.defaultNullTexture;
            }

// #if PLATFORM_ANDROID
//         await UniTask.WaitWhile(() => currentScreenshot == null);
// #endif

        }

        Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }

        [Button]
        public void PauseGame()
        {
            // the only thing we need to do 
            isGamePaused = true;
            Time.timeScale = 0;
            if (SceneManager.GetActiveScene().name == "Ingame")
            {
                DialogueSystemManager.Instance.DialogueUIManager.PauseTypewriter();
            }
        }
        [Button]
        public void ResumeGame()
        {
            isGamePaused = false;
            Time.timeScale = 1;
            if (SceneManager.GetActiveScene().name == "Ingame")
            {
                if (DialogueSystemManager.Instance.DialogueUIManager.IsDisplayingLine)
                {
                    DialogueSystemManager.Instance.DialogueUIManager.ContinueTypewriter();

                }

            }
        }

        void OnApplicationQuit()
        {
            Debug.Log("is on application quit ever called");
            // SaveLoadManager.Instance.SaveSettings();

            string sspath = "";
#if PLATFORM_ANDROID
        sspath = "current.PNG";
#else
            SaveLoadManager.Instance.CreateSavePath("current.PNG");
#endif

            if (File.Exists(sspath))
            {
                File.Delete(sspath);
            }
        }
        void OnApplicationPause()
        {
            Debug.Log("is on apllication pause ever called");
            // SaveLoadManager.Instance.SaveSettings();

            string sspath = "";
#if PLATFORM_ANDROID
        sspath = "current.PNG";
#else
            SaveLoadManager.Instance.CreateSavePath("current.PNG");
#endif

            if (File.Exists(sspath))
            {
                File.Delete(sspath);
            }
        }
    }
}