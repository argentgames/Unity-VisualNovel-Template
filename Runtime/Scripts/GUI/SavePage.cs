using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UniRx;
using System.IO;
using Cysharp.Threading.Tasks;

// TODO: this should be an abstract class...
namespace com.argentgames.visualnoveltemplate
{


    public class SavePage : MonoBehaviour
    {
        [SerializeField]
        GameObject saveSlotPrefab, saveSlotContentHolder;
        GameObject overwriteSavePanelPrefab;
        [SerializeField]
        int numSaveSlots = 9;
        int currentSelectedSaveSlot = 0;
        CompositeDisposable disposables = new CompositeDisposable();
        // Start is called before the first frame update
        void Start()
        {

            overwriteSavePanelPrefab.SetActive(false);

            overwriteSavePanelPrefab.GetComponent<ConfirmPanel>().no.OnClickAsObservable().Subscribe(val =>
            {
                disposables.Clear();
                overwriteSavePanelPrefab.SetActive(false);
            });
        }

        // Update is called once per frame
        void OnEnable()
        {
            for (int i = 0; i < saveSlotContentHolder.transform.childCount; i++)
            {
                Destroy(saveSlotContentHolder.transform.GetChild(i).gameObject);
            }
            // SaveLoadManager.Instance.LoadSaveFiles();
            CreateSaveSlots();
        }

        void CreateSaveSlot(int i)
        {
            var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
            var saveSlot = go.GetComponent<SaveLoadSlot>();
            // slot text > screenshot > date time
            string filePath = SaveLoadManager.Instance.saveFileNamePrefix + i.ToString() + SaveLoadManager.Instance.extension;
            Debug.Log(filePath);
            var slotIndex = i;
            Debug.LogFormat("saveloadmanager contains save file {0}: {1}", filePath, SaveLoadManager.Instance.saveFiles.ContainsKey(filePath));
            // if a save file already exists, when we click it we want to ask if they want to overwrite
            if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
            {
                var save = SaveLoadManager.Instance.saveFiles[filePath];
                //Read
                // byte[] bytes = save.screenshot;
                //Convert image to texture
                // Texture2D loadTexture = new Texture2D(2, 2);
                // loadTexture.LoadImage(bytes);

                saveSlot.emptyText.text = "";

                saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
                new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

                saveSlot.date.text = save.dateTime;

                go.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    overwriteSavePanelPrefab.SetActive(true);
                // TECHDEBT: stupid hard code get child
                overwriteSavePanelPrefab.GetComponent<ConfirmPanel>().yes.OnClickAsObservable()
            .Subscribe(_ =>
            {
                        currentSelectedSaveSlot = slotIndex;
                        RunSave(go);
                        overwriteSavePanelPrefab.SetActive(false);

                    }).AddTo(disposables);

                }
                )
                .AddTo(this);
            }
            else
            {
                saveSlot.emptyText.text = "Empty Slot";
                saveSlot.date.text = "";
                go.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    currentSelectedSaveSlot = slotIndex;
                    RunSave(go);
                    overwriteSavePanelPrefab.SetActive(false);
                }).AddTo(this);
            }
        }
        void CreateSaveSlots()
        {
            // put auto save slot at the top
            var go = Instantiate(saveSlotPrefab, saveSlotContentHolder.transform);
            var saveSlot = go.GetComponent<SaveLoadSlot>();
            go.GetComponent<Button>().interactable = false;
            string filePath = SaveLoadManager.Instance.autoSaveNamePrefix + SaveLoadManager.Instance.extension;

            // not allowed to save over autosave
            if (SaveLoadManager.Instance.saveFiles.ContainsKey(filePath))
            {
                var save = SaveLoadManager.Instance.saveFiles[filePath];
                saveSlot.emptyText.text = "";

                saveSlot.screenshot.sprite = Sprite.Create(save.screenshot,
                new Rect(0, 0, save.screenshot.width, save.screenshot.height), Vector2.zero);

                saveSlot.date.text = save.dateTime;
            }
            else
            {
                saveSlot.emptyText.text = "Autosave";
                saveSlot.date.text = "";
            }


            for (int i = 0; i < numSaveSlots; i++)
            {
                CreateSaveSlot(i);
            }
        }

        private async UniTaskVoid RunSave(GameObject saveSlot)
        {
            string filePath = SaveLoadManager.Instance.saveFileNamePrefix + currentSelectedSaveSlot.ToString() + SaveLoadManager.Instance.extension;
            Debug.Log("running save for slot: " + filePath);
            var ss = saveSlot.GetComponent<SaveLoadSlot>();
            string date = System.DateTime.Now.ToString("dddd, MMM dd yyyy, hh:mm", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));


            ss.date.text = date;
            ss.emptyText.text = "";

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

            Debug.Log("now waiting for currentScreenshot != null");
            await UniTask.WaitUntil(() => GameManager.Instance.currentScreenshot != null);
            ss.screenshot.sprite = Sprite.Create(GameManager.Instance.currentScreenshot,
            new Rect(0, 0, GameManager.Instance.currentScreenshot.width, GameManager.Instance.currentScreenshot.height), Vector2.zero);

            Debug.Log("now running SaveGame function");
            SaveLoadManager.Instance.SaveGame(SaveLoadManager.Instance.CreateSavePath("Saves/" + filePath), save);
            SaveLoadManager.Instance.saveFiles[filePath] = save;

        }
    }

}