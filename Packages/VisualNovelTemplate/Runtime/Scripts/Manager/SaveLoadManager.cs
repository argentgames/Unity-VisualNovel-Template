using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using Sirenix.Serialization;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
public struct SpriteSaveData
{
    public string expressionImageName;
    public Vector3 position;
    public string activeTintColor;
}
namespace com.argentgames.visualnoveltemplate
{



    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; set; }

        public SaveData currentSave;


        // each saveFile is a json
        public Dictionary<string, SaveData> saveFiles = new Dictionary<string, SaveData>();
        [SerializeField]
        public string saveFileNamePrefix = "gameSave_";
        public string autoSaveNamePrefix = "autosave";
        [SerializeField]
        string saveDir = "Saves";
        [SerializeField]
        bool jsonFormat = true;
        public string extension = ".json";

        public bool DoneLoadingSaves = false;
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

            if (!jsonFormat)
            {
                extension = ".save";
            }


            if (!Directory.Exists(CreateSavePath("Saves")))
            {
                Debug.Log("saves directory doesnt exist, creating now");
                Directory.CreateDirectory(CreateSavePath("Saves"));
            }
            // await LoadSaveFiles();

#if PLATFORM_ANDROID
        // auto save game if we are ingame, every 5? minutes
        InvokeRepeating("AutoSave", 3f,30f);
#endif
        }

        async UniTask AutoSave()
        {
            // while (true)
            // {
            if (SceneManager.GetActiveScene().name == "Ingame")
            {

                await GameManager.Instance.TakeScreenshot();
                string date = "Autosave: " + System.DateTime.Now.ToString("dddd, MMM dd yyyy, hh:mm");

                var save = new SaveData(DialogueSystemManager.Instance.Story.state.ToJson(),
                GameManager.Instance.currentScreenshot, date);

                save.spriteSaveDatas = ImageManager.Instance.GetAllCharacterOnScreenSaveData();

                // save.currentAmbient1 = AudioManager.Instance.currentAmbient1;
                // save.currentAmbient2 = AudioManager.Instance.currentAmbient2;
                // save.currentAmbient3 = AudioManager.Instance.currentAmbient3;
                save.currentMusic = AudioManager.Instance.GetCurrentPlayingMusic();

                save.currentBGCameraPosition = ImageManager.Instance.CurrentBGCamera.transform.position;
                save.currentBGCameraRotation = ImageManager.Instance.CurrentBGCamera.transform.eulerAngles;
                ;
                save.currentBGSize = ImageManager.Instance.CurrentBGCamera.orthographicSize;
                save.currentShot = ImageManager.Instance.CurrentCameraShot;

                save.dialogueHistory = DialogueSystemManager.Instance.currentSessionDialogueHistory;
                // save.isTinted = ImageManager.Instance.darkTintOn;
                var filePath = autoSaveNamePrefix + extension;
                SaveGame(SaveLoadManager.Instance.CreateSavePath("Saves/" + filePath), save);
                this.saveFiles[filePath] = save;
                Debug.Log("auto saving now...");
            }
            // }    
        }

        public string CreateSavePath(string subPath)

        {
            // var s = "";
            // #if PLATFORM_ANDROID && !UNITY_EDITOR
            // Debug.Log("we are on android");
            // s = subPath;
            // #else
            // s = Application.persistentDataPath + "/" + subPath;
            // #endif

            var s = Application.persistentDataPath + "/" + subPath;
            Debug.Log("our save path is: " + s);
            return s;
        }

        public async UniTask LoadSaveFiles()
        {
            try
            {
                var path = CreateSavePath("Saves");
                Debug.Log("Load saves from: " + path);
                string[] fileArray = Directory.GetFiles(path, "*" + extension);
                var taskList = new List<UniTask>();
                for (int i = 0; i < fileArray.Length; i++)
                {
                    Debug.Log(fileArray[i]);
                    taskList.Add(LoadSaveFile(fileArray[i]));


                }

                await UniTask.WhenAll(taskList);
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("failed to lod saves??? {0}", e);
            }

            DoneLoadingSaves = true;

        }
        async UniTask LoadSaveFile(string filePath)
        {
            Debug.Log("reading save file..." + filePath);

            // byte[] bytes = File.ReadAllBytes(filePath);

            var save = new SaveData().Load(filePath);// SerializationUtility.DeserializeValue<SaveData>(bytes, DataFormat.JSON);

            await UniTask.Yield();
            await UniTask.Yield();

            // var saveData = File.ReadAllText(fileArray[i]);
            // SaveData save = JsonUtility.FromJson<SaveData>(saveData);
            saveFiles[Path.GetFileName(filePath)] = save;
        }
        public async UniTaskVoid LoadGame(string filePath)
        {
            Debug.Log("loading game from: " + filePath);
            this.currentSave = GetSaveData(filePath);

            GameManager.Instance.SetSkipping(false);
            GameManager.Instance.SetAuto(false);
            if (DialogueSystemManager.Instance != null)
            {
                Debug.Log("please run cancellation on ds");
                DialogueSystemManager.Instance.RunCancellationToken();
            }
            AudioManager.Instance.StopMusic(1);
            AudioManager.Instance.StopAllAmbient(1);
            await SceneTransitionManager.Instance.LoadScene("Ingame", 0, doFadeIn: false);
            try
            {
                MenuManager.Instance.CloseAllMenus();
            }
            catch
            {
                Debug.Log("dont need to close settings when loading game?");
            }

        }
        public void SaveGame(string filePath, SaveData save)
        {
            Debug.Log("saving game to: " + filePath);
            save.Save(filePath);
            // byte[] bytes = SerializationUtility.SerializeValue(save, DataFormat.JSON);
            // File.WriteAllBytes(filePath, bytes);

            // string saveToJson = JsonUtility.ToJson(save, true);
            // File.WriteAllText(filePath, saveToJson);




        }
        public void SaveGame(string fileName = "autosave")
        {
            Debug.Log("run save game from slm please");
            string filePath = saveFileNamePrefix + fileName + extension;
            string date = System.DateTime.Now.ToString("dddd, MMM dd yyyy, hh:mm", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

            var save = new SaveData(DialogueSystemManager.Instance.Story.state.ToJson(),
            GameManager.Instance.currentScreenshot, date);

            save.spriteSaveDatas = ImageManager.Instance.GetAllCharacterOnScreenSaveData();

            save.currentMusic = AudioManager.Instance.GetCurrentPlayingMusic();
            save.currentAmbients = AudioManager.Instance.GetCurrentPlayingAmbients();

            save.currentBGCameraPosition = ImageManager.Instance.CurrentBGCamera.transform.position;
            save.currentBGCameraRotation = ImageManager.Instance.CurrentBGCamera.transform.eulerAngles;
            
            save.currentBGSize = ImageManager.Instance.CurrentBGCamera.orthographicSize;
            save.currentShot = ImageManager.Instance.CurrentCameraShot;

            save.dialogueHistory = DialogueSystemManager.Instance.currentSessionDialogueHistory;

            saveFiles[filePath] = save;

            byte[] bytes = SerializationUtility.SerializeValue(save, DataFormat.JSON);
            File.WriteAllBytes(CreateSavePath(saveDir + "/" + filePath), bytes);
        }
        public void SaveSettings()
        {
            Debug.Log("save path to save settings to is: " + CreateSavePath("settings.json"));
            Debug.Log("saving settings now");
            byte[] bytes = SerializationUtility.SerializeValue(GameManager.Instance.Settings.Save(), DataFormat.JSON);
            File.WriteAllBytes(CreateSavePath("settings.json"), bytes);
        }
        public void LoadSettings()
        {
            // if settings file doesn't exist, do nothing
            var savePath = CreateSavePath("settings.json");
            Debug.Log("save path to check for loading settings is: " + savePath);
            Debug.Log("does settings.json already exist?: " + File.Exists(savePath).ToString());
            if (!File.Exists(savePath))
            {
                Debug.Log("file doesnt exist, resetting settings to default");
                GameManager.Instance.Settings.ResetDefaults();
            }
            else
            {
                Debug.Log("loading settings");
                byte[] bytes = File.ReadAllBytes(savePath);
                var settings = SerializationUtility.DeserializeValue<SettingsSaveData>(bytes, DataFormat.JSON);
                GameManager.Instance.Settings.Load(settings);
            }

        }

        private Texture2D ConvertToTextureAndLoad(string path)
        {
            //Read
            byte[] bytes = File.ReadAllBytes(path);
            //Convert image to texture
            Texture2D loadTexture = new Texture2D(2, 2);
            loadTexture.LoadImage(bytes);
            //Convert textures to sprites
            return loadTexture;
            // _paintImage.sprite = Sprite.Create(loadTexture, new Rect(0, 0, loadTexture.width, loadTexture.height), Vector2.zero);
        }

        public SaveData GetSaveData(string saveFileName)
        {
            string filePath = saveFileNamePrefix + saveFileName + extension;
            return saveFiles[filePath];
        }
        public bool SaveExists(string saveFileName)
        {
            foreach (var k in saveFiles.Keys)
            {
                Debug.LogFormat("save file exists: {0}",k);
            }
            string filePath = saveFileNamePrefix + saveFileName + extension;
            if (saveFiles.ContainsKey(filePath))
            {
                return true;
            }
            return false;
        }


    }
}