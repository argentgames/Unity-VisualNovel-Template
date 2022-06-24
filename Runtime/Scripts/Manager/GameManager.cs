using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
public class GameManager : SerializedMonoBehaviour
{
    [SerializeField]
    private GlobalDefinitions globalDefinitions;
    [SerializeField]
    private SettingsData_SO settings;
    [SerializeField]
    private GenericTexts_SO genericTexts;
    public GenericTexts_SO GenericTexts { get { return genericTexts; } }
    public static GameManager Instance { get; set; }
    [SerializeField]
    private NPCBank_SO characterDatabase;
    public Dictionary<NPC_NAME, NPC_SO> NamedCharacterDatabase;
    private Dictionary<string, NPC_SO> _characterDatabase;
    public SettingsData_SO Settings { get { return settings; } }
    public GlobalDefinitions GlobalDefinitions { get { return globalDefinitions; } }

    // public Camera bgCamera, spriteCamera;

    public BoolReactiveProperty isAuto = new BoolReactiveProperty(false);
    public bool IsAuto { get { return isAuto.Value; } }
    public BoolReactiveProperty isSkipping = new BoolReactiveProperty(false);
    public bool IsSkipping { get { return isSkipping.Value; } }
    public bool isGamePaused = false;
    public Texture2D currentScreenshot;
    public IngameHUDPresenter ingameHUDPresenter;
    public bool IsExtrasOpen = false;

    public void SetSkipping(bool val)
    {
        isSkipping.Value = val;
        if (ingameHUDPresenter != null)
        {
#if !PLATFORM_ANDROID && !UNITY_ANDROID
            ingameHUDPresenter.skip.SetIsOnWithoutNotify(val);
            ingameHUDPresenter.skip.gameObject.GetComponent<ToggleExtension>().TextColorSwapOnSelect(val);
#else
            ingameHUDPresenter.skip.SetIsOnWithoutNotify(val);
            ingameHUDPresenter.skip.gameObject.GetComponent<ToggleExtension>().ImageColorSwapOnSelect(val);
#endif
        }
    }
    public void SetAuto(bool val)
    {
        isAuto.Value = val;
        if (ingameHUDPresenter != null)
        {
#if !PLATFORM_ANDROID && !UNITY_ANDROID
            ingameHUDPresenter.auto.SetIsOnWithoutNotify(val);
            ingameHUDPresenter.auto.gameObject.GetComponent<ToggleExtension>().TextColorSwapOnSelect(val);
#else
            ingameHUDPresenter.auto.SetIsOnWithoutNotify(val);
            ingameHUDPresenter.auto.gameObject.GetComponent<ToggleExtension>().ImageColorSwapOnSelect(val);

#endif
        }
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
        NamedCharacterDatabase = characterDatabase.namedNPCDatabase;


        _characterDatabase = characterDatabase.allNPCDatabase;
        await UniTask.WaitUntil(() => SaveLoadManager.Instance != null);
        // load settings
        SaveLoadManager.Instance.LoadSettings();

#if PLATFORM_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

        this.currentScreenshot = globalDefinitions.defaultNullTexture;

    }


    public NPC_SO GetNPC(string npcName)
    {
        try
        {
            return _characterDatabase[npcName];
        }
        catch
        {
            Debug.LogWarningFormat("Character database does not contain npc: {0}", npcName);
            return _characterDatabase["narrator"];
        }

    }


    public async UniTask TakeScreenshot()
    {
        currentScreenshot = null;
        try
        {
            string sspath = "";
#if PLATFORM_ANDROID
            sspath = "current.PNG";
#else
            SaveLoadManager.Instance.CreateSavePath("current.PNG");
#endif

            var actualSavedFP = SaveLoadManager.Instance.CreateSavePath("current.PNG");

            Debug.Log("trying to save screenshot to " + sspath);
            // if (File.Exists(actualSavedFP))
            // {
            //     File.Delete(actualSavedFP);
            //     Console.WriteLine("The file exists.");
            // }
            ScreenCapture.CaptureScreenshot(sspath);
            await UniTask.WaitUntil(() => File.Exists(actualSavedFP));
            //Read
#if PLATFORM_ANDROID
            actualSavedFP = "file://" + actualSavedFP;
#endif
            var bytes = (await UnityWebRequest.Get(actualSavedFP).SendWebRequest()).downloadHandler.data;
            // byte[] bytes = File.ReadAllBytes(sspath);
            //Convert image to texture
            Texture2D loadTexture = new Texture2D(2, 2);
            loadTexture.LoadImage(bytes);
            // resize texture because we only need a tiny screenshot 
            currentScreenshot = loadTexture;


            int width = Screen.currentResolution.width;
            int height = Screen.currentResolution.height;
            var scaleFactorWidth = width / (float)419;
            var scalefactorHeight = height / (float)213;
            var scaleFactor = scaleFactorWidth > scalefactorHeight ? scaleFactorWidth : scalefactorHeight;
            currentScreenshot = Resize(currentScreenshot, 419, 213);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("couldn't save screenshot, using default null textureee, exception {0}",e);
            currentScreenshot = GlobalDefinitions.defaultNullTexture;
        }

#if PLATFORM_ANDROID
        await UniTask.WaitWhile(() => currentScreenshot == null);
#endif

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
            DialogueSystem.Instance.dialogueUIManager.PauseTypewriter();
        }
    }
    [Button]
    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1;
        if (SceneManager.GetActiveScene().name == "Ingame")
        {
            if (DialogueSystem.Instance.dialogueUIManager.IsDisplayingLine)
            {
                DialogueSystem.Instance.dialogueUIManager.ContinueTypewriter();

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
