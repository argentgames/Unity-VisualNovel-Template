using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
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
    public class SaveLoadManager : SerializedMonoBehaviour
    {
        public static SaveLoadManager Instance { get; set; }

        public SaveData currentSave;

        // each saveFile is a json
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="string">base fileIndex name without any extensions or prefixes. Not full path!!!</typeparam>
        /// <typeparam name="SaveData"></typeparam>
        /// <returns></returns>
        [SerializeField]
        public Dictionary<string, SaveData> saveFiles = new Dictionary<string, SaveData>();

        [SerializeField]
        public string saveFileNamePrefix = "gameSave_";
        public string autoSaveNamePrefix = "autosave";

        [SerializeField]
        string saveDir = "Saves";

        [SerializeField]
        bool jsonFormat = true;

        [SerializeField]
        public DataFormat format = DataFormat.JSON;
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
            InvokeRepeating("AutoSave", 3f, 30f);
#endif

            currentSave = new SaveData();
            Debug.LogFormat("current state of curretnSave is... {0}", currentSave);
            saveFiles = new Dictionary<string, SaveData>();
            await UniTask.WaitWhile(() => GameManager.Instance == null);
            LoadSaveFiles().Forget();
            LoadPersistent();
        }

        async UniTask AutoSave()
        {
            // while (true)
            // {
            if (SceneManager.GetActiveScene().name == "Ingame")
            {
                await GameManager.Instance.TakeScreenshot();
                string date =
                    "Autosave: " + System.DateTime.Now.ToString("dddd, MMM dd yyyy, hh:mm");

                var save = new SaveData(
                    DialogueSystemManager.Instance.Story.state.ToJson(),
                    GameManager.Instance.currentScreenshot,
                    date
                );

                save.spriteSaveDatas = ImageManager.Instance.GetAllCharacterOnScreenSaveData();

                // save.currentAmbient1 = AudioManager.Instance.currentAmbient1;
                // save.currentAmbient2 = AudioManager.Instance.currentAmbient2;
                // save.currentAmbient3 = AudioManager.Instance.currentAmbient3;
                save.currentMusic = AudioManager.Instance.GetCurrentPlayingMusic();

                save.currentBGCameraPosition = ImageManager
                    .Instance
                    .CurrentBGCamera
                    .transform
                    .position;
                save.currentBGCameraRotation = ImageManager
                    .Instance
                    .CurrentBGCamera
                    .transform
                    .eulerAngles;
                ;
                save.currentBGSize = ImageManager.Instance.CurrentBGCamera.orthographicSize;
                save.currentShot = ImageManager.Instance.CurrentCameraShot;

                save.dialogueHistory = DialogueSystemManager.Instance.currentSessionDialogueHistory;
                // save.isTinted = ImageManager.Instance.darkTintOn;
                var filePath = autoSaveNamePrefix + extension;
                SaveGame(SaveLoadManager.Instance.CreateSavePath("Saves/" + filePath), save);
                this.saveFiles[autoSaveNamePrefix] = save;
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

            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(s));

            // Debug.Log("our save path is: " + s);
            return s;
        }

        [Sirenix.OdinInspector.Button]
        public async UniTask LoadSaveFiles()
        {
            try
            {
                var path = CreateSavePath(saveDir);
                Debug.Log("Load saves from: " + path);
                string[] fileArray = Directory.GetFiles(path, "*" + extension);
                var taskList = new List<UniTask>();
                for (int i = 0; i < fileArray.Length; i++)
                {
                    Debug.Log(fileArray[i]);
                    taskList.Add(
                        LoadSaveFile(
                            SaveFilePath(Path.GetFileName(fileArray[i]).Split('_')[1].Split('.')[0])
                        )
                    );
                }

                await UniTask.WhenAll(taskList);
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("failed to lod saves??? {0}", e);
            }

            DoneLoadingSaves = true;
            Debug.LogFormat("done loading save files");
        }

        /// <summary>
        /// Helper function so we can load multiple saves simultaneously. Mostly an Android thing
        /// because loading up images is slow (for screenshot)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async UniTask LoadSaveFile(string filePath)
        {
            // try
            // {
            Debug.Log("reading save file..." + filePath);

            // byte[] bytes = File.ReadAllBytes(filePath);

            var save = new SaveData();
            save.Load(filePath, format); // SerializationUtility.DeserializeValue<SaveData>(bytes, DataFormat.JSON);

            Debug.LogFormat("our loaded save object has date: {0}", save.dateTime);

            await UniTask.Yield();
            await UniTask.Yield();

            // var saveData = File.ReadAllText(fileArray[i]);
            // SaveData save = JsonUtility.FromJson<SaveData>(saveData);
            Debug.LogFormat(
                "we have added this save to savefiles with name: {0}",
                Path.GetFileName(filePath)
            );
            saveFiles[Path.GetFileName(filePath).Split('_')[1].Split('.')[0]] = save;
            // }
            // catch (System.Exception e)
            // {
            //     Debug.LogErrorFormat("failed to load save from: {0}, {1}",filePath, e);
            // }
        }

        public async UniTaskVoid LoadGame(string fileIndex)
        {
            Debug.Log("loading game from: " + fileIndex);
            this.currentSave = GetSaveData(fileIndex);

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

        /// <summary>
        /// DELETE
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="save"></param>
        public void SaveGame(string filePath, SaveData save)
        {
            Debug.Log("saving game to: " + filePath);
            save.Save(filePath, format);
            // byte[] bytes = SerializationUtility.SerializeValue(save, DataFormat.JSON);
            // File.WriteAllBytes(filePath, bytes);

            // string saveToJson = JsonUtility.ToJson(save, true);
            // File.WriteAllText(filePath, saveToJson);
        }

        /// <summary>
        /// This is the actually used function.
        /// </summary>
        /// <param name="fileName">name used as key in our saveFiles map</param>
        public void SaveGame(string fileName = "autosave")
        {
            Debug.Log("run save game from slm please");
            string filePath = SaveFilePath(fileName);
            string date = System.DateTime.Now.ToString(
                "dddd, MMM dd yyyy, hh:mm",
                System.Globalization.CultureInfo.CreateSpecificCulture("en-US")
            );

            var save = new SaveData(
                DialogueSystemManager.Instance.Story.state.ToJson(),
                GameManager.Instance.currentScreenshot,
                date
            );

            save.spriteSaveDatas = ImageManager.Instance.GetAllCharacterOnScreenSaveData();

            save.currentMusic = AudioManager.Instance.GetCurrentPlayingMusic();
            save.currentAmbients = AudioManager.Instance.GetCurrentPlayingAmbients();

            save.currentBGCameraPosition = ImageManager.Instance.CurrentBGCamera.transform.position;
            save.currentBGCameraRotation = ImageManager
                .Instance
                .CurrentBGCamera
                .transform
                .eulerAngles;

            save.currentBGSize = ImageManager.Instance.CurrentBGCamera.orthographicSize;
            save.currentShot = ImageManager.Instance.CurrentCameraShot;

            save.dialogueHistory = DialogueSystemManager.Instance.currentSessionDialogueHistory;
            save.currentDialogue = DialogueSystemManager.Instance.CurrentProcessedDialogue;
            save.currentDialogueWindowMode = DialogueSystemManager
                .Instance
                .CurrentDialogueWindow
                .Value;

            save.Save(filePath, format);

            saveFiles[fileName] = save;
        }

        /// <summary>
        /// Unsure if this is needed if we modify the save after it's been Saved...
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="save"></param>
        public void UpdateSaveFile(string fileName, SaveData save)
        {
            saveFiles[fileName] = save;
        }

        public void SaveSettings()
        {
            Debug.Log("save path to save settings to is: " + CreateSavePath("settings.json"));
            Debug.Log("saving settings now");
            byte[] bytes = SerializationUtility.SerializeValue(
                GameManager.Instance.Settings.Save(),
                DataFormat.JSON
            );
            File.WriteAllBytes(CreateSavePath("settings.json"), bytes);
        }

        public void SavePersistent()
        {
            byte[] bytes = SerializationUtility.SerializeValue(
                GameManager.Instance.PersistentGameData.Save(),
                DataFormat.JSON
            );
            File.WriteAllBytes(CreateSavePath("persistent.json"), bytes);
        }

        public void LoadPersistent()
        {
            if (File.Exists(CreateSavePath("persistent.json")))
            {
                byte[] bytes = File.ReadAllBytes(CreateSavePath("persistent.json"));
                var persistent = SerializationUtility.DeserializeValue<PersistentGameDataSaveData>(
                    bytes,
                    DataFormat.JSON
                );
                GameManager.Instance.PersistentGameData.Load(persistent);
                if (GameManager.Instance.PersistentGameData.seenText == null)
                {
                    GameManager.Instance.PersistentGameData.seenText = new HashSet<string>();
                }
                if (GameManager.Instance.PersistentGameData.chosenChoices == null)
                {
                    GameManager.Instance.PersistentGameData.chosenChoices = new HashSet<string>();
                }
            }
            else
            {
                SavePersistent();
            }
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
                var settings = SerializationUtility.DeserializeValue<SettingsSaveData>(
                    bytes,
                    DataFormat.JSON
                );
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

        public void SetCurrentSave(string saveName)
        {
            try
            {
                currentSave = GetSaveData(saveName);
            }
            catch
            {
                Debug.LogErrorFormat("failed to set currentSave to: {0}", saveName);
            }
        }

        public SaveData GetSaveData(string fileIndex)
        {
            fileIndex = fileIndex.Trim();
            if (SaveExists(fileIndex))
            {
                return saveFiles[fileIndex];
            }
            else
            {
                Debug.LogErrorFormat("save {0} doesn't exist in our loaded saves map!!", fileIndex);
                return null;
            }
        }

        public bool SaveExists(string fileIndex)
        {
            fileIndex = fileIndex.Trim();
            // foreach (var key in saveFiles.Keys)
            // {
            //     Debug.Log(key);
            // }
            return saveFiles.ContainsKey(fileIndex.Trim());
        }

        public string SaveFilePath(string saveFileName)
        {
            return CreateSavePath(saveDir) + "/" + saveFileNamePrefix + saveFileName + extension;
        }
    }
}
